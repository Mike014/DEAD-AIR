using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using DeadAir.Events;
using DeadAir.Narrative;

namespace DeadAir.UI
{
    /// <summary>
    /// Gestisce la UI del dialogo: testo, scelte, timer bar.
    /// Effetto typewriter per il testo narrativo.
    /// Comunica via Event Channels (ScriptableObject).
    /// 
    /// Responsabilità:
    /// - Mostrare testo con typewriter effect
    /// - Mostrare/nascondere scelte
    /// - Gestire click per continuare
    /// - Gestire comandi UI dal file ink
    /// - Posizionare il cursore dopo l'ultimo carattere
    /// 
    /// NON responsabile di:
    /// - Logica narrativa (→ StoryManager)
    /// - Audio (→ AudioManager, VoiceManager)
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS - UI REFERENCES
        // ============================================

        [Header("Text Display")]
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private RectTransform _continueIndicator;

        [Header("Cursor Settings")]
        [SerializeField] private Vector2 _cursorOffset = new Vector2(5f, 0f);

        [Header("Speaker Colors")]
        [SerializeField] private Color _narratorColor = new Color(1f, 0.69f, 0f);      // Ambra #FFB000
        [SerializeField] private Color _wardColor = new Color(1f, 0.69f, 0f);          // Ambra #FFB000
        [SerializeField] private Color _irisColor = new Color(0f, 1f, 0.53f);          // Verde #00FF88

        [Header("Typewriter Settings")]
        [SerializeField] private float _typewriterSpeed = 0.03f;    // Secondi per carattere
        [SerializeField] private float _punctuationPause = 0.15f;   // Pausa extra su . , ! ?

        [Header("Choices")]
        [SerializeField] private Transform _choicesContainer;
        [SerializeField] private ChoiceButton _choiceButtonPrefab;

        [Header("Special Screens")]
        [SerializeField] private GameObject _deadAirScreen;

        // ============================================
        // EVENT CHANNELS (Dependency Injection)
        // ============================================

        [Header("Input Channels (DialogueUI è Observer)")]
        [SerializeField] private StringEventChannel dialogueLineChannel;
        [SerializeField] private StringStringEventChannel speakerLineChannel;
        [SerializeField] private ChoiceListEventChannel choicesPresentedChannel;
        [SerializeField] private StringEventChannel uiCommandChannel;
        [SerializeField] private VoidEventChannel storyEndChannel;

        [Header("Output Channels (DialogueUI è Publisher)")]
        [SerializeField] private VoidEventChannel continueRequestedChannel;
        [SerializeField] private IntEventChannel choiceSelectedChannel;
        [SerializeField] private VoidEventChannel voiceStopChannel;

        // ============================================
        // PRIVATE STATE
        // ============================================

        private Coroutine _typewriterCoroutine;
        private bool _isTyping;
        private bool _skipRequested;
        private List<ChoiceButton> _activeChoiceButtons = new List<ChoiceButton>();
        private bool _choicesVisible;
        private string _lastDialogueLine = "";

        // ============================================
        // UNITY LIFECYCLE
        // ============================================

        private void Awake()
        {
            // Stato iniziale
            if (_continueIndicator != null)
                _continueIndicator.gameObject.SetActive(false);

            if (_deadAirScreen != null)
                _deadAirScreen.SetActive(false);

            ClearChoices();
        }

        private void OnEnable()
        {
            // Subscribe agli eventi via channels (DialogueUI è Observer)
            if (dialogueLineChannel != null)
                dialogueLineChannel.Subscribe(HandleDialogueLine);

            if (speakerLineChannel != null)
                speakerLineChannel.Subscribe(HandleSpeakerLine);

            if (choicesPresentedChannel != null)
                choicesPresentedChannel.Subscribe(HandleChoicesPresented);

            if (uiCommandChannel != null)
                uiCommandChannel.Subscribe(HandleUICommand);

            if (storyEndChannel != null)
                storyEndChannel.Subscribe(HandleStoryEnd);
        }

