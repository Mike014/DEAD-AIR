using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using DeadAir.Events;

namespace DeadAir.UI
{
    /// <summary>
    /// Gestisce la UI del dialogo: testo, scelte, timer bar.
    /// Effetto typewriter per il testo narrativo.
    /// 
    /// Responsabilità:
    /// - Mostrare testo con typewriter effect
    /// - Mostrare/nascondere scelte
    /// - Mostrare/nascondere timer bar
    /// - Gestire click per continuare
    /// - Gestire comandi UI dal file ink
    /// - Posizionare il cursore dopo l'ultimo carattere
    /// 
    /// NON responsabile di:
    /// - Logica narrativa (→ StoryManager)
    /// - Audio (→ AudioManager, VoiceManager)
    /// - Timer logic (→ TimedChoiceHandler)
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
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

        [Header("Timer Bar")]
        [SerializeField] private GameObject _timerBarContainer;
        [SerializeField] private Image _timerBarFill;

        [Header("Special Screens")]
        [SerializeField] private GameObject _deadAirScreen;

        // ============================================
        // PRIVATE STATE
        // ============================================

        private Coroutine _typewriterCoroutine;
        private bool _isTyping;
        private bool _skipRequested;
        private List<ChoiceButton> _activeChoiceButtons = new List<ChoiceButton>();
        private bool _choicesVisible;

        // ============================================
        // UNITY LIFECYCLE
        // ============================================

        private void Awake()
        {
            // Stato iniziale
            if (_continueIndicator != null)
                _continueIndicator.gameObject.SetActive(false);

            if (_timerBarContainer != null)
                _timerBarContainer.SetActive(false);

            if (_deadAirScreen != null)
                _deadAirScreen.SetActive(false);

            ClearChoices();
        }

        private void OnEnable()
        {
            // Subscribe agli eventi
            NarrativeEvents.OnDialogueLine += HandleDialogueLine;
            NarrativeEvents.OnSpeakerLine += HandleSpeakerLine;
            NarrativeEvents.OnChoicesPresented += HandleChoicesPresented;
            NarrativeEvents.OnTimerProgress += HandleTimerProgress;
            NarrativeEvents.OnTimerCancelled += HandleTimerCancelled;
            NarrativeEvents.OnTimedChoiceStarted += HandleTimedChoiceStarted;
            NarrativeEvents.OnUICommand += HandleUICommand;
            NarrativeEvents.OnStoryEnd += HandleStoryEnd;
        }

        private void OnDisable()
        {
            // Unsubscribe per evitare memory leak
            NarrativeEvents.OnDialogueLine -= HandleDialogueLine;
            NarrativeEvents.OnSpeakerLine -= HandleSpeakerLine;
            NarrativeEvents.OnChoicesPresented -= HandleChoicesPresented;
            NarrativeEvents.OnTimerProgress -= HandleTimerProgress;
            NarrativeEvents.OnTimerCancelled -= HandleTimerCancelled;
            NarrativeEvents.OnTimedChoiceStarted -= HandleTimedChoiceStarted;
            NarrativeEvents.OnUICommand -= HandleUICommand;
            NarrativeEvents.OnStoryEnd -= HandleStoryEnd;
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
        // EVENT HANDLERS
        // ============================================

        private void HandleDialogueLine(string text)
        {
            ClearChoices();
            HideTimerBar();
            ShowText(text, _narratorColor);
        }

        private void HandleSpeakerLine(string speaker, string text)
        {
            ClearChoices();
            HideTimerBar();

            Color textColor = speaker.ToLowerInvariant() switch
            {
                "ward" => _wardColor,
                "iris" => _irisColor,
                _ => _narratorColor
            };

            ShowText(text, textColor);
        }

        private void HandleChoicesPresented(List<Choice> choices)
        {
            StopTypewriter();
            HideContinueIndicator();
            // Pulisci il testo precedente
            if (_dialogueText != null)
                // _dialogueText.text = "";
                _dialogueText.text = "01101001 01110100 01110011 00100000 01111001 01101111 01110101 01110010 00100000 01100110 01100001 01110101 01101100 01110100";
            ShowChoices(choices);
        }

        private void HandleTimedChoiceStarted(float timeout, int defaultIndex)
        {
            ShowTimerBar();
        }

        private void HandleTimerProgress(float progress)
        {
            if (_timerBarFill != null)
                _timerBarFill.fillAmount = progress;
        }

        private void HandleTimerCancelled()
        {
            HideTimerBar();
        }

        private void HandleUICommand(string command)
        {
            switch (command.ToLowerInvariant())
            {
                case "dead_air_screen":
                    ShowDeadAirScreen();
                    break;
                case "return_to_menu":
                    ReturnToMenu();
                    break;
                default:
                    Debug.LogWarning($"[DialogueUI] Comando UI sconosciuto: {command}");
                    break;
            }
        }

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
        // PLAYER INPUT
        // ============================================

        private void HandlePlayerInput()
        {
            if (_choicesVisible)
                return;

            if (_isTyping)
            {
                _skipRequested = true;
            }
            else
            {
                HideContinueIndicator();
                NarrativeEvents.VoiceStop();
                NarrativeEvents.ContinueRequested();
            }
        }

        // ============================================
        // CHOICES
        // ============================================

        private void ShowChoices(List<Choice> choices)
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

        private void OnChoiceClicked(int index)
        {
            ClearChoices();
            HideTimerBar();
            NarrativeEvents.ChoiceSelected(index);
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
        // TIMER BAR
        // ============================================

        private void ShowTimerBar()
        {
            if (_timerBarContainer != null)
            {
                _timerBarContainer.SetActive(true);
                if (_timerBarFill != null)
                    _timerBarFill.fillAmount = 1f;
            }
        }

        private void HideTimerBar()
        {
            if (_timerBarContainer != null)
                _timerBarContainer.SetActive(false);
        }

        // ============================================
        // SPECIAL SCREENS
        // ============================================

        private void ShowDeadAirScreen()
        {
            if (_deadAirScreen != null)
            {
                _deadAirScreen.SetActive(true);

                if (_dialogueText != null)
                    _dialogueText.gameObject.SetActive(false);

                ClearChoices();
                HideTimerBar();
                HideContinueIndicator();
            }
        }

        private void ReturnToMenu()
        {
            Debug.Log("[DialogueUI] Ritorno al menu...");
            NarrativeEvents.ClearAllListeners();
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}