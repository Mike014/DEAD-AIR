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
        [SerializeField] private GameObject _continueIndicator;
        
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
                _continueIndicator.SetActive(false);
            
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
        
        /// <summary>
        /// Linea narrativa senza speaker (descrizioni, pensieri di Ward).
        /// </summary>
        private void HandleDialogueLine(string text)
        {
            ClearChoices();
            HideTimerBar();
            ShowText(text, _narratorColor);
        }
        
        /// <summary>
        /// Linea con speaker identificato.
        /// </summary>
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
        
        /// <summary>
        /// Scelte disponibili per il giocatore.
        /// </summary>
        private void HandleChoicesPresented(List<Choice> choices)
        {
            // Ferma typewriter se ancora in corso
            StopTypewriter();
            
            if (_continueIndicator != null)
                _continueIndicator.SetActive(false);
            
            ShowChoices(choices);
        }
        
        /// <summary>
        /// Una timed choice è iniziata — mostra la timer bar.
        /// </summary>
        private void HandleTimedChoiceStarted(float timeout, int defaultIndex)
        {
            ShowTimerBar();
        }
        
        /// <summary>
        /// Aggiorna il progresso della timer bar.
        /// </summary>
        private void HandleTimerProgress(float progress)
        {
            if (_timerBarFill != null)
            {
                _timerBarFill.fillAmount = progress;
            }
        }
        
        /// <summary>
        /// Il timer è stato cancellato (giocatore ha scelto).
        /// </summary>
        private void HandleTimerCancelled()
        {
            HideTimerBar();
        }
        
        /// <summary>
        /// Comando UI dal file ink.
        /// </summary>
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
        
        /// <summary>
        /// La storia è terminata.
        /// </summary>
        private void HandleStoryEnd()
        {
            Debug.Log("[DialogueUI] Storia terminata.");
        }
        
        // ============================================
        // TEXT DISPLAY
        // ============================================
        
        /// <summary>
        /// Mostra testo con effetto typewriter.
        /// </summary>
        private void ShowText(string text, Color color)
        {
            StopTypewriter();
            
            if (_dialogueText == null)
                return;
            
            _dialogueText.color = color;
            _typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }
        
        /// <summary>
        /// Effetto typewriter — mostra un carattere alla volta.
        /// </summary>
        private IEnumerator TypewriterEffect(string text)
        {
            _isTyping = true;
            _skipRequested = false;
            
            if (_continueIndicator != null)
                _continueIndicator.SetActive(false);
            
            _dialogueText.text = "";
            
            foreach (char c in text)
            {
                if (_skipRequested)
                {
                    // Skip richiesto — mostra tutto il testo
                    _dialogueText.text = text;
                    break;
                }
                
                _dialogueText.text += c;
                
                // Pausa variabile basata sulla punteggiatura
                float delay = _typewriterSpeed;
                if (c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';')
                {
                    delay += _punctuationPause;
                }
                else if (c == '\n')
                {
                    delay += _punctuationPause * 2;
                }
                
                yield return new WaitForSeconds(delay);
            }
            
            _isTyping = false;
            _typewriterCoroutine = null;
            
            // Mostra indicatore "clicca per continuare"
            if (_continueIndicator != null)
                _continueIndicator.SetActive(true);
        }
        
        /// <summary>
        /// Ferma il typewriter corrente.
        /// </summary>
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
        // PLAYER INPUT
        // ============================================
        
        /// <summary>
        /// Gestisce l'input del giocatore (click/spazio).
        /// </summary>
        private void HandlePlayerInput()
        {
            // Se ci sono scelte visibili, ignora (il giocatore deve cliccare una scelta)
            if (_choicesVisible)
                return;
            
            if (_isTyping)
            {
                // Skip typewriter — mostra tutto il testo
                _skipRequested = true;
            }
            else
            {
                // Continua la storia
                if (_continueIndicator != null)
                    _continueIndicator.SetActive(false);
                
                NarrativeEvents.ContinueRequested();
            }
        }
        
        // ============================================
        // CHOICES
        // ============================================
        
        /// <summary>
        /// Mostra le scelte al giocatore.
        /// </summary>
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
        
        /// <summary>
        /// Callback quando una scelta viene cliccata.
        /// </summary>
        private void OnChoiceClicked(int index)
        {
            ClearChoices();
            HideTimerBar();
            NarrativeEvents.ChoiceSelected(index);
        }
        
        /// <summary>
        /// Rimuove tutte le scelte dalla UI.
        /// </summary>
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
        
        /// <summary>
        /// Mostra la schermata "DEAD AIR" finale.
        /// </summary>
        private void ShowDeadAirScreen()
        {
            if (_deadAirScreen != null)
            {
                _deadAirScreen.SetActive(true);
                
                // Nascondi elementi normali
                if (_dialogueText != null)
                    _dialogueText.gameObject.SetActive(false);
                
                ClearChoices();
                HideTimerBar();
            }
        }
        
        /// <summary>
        /// Torna al menu principale.
        /// </summary>
        private void ReturnToMenu()
        {
            Debug.Log("[DialogueUI] Ritorno al menu...");
            
            // Pulizia eventi prima del cambio scena
            NarrativeEvents.ClearAllListeners();
            
            // TODO: Implementare caricamento scena menu
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