        private void OnDisable()
        {
            // Unsubscribe per evitare memory leak - CRITICO
            if (dialogueLineChannel != null)
                dialogueLineChannel.Unsubscribe(HandleDialogueLine);

            if (speakerLineChannel != null)
                speakerLineChannel.Unsubscribe(HandleSpeakerLine);

            if (choicesPresentedChannel != null)
                choicesPresentedChannel.Unsubscribe(HandleChoicesPresented);

            if (uiCommandChannel != null)
                uiCommandChannel.Unsubscribe(HandleUICommand);

            if (storyEndChannel != null)
                storyEndChannel.Unsubscribe(HandleStoryEnd);
        }

        private void Update()
        {
            // Input per continuare o skippare typewriter
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                HandlePlayerInput();
            }
        }

        // ============================================
        // EVENT HANDLERS (Observer Reactions)
        // ============================================

        /// <summary>
        /// Chiamato quando StoryManager pubblica evento DialogueLine.
        /// </summary>
        private void HandleDialogueLine(string text)
        {
            ClearChoices();
            ShowText(text, _narratorColor);
        }

        /// <summary>
        /// Chiamato quando StoryManager pubblica evento SpeakerLine.
        /// PROBLEMA: Il canale usa ancora (string speaker, string text).
        /// 
        /// SOLUZIONE TEMPORANEA: Parse string → SpeakerType enum qui.
        /// SOLUZIONE IDEALE (futuro): Canale usa direttamente SpeakerType.
        /// </summary>
        private void HandleSpeakerLine(string speaker, string text)
        {
            ClearChoices();

            // Parse string → SpeakerType enum
            SpeakerType speakerType = speaker?.ToLowerInvariant() switch
            {
                "ward" => SpeakerType.Ward,
                "iris" => SpeakerType.Iris,
                _ => SpeakerType.Unknown
            };

            // Switch type-safe su enum
            Color textColor = speakerType switch
            {
                SpeakerType.Ward => _wardColor,
                SpeakerType.Iris => _irisColor,
                SpeakerType.Narrator => _narratorColor,
                _ => _narratorColor  // Unknown/default
            };

            ShowText(text, textColor);
        }

        /// <summary>
        /// Chiamato quando StoryManager pubblica evento ChoicesPresented.
        /// Nota: firma cambiata da List<Choice> a IReadOnlyList<Choice>
        /// </summary>
        private void HandleChoicesPresented(IReadOnlyList<Choice> choices)
        {
            Debug.Log($"[DialogueUI] HandleChoicesPresented - Testo corrente: '{_dialogueText.text}'");

            StopTypewriter();
            HideContinueIndicator();

            // Pulisci il testo precedente (opzionale - easter egg binario)
            // if (_dialogueText != null)
            //     _dialogueText.text = "01101001 01110100 01110011 00100000 01111001 01101111 01110101 01110010 00100000 01100110 01100001 01110101 01101100 01110100";
            if (_dialogueText != null)
            {
                _dialogueText.text = _lastDialogueLine;
            }

            ShowChoices(choices);

            Debug.Log($"[DialogueUI] Dopo ShowChoices - Testo corrente: '{_dialogueText.text}'");
        }

        /// <summary>
        /// Chiamato quando StoryManager pubblica comando UI.
        /// PROBLEMA: Il canale usa ancora string invece di UICommandType.
        /// 
        /// SOLUZIONE TEMPORANEA: Parse string → UICommandType enum qui.
        /// SOLUZIONE IDEALE (futuro): Canale usa direttamente UICommandType.
        /// </summary>
        private void HandleUICommand(string command)
        {
            // Parse string → UICommandType enum
            UICommandType commandType = command?.ToLowerInvariant() switch
            {
                "dead_air_screen" => UICommandType.DeadAirScreen,
                "return_to_menu" => UICommandType.ReturnToMenu,
                _ => UICommandType.None
            };

            // Switch type-safe su enum (exhaustive)
            switch (commandType)
            {
                case UICommandType.DeadAirScreen:
                    ShowDeadAirScreen();
                    break;

                case UICommandType.ReturnToMenu:
                    QuitApplication();
                    break;

                case UICommandType.None:
                    Debug.LogWarning($"[DialogueUI] Comando UI sconosciuto: {command}");
                    break;

                default:
                    // Questo caso non dovrebbe mai verificarsi se l'enum è completo
                    Debug.LogError($"[DialogueUI] UICommandType non gestito: {commandType}");
                    break;
            }
        }

        /// <summary>
        /// Chiamato quando StoryManager pubblica evento StoryEnd.
        /// </summary>
        private void HandleStoryEnd()
        {
            Debug.Log("[DialogueUI] Storia terminata.");
        }

        // ============================================
        // TEXT DISPLAY
        // ============================================

        private void ShowText(string text, Color color)
        {
            StopTypewriter();

            if (_dialogueText == null)
                return;

            _dialogueText.color = color;
            _lastDialogueLine = text;

            _typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }

        private IEnumerator TypewriterEffect(string text)
        {
            _isTyping = true;
            _skipRequested = false;

            HideContinueIndicator();

            _dialogueText.text = "";

            foreach (char c in text)
            {
                if (_skipRequested)
                {
                    _dialogueText.text = text;
                    break;
                }

                _dialogueText.text += c;

                float delay = _typewriterSpeed;
                if (c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';')
                    delay += _punctuationPause;
                else if (c == '\n')
                    delay += _punctuationPause * 2;

                yield return new WaitForSeconds(delay);
            }

            _isTyping = false;
            _typewriterCoroutine = null;

            // Posiziona e mostra il cursore dopo l'ultimo carattere
            yield return null; // Aspetta un frame per l'update del mesh
            ShowContinueIndicator();
        }

        private void StopTypewriter()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }
            _isTyping = false;
        }

        // ============================================
        // CURSOR POSITIONING
        // ============================================

        /// <summary>
        /// Mostra il cursore posizionandolo dopo l'ultimo carattere.
        /// Usa TMP_TextInfo per calcolare la posizione esatta.
        /// </summary>
        private void ShowContinueIndicator()
        {
            if (_continueIndicator == null || _dialogueText == null)
                return;

            // TRADE-OFF PERFORMANCE: ForceMeshUpdate rigenera l'intera geometria del testo.
            // Chiamarlo una tantum a fine frase è corretto, ma è un anti-pattern se inserito
            // all'interno di un Update() o chiamato ad ogni frame.
            _dialogueText.ForceMeshUpdate();

            TMP_TextInfo textInfo = _dialogueText.textInfo;

            if (textInfo.characterCount == 0)
            {
                _continueIndicator.gameObject.SetActive(false);
                return;
            }

            int lastCharIndex = textInfo.characterCount - 1;

            // Troviamo l'ultimo carattere visibile per calcolare la X orizzontale
            while (lastCharIndex >= 0 && !textInfo.characterInfo[lastCharIndex].isVisible)
            {
                lastCharIndex--;
            }

            if (lastCharIndex < 0)
            {
                _continueIndicator.gameObject.SetActive(false);
                return;
            }

            // 1. Estraiamo i dati del carattere (per la coordinata X)
            TMP_CharacterInfo charInfo = textInfo.characterInfo[lastCharIndex];

            // 2. Estraiamo i dati dell'INTERA RIGA (per la coordinata Y)
            TMP_LineInfo lineInfo = textInfo.lineInfo[charInfo.lineNumber];

            // PERCHÉ xAdvance e baseline?
            // xAdvance: è la posizione calcolata da TMP dove dovrebbe iniziare il carattere successivo.
            // baseline: è la linea immaginaria su cui poggiano tutte le lettere, garantendo un'altezza Y costante.
            Vector3 stableLocalPos = new Vector3(charInfo.xAdvance, lineInfo.baseline, 0f);

            // Trasformazioni spaziali per disaccoppiare la gerarchia UI
            Vector3 worldPos = _dialogueText.transform.TransformPoint(stableLocalPos);
            Vector3 finalLocalPos = _continueIndicator.parent.InverseTransformPoint(worldPos);

            _continueIndicator.localPosition = finalLocalPos + (Vector3)_cursorOffset;
            _continueIndicator.gameObject.SetActive(true);
        }

        private void HideContinueIndicator()
        {
            if (_continueIndicator != null)
                _continueIndicator.gameObject.SetActive(false);
        }

        // ============================================
        // PLAYER INPUT (DialogueUI è Publisher)
        // ============================================

        /// <summary>
        /// Gestisce input player: skip typewriter o pubblica ContinueRequested.
        /// DialogueUI agisce come Publisher verso StoryManager.
        /// </summary>
        private void HandlePlayerInput()
        {
            if (_choicesVisible)
                return;

            if (_isTyping)
            {
                // Skip typewriter
                _skipRequested = true;
            }
            else
            {
                // Pubblica evento: player vuole continuare
                HideContinueIndicator();

                // Stop voice clip in corso
                if (voiceStopChannel != null)
                    voiceStopChannel.RaiseEvent();

                // Richiesta continue a StoryManager
                if (continueRequestedChannel != null)
                    continueRequestedChannel.RaiseEvent();
            }
        }

        // ============================================
        // CHOICES
        // ============================================

        /// <summary>
        /// Mostra bottoni di scelta.
        /// Nota: firma cambiata per accettare IReadOnlyList<Choice>
        /// </summary>
        private void ShowChoices(IReadOnlyList<Choice> choices)
        {
            ClearChoices();

            if (_choicesContainer == null || _choiceButtonPrefab == null)
            {
                Debug.LogWarning("[DialogueUI] ChoicesContainer o ChoiceButtonPrefab non assegnati!");
                return;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                ChoiceButton button = Instantiate(_choiceButtonPrefab, _choicesContainer);
                button.Setup(i, choices[i].text, OnChoiceClicked);
                _activeChoiceButtons.Add(button);
            }

            _choicesVisible = true;
        }

        /// <summary>
        /// Callback da ChoiceButton: pubblica evento ChoiceSelected.
        /// DialogueUI agisce come Publisher verso StoryManager.
        /// </summary>
        private void OnChoiceClicked(int index)
        {
            ClearChoices();

            // Pubblica evento: player ha scelto
            if (choiceSelectedChannel != null)
                choiceSelectedChannel.RaiseEvent(index);
        }

        private void ClearChoices()
        {
            foreach (var button in _activeChoiceButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _activeChoiceButtons.Clear();
            _choicesVisible = false;
        }

        // ============================================
        // SPECIAL SCREENS
        // ============================================

        private void ShowDeadAirScreen()
        {
            if (_deadAirScreen != null)
            {
                // FERMA TUTTO immediatamente
                StopTypewriter();

                // NASCONDI TUTTO L'UI DEL DIALOGO
                HideContinueIndicator();
                ClearChoices();

                // Nasconde anche il testo
                if (_dialogueText != null)
                    _dialogueText.gameObject.SetActive(false);

                // MOSTRA DEAD AIR SCREEN
                _deadAirScreen.SetActive(true);

                // DISABILITA INPUT (importante!)
                _choicesVisible = true; // Trick per bloccare HandlePlayerInput

                Debug.Log("[DialogueUI] Dead Air screen attivo - UI nascosta");
            }
        }

        private void ReturnToMenu()
        {
            Debug.Log("[DialogueUI] Ritorno al menu...");

            // NOTA: ClearAllListeners non esiste più in architettura channel-based
            // Unsubscribe viene gestito automaticamente da OnDisable()

            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void QuitApplication()
        {
            Debug.Log("[DialogueUI] Chiusura applicazione...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}