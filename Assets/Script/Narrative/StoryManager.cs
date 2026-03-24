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
    /// - Dispatchare eventi ai sistemi (Audio, UI, Timer, Voice)
    /// - Gestire il flusso Continue/Choice
    /// 
    /// NON responsabile di:
    /// - Rendering UI (→ DialogueUI)
    /// - Riproduzione audio (→ AudioManager, VoiceManager)
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
            NarrativeEvents.OnContinueRequested += HandleContinueRequested;
            NarrativeEvents.OnChoiceSelected += HandleChoiceSelected;
        }

        private void OnDisable()
        {
            NarrativeEvents.OnContinueRequested -= HandleContinueRequested;
            NarrativeEvents.OnChoiceSelected -= HandleChoiceSelected;
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

            try
            {
                _story = new Story(_inkAsset.text);
                _story.onError += HandleInkError;
                _isInitialized = true;
                Log("Story inizializzata.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StoryManager] Errore inizializzazione Story: {e.Message}");
            }
        }

        // ============================================
        // STORY FLOW
        // ============================================

        /// <summary>
        /// Continua la storia fino alla prossima linea con testo visibile.
        /// Le linee con soli tag (audio, UI, ambience) vengono consumate in loop
        /// senza ricorsione — evita stack overflow su sequenze di tag consecutivi.
        /// </summary>
        private void ContinueStory()
        {
            if (!_isInitialized || _story == null)
                return;

            // Itera sulle linee ink: processa tag e si ferma solo su testo visibile
            while (_story.canContinue)
            {
                string text = _story.Continue();
                bool hasVisibleText = ProcessLine(text, _story.currentTags);

                if (hasVisibleText)
                {
                    if (_story.canContinue)
                    {
                        _waitingForInput = true;
                        _waitingForChoice = false;
                    }
                    else if (_story.currentChoices.Count > 0)
                    {
                        PresentChoices();
                    }
                    else
                    {
                        HandleStoryEnd();
                    }
                    return; // Aspetta che il giocatore agisca
                }
                // Nessun testo: la riga aveva solo tag → loop continua
            }

            // While esaurito senza testo visibile (es. fine blocco solo-tag)
            if (_story.currentChoices.Count > 0)
                PresentChoices();
            else
                HandleStoryEnd();
        }

        /// <summary>
        /// Processa una singola linea: dispatcha eventi audio/UI.
        /// Restituisce true se la linea ha testo visibile da mostrare al giocatore.
        /// </summary>
        private bool ProcessLine(string text, List<string> tags)
        {
            // FIX 1: Pulizia aggressiva della stringa nativa di Ink
            // Ink spesso restituisce newline residui (\n) dopo aver fatto una scelta.
            string cleanText = text.Trim();

            // Se la stringa pulita è vuota, ignora completamente il rendering UI
            if (string.IsNullOrEmpty(cleanText) && (tags == null || tags.Count == 0))
            {
                return false;
            }

            var parsed = DialogueParser.ParseTags(tags, cleanText);

            Log($"Line: \"{parsed.Text}\" | Speaker: {parsed.Speaker ?? "none"} | Voice: {parsed.VoiceId ?? "none"} | Tags: {tags?.Count ?? 0}");

            // === AUDIO EVENTS ===

            if (parsed.HasSFX)
            {
                NarrativeEvents.SFXRequested(parsed.SFX);
                Log($"  → SFX: {parsed.SFX}");
            }

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

            if (parsed.HasVoice)
            {
                NarrativeEvents.VoiceRequested(parsed.VoiceId);
                Log($"  → Voice: {parsed.VoiceId}");
            }

            // === UI EVENTS ===

            if (parsed.HasUICommand)
            {
                NarrativeEvents.UICommand(parsed.UICommand);
                Log($"  → UI Command: {parsed.UICommand}");
            }

            // === DIALOGUE EVENTS ===

            if (string.IsNullOrWhiteSpace(parsed.Text))
                return false; // Riga solo-tag: nessun testo, il loop continua

            if (parsed.HasSpeaker)
                NarrativeEvents.SpeakerLine(parsed.Speaker, parsed.Text);
            else
                NarrativeEvents.DialogueLine(parsed.Text);

            return true;
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

            // Dispatch evento scelte — passa una copia per evitare mutazione esterna
            NarrativeEvents.ChoicesPresented(new List<Choice>(_currentChoices));
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
            try
            {
                return (T)_story.variablesState[variableName];
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StoryManager] GetVariable<{typeof(T).Name}>('{variableName}') failed: {e.Message}");
                return default;
            }
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
