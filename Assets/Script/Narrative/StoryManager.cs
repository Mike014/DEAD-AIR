using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using DeadAir.Events;

namespace DeadAir.Narrative
{
    /// <summary>
    /// Cervello narrativo. Orchestra il runtime ink e comunica via Event Channels.
    /// 
    /// Responsabilità:
    /// - Caricare e gestire la Story ink
    /// - Processare linee e tag
    /// - Dispatchare eventi ai sistemi (Audio, UI, Voice) via ScriptableObject Channels
    /// - Gestire il flusso Continue/Choice
    /// 
    /// NON responsabile di:
    /// - Rendering UI (→ DialogueUI)
    /// - Riproduzione audio (→ AudioManager, VoiceManager)
    /// </summary>
    public class StoryManager : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================

        [Header("Ink Story")]
        [SerializeField] private TextAsset _inkAsset;

        // ============================================
        // EVENT CHANNELS (Dependency Injection)
        // ============================================

        [Header("Dialogue Events")]
        [SerializeField] private StringEventChannel dialogueLineChannel;
        [SerializeField] private StringStringEventChannel speakerLineChannel;
        [SerializeField] private ChoiceListEventChannel choicesPresentedChannel;

        [Header("Audio Events")]
        [SerializeField] private StringEventChannel sfxRequestedChannel;
        [SerializeField] private StringEventChannel ambienceStartChannel;
        [SerializeField] private VoidEventChannel ambienceStopChannel;
        [SerializeField] private StringEventChannel voiceRequestedChannel;
        [SerializeField] private VoidEventChannel voiceStopChannel;

        [Header("UI Events")]
        [SerializeField] private StringEventChannel uiCommandChannel;

        [Header("Story Flow Events")]
        [SerializeField] private VoidEventChannel storyEndChannel;

        [Header("Voice Playback Events")]
        [SerializeField] private FloatEventChannel voiceStartedChannel;
        [SerializeField] private VoidEventChannel voiceFinishedChannel;

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
            // Subscribe agli eventi usando i channels iniettati
            if (continueRequestedChannel != null)
                continueRequestedChannel.Subscribe(HandleContinueRequested);
            
            if (choiceSelectedChannel != null)
                choiceSelectedChannel.Subscribe(HandleChoiceSelected);
        }

        private void OnDisable()
        {
            // Unsubscribe per prevenire memory leak
            if (continueRequestedChannel != null)
                continueRequestedChannel.Unsubscribe(HandleContinueRequested);
            
            if (choiceSelectedChannel != null)
                choiceSelectedChannel.Unsubscribe(HandleChoiceSelected);
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
        /// Processa una singola linea: dispatcha eventi audio/UI via channels.
        /// Restituisce true se la linea ha testo visibile da mostrare al giocatore.
        /// </summary>
        private bool ProcessLine(string text, List<string> tags)
        {
            // Pulizia aggressiva della stringa nativa di Ink
            // Ink spesso restituisce newline residui (\n) dopo aver fatto una scelta
            string cleanText = text.Trim();

            // Se la stringa pulita è vuota, ignora completamente il rendering UI
            if (string.IsNullOrEmpty(cleanText) && (tags == null || tags.Count == 0))
            {
                return false;
            }

            var parsed = DialogueParser.ParseTags(tags, cleanText);

            Log($"Line: \"{parsed.Text}\" | Speaker: {parsed.Speaker ?? "none"} | Voice: {parsed.VoiceId ?? "none"} | Tags: {tags?.Count ?? 0}");

            // === AUDIO EVENTS (via Channels) ===

            if (parsed.HasSFX && sfxRequestedChannel != null)
            {
                sfxRequestedChannel.RaiseEvent(parsed.SFX);
                Log($"  → SFX: {parsed.SFX}");
            }

            if (parsed.HasAmbience)
            {
                if (parsed.IsAmbienceStop && ambienceStopChannel != null)
                {
                    ambienceStopChannel.RaiseEvent();
                    Log("  → Ambience STOP");
                }
                else if (ambienceStartChannel != null)
                {
                    ambienceStartChannel.RaiseEvent(parsed.Ambience);
                    Log($"  → Ambience: {parsed.Ambience}");
                }
            }

            if (parsed.HasVoice && voiceRequestedChannel != null)
            {
                voiceRequestedChannel.RaiseEvent(parsed.VoiceId);
                Log($"  → Voice: {parsed.VoiceId}");
            }

            // === UI EVENTS (via Channels) ===

            if (parsed.HasUICommand && uiCommandChannel != null)
            {
                uiCommandChannel.RaiseEvent(parsed.UICommand);
                Log($"  → UI Command: {parsed.UICommand}");
            }

            // === DIALOGUE EVENTS (via Channels) ===

            if (string.IsNullOrWhiteSpace(parsed.Text))
                return false; // Riga solo-tag: nessun testo, il loop continua

            if (parsed.HasSpeaker && speakerLineChannel != null)
            {
                speakerLineChannel.RaiseEvent(parsed.Speaker, parsed.Text);
            }
            else if (dialogueLineChannel != null)
            {
                dialogueLineChannel.RaiseEvent(parsed.Text);
            }

            return true;
        }

        /// <summary>
        /// Presenta le scelte al giocatore.
        /// </summary>
        private void PresentChoices()
        {
            _currentChoices.Clear();
            _currentChoices.AddRange(_story.currentChoices);

            _waitingForInput = false;
            _waitingForChoice = true;

            Log($"Presentando {_currentChoices.Count} scelte.");

            // Notifica le scelte via channel
            if (choicesPresentedChannel != null)
            {
                // IReadOnlyList upcast da List automatico
                choicesPresentedChannel.RaiseEvent(_currentChoices);
            }
        }

        // ============================================
        // EVENT HANDLERS
        // ============================================

        /// <summary>
        /// Chiamato quando il giocatore clicca per continuare.
        /// Subscribed a continueRequestedChannel in OnEnable.
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
        /// Subscribed a choiceSelectedChannel in OnEnable.
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
            
            if (storyEndChannel != null)
                storyEndChannel.RaiseEvent();
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

        // ============================================
        // CHANNELS MANCANTI (Aggiunti sopra come SerializeFields)
        // ============================================
        
        [Header("Input Events (StoryManager è anche Observer)")]
        [SerializeField] private VoidEventChannel continueRequestedChannel;
        [SerializeField] private IntEventChannel choiceSelectedChannel;
    }
}