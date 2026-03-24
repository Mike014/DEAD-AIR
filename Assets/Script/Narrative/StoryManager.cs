using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using DeadAir.Events;

namespace DeadAir.Narrative
{
    /// <summary>
    /// Cervello narrativo. Orchestra il runtime ink e comunica via eventi.
    /// 
    /// Responsabilità:
    /// - Caricare e gestire la Story ink
    /// - Processare linee e tag
    /// - Dispatchare eventi ai sistemi (Audio, UI, Timer)
    /// - Gestire il flusso Continue/Choice
    /// 
    /// NON responsabile di:
    /// - Rendering UI (→ DialogueUI)
    /// - Riproduzione audio (→ AudioManager)
    /// - Gestione timer (→ TimedChoiceHandler)
    /// </summary>
    public class StoryManager : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================
        
        [Header("Ink Story")]
        [SerializeField] private TextAsset _inkAsset;
        
        [Header("Debug")]
        [SerializeField] private bool _logToConsole = true;
        
        // ============================================
        // PRIVATE STATE
        // ============================================
        
        private Story _story;
        private bool _isInitialized;
        private bool _waitingForInput;      // Aspetta click per continuare
        private bool _waitingForChoice;     // Aspetta selezione scelta
        
        // Cache per evitare allocazioni ripetute
        private List<Choice> _currentChoices = new List<Choice>();
        
        // ============================================
        // UNITY LIFECYCLE
        // ============================================
        
        private void Awake()
        {
            InitializeStory();
        }
        
        private void OnEnable()
        {
            // Iscriviti agli eventi
            NarrativeEvents.OnContinueRequested += HandleContinueRequested;
            NarrativeEvents.OnChoiceSelected += HandleChoiceSelected;
            NarrativeEvents.OnTimedChoiceExpired += HandleTimedChoiceExpired;
        }
        
        private void OnDisable()
        {
            // Disiscrivi per evitare memory leak
            NarrativeEvents.OnContinueRequested -= HandleContinueRequested;
            NarrativeEvents.OnChoiceSelected -= HandleChoiceSelected;
            NarrativeEvents.OnTimedChoiceExpired -= HandleTimedChoiceExpired;
        }
        
        private void Start()
        {
            // Avvia la storia
            if (_isInitialized)
            {
                ContinueStory();
            }
        }
        
        // ============================================
        // INITIALIZATION
        // ============================================
        
        private void InitializeStory()
        {
            if (_inkAsset == null)
            {
                Debug.LogError("[StoryManager] Ink asset non assegnato!");
                return;
            }
            
            _story = new Story(_inkAsset.text);
            
            // Error handler per errori ink runtime
            _story.onError += HandleInkError;
            
            _isInitialized = true;
            
            Log("Story inizializzata.");
        }
        
        // ============================================
        // STORY FLOW
        // ============================================
        
        /// <summary>
        /// Continua la storia fino alla prossima pausa (scelta o fine linea).
        /// Processa UNA linea alla volta per permettere effetto typewriter.
        /// </summary>
        private void ContinueStory()
        {
            if (!_isInitialized || _story == null)
                return;
            
            // Se possiamo continuare, processa la prossima linea
            if (_story.canContinue)
            {
                string text = _story.Continue();
                List<string> tags = _story.currentTags;
                
                ProcessLine(text, tags);
                
                // Dopo aver processato, controlla se ci sono altre linee
                // o se dobbiamo aspettare input
                if (_story.canContinue)
                {
                    // Ci sono altre linee — aspetta click per continuare
                    _waitingForInput = true;
                    _waitingForChoice = false;
                }
                else if (_story.currentChoices.Count > 0)
                {
                    // Ci sono scelte — presentale
                    PresentChoices();
                }
                else
                {
                    // Fine storia
                    HandleStoryEnd();
                }
            }
            else if (_story.currentChoices.Count > 0)
            {
                // Non può continuare ma ci sono scelte
                PresentChoices();
            }
            else
            {
                // Fine storia
                HandleStoryEnd();
            }
        }
        
        /// <summary>
        /// Processa una singola linea: parsing tag e dispatch eventi.
        /// </summary>
        private void ProcessLine(string text, List<string> tags)
        {
            // Parsing
            var parsed = DialogueParser.ParseTags(tags, text);
            
            Log($"Line: \"{parsed.Text}\" | Speaker: {parsed.Speaker ?? "none"} | Tags: {tags?.Count ?? 0}");
            
            // === AUDIO EVENTS ===
            
            // SFX
            if (parsed.HasSFX)
            {
                NarrativeEvents.SFXRequested(parsed.SFX);
                Log($"  → SFX: {parsed.SFX}");
            }
            
            // Ambience
            if (parsed.HasAmbience)
            {
                if (parsed.IsAmbienceStop)
                {
                    NarrativeEvents.AmbienceStop();
                    Log("  → Ambience STOP");
                }
                else
                {
                    NarrativeEvents.AmbienceStart(parsed.Ambience);
                    Log($"  → Ambience: {parsed.Ambience}");
                }
            }
            
            // === UI EVENTS ===
            
            if (parsed.HasUICommand)
            {
                NarrativeEvents.UICommand(parsed.UICommand);
                Log($"  → UI Command: {parsed.UICommand}");
            }
            
            // === DIALOGUE EVENTS ===
            
            // Salta linee vuote (solo tag, niente testo)
            if (string.IsNullOrWhiteSpace(parsed.Text))
            {
                // Se c'erano solo tag, continua automaticamente
                _waitingForInput = false;
                ContinueStory();
                return;
            }
            
            // Dispatch linea di dialogo
            if (parsed.HasSpeaker)
            {
                NarrativeEvents.SpeakerLine(parsed.Speaker, parsed.Text);
            }
            else
            {
                NarrativeEvents.DialogueLine(parsed.Text);
            }
        }
        
        /// <summary>
        /// Presenta le scelte al giocatore.
        /// Controlla anche se è una timed choice.
        /// </summary>
        private void PresentChoices()
        {
            _currentChoices.Clear();
            _currentChoices.AddRange(_story.currentChoices);
            
            _waitingForInput = false;
            _waitingForChoice = true;
            
            Log($"Presentando {_currentChoices.Count} scelte.");
            
            // Controlla se è una timed choice
            // I tag della timed choice sono sulla prima scelta per convenzione
            if (_currentChoices.Count > 0 && _currentChoices[0].tags != null)
            {
                var timedData = DialogueParser.ParseTimedChoiceTags(_currentChoices[0].tags);
                
                if (timedData.IsTimedChoice)
                {
                    Log($"  → TIMED CHOICE: {timedData.Timeout}s, default index: {timedData.DefaultIndex}");
                    NarrativeEvents.TimedChoiceStarted(timedData.Timeout, timedData.DefaultIndex);
                }
            }
            
            // Dispatch evento scelte
            NarrativeEvents.ChoicesPresented(_currentChoices);
        }
        
        // ============================================
        // EVENT HANDLERS
        // ============================================
        
        /// <summary>
        /// Chiamato quando il giocatore clicca per continuare.
        /// </summary>
        private void HandleContinueRequested()
        {
            if (_waitingForInput)
            {
                _waitingForInput = false;
                ContinueStory();
            }
        }
        
        /// <summary>
        /// Chiamato quando il giocatore seleziona una scelta.
        /// </summary>
        private void HandleChoiceSelected(int index)
        {
            if (!_waitingForChoice)
                return;
            
            if (index < 0 || index >= _story.currentChoices.Count)
            {
                Debug.LogError($"[StoryManager] Indice scelta non valido: {index}");
                return;
            }
            
            Log($"Scelta selezionata: {index} - \"{_story.currentChoices[index].text}\"");
            
            _waitingForChoice = false;
            _story.ChooseChoiceIndex(index);
            
            ContinueStory();
        }
        
        /// <summary>
        /// Chiamato quando il timer di una timed choice scade.
        /// Il TimedChoiceHandler avrà già selezionato l'indice default.
        /// </summary>
        private void HandleTimedChoiceExpired()
        {
            Log("Timed choice scaduta — gestita da TimedChoiceHandler.");
            // Il TimedChoiceHandler chiama NarrativeEvents.ChoiceSelected(defaultIndex)
            // che triggera HandleChoiceSelected
        }
        
        /// <summary>
        /// Chiamato quando la storia raggiunge -> END.
        /// </summary>
        private void HandleStoryEnd()
        {
            Log("=== FINE STORIA ===");
            NarrativeEvents.StoryEnd();
        }
        
        /// <summary>
        /// Handler errori ink runtime.
        /// </summary>
        private void HandleInkError(string message, Ink.ErrorType type)
        {
            if (type == Ink.ErrorType.Warning)
                Debug.LogWarning($"[Ink Warning] {message}");
            else
                Debug.LogError($"[Ink Error] {message}");
        }
        
        // ============================================
        // PUBLIC API (per debug/testing)
        // ============================================
        
        /// <summary>
        /// Salta a un knot specifico. Utile per testing.
        /// </summary>
        public void JumpToKnot(string knotName)
        {
            if (!_isInitialized) return;
            
            _story.ChoosePathString(knotName);
            _waitingForInput = false;
            _waitingForChoice = false;
            ContinueStory();
        }
        
        /// <summary>
        /// Restituisce il valore di una variabile ink.
        /// </summary>
        public T GetVariable<T>(string variableName)
        {
            if (!_isInitialized) return default;
            return (T)_story.variablesState[variableName];
        }
        
        /// <summary>
        /// Imposta il valore di una variabile ink.
        /// </summary>
        public void SetVariable(string variableName, object value)
        {
            if (!_isInitialized) return;
            _story.variablesState[variableName] = value;
        }
        
        // ============================================
        // UTILITY
        // ============================================
        
        private void Log(string message)
        {
            if (_logToConsole)
                Debug.Log($"[StoryManager] {message}");
        }
    }
}
