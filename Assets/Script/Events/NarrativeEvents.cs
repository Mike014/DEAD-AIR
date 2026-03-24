using System;
using System.Collections.Generic;
using Ink.Runtime;

namespace DeadAir.Events
{
    /// <summary>
    /// Static Event Hub per la comunicazione disaccoppiata tra sistemi.
    /// Nessun sistema conosce gli altri — tutti comunicano attraverso eventi.
    /// 
    /// Pattern: Observer via C# Events
    /// Perché static: Accesso globale senza Singleton MonoBehaviour, 
    /// zero allocazioni per lookup, ideale per eventi frequenti.
    /// </summary>
    public static class NarrativeEvents
    {
        // ============================================
        // DIALOGUE EVENTS
        // ============================================
        
        /// <summary>
        /// Linea di testo narrativo senza speaker (pensieri di Ward, descrizioni).
        /// </summary>
        public static event Action<string> OnDialogueLine;
        
        /// <summary>
        /// Linea con speaker identificato (Ward o Iris).
        /// speaker: "ward" o "iris"
        /// text: il contenuto della linea
        /// </summary>
        public static event Action<string, string> OnSpeakerLine;
        
        /// <summary>
        /// Scelte disponibili per il giocatore.
        /// </summary>
        public static event Action<List<Choice>> OnChoicesPresented;
        
        /// <summary>
        /// Il giocatore ha selezionato una scelta.
        /// </summary>
        public static event Action<int> OnChoiceSelected;
        
        // ============================================
        // TIMED CHOICE EVENTS
        // ============================================
        
        /// <summary>
        /// Una scelta con timer è iniziata.
        /// timeout: secondi disponibili
        /// defaultIndex: indice della scelta forzata se il timer scade
        /// </summary>
        public static event Action<float, int> OnTimedChoiceStarted;
        
        /// <summary>
        /// Il timer della scelta è scaduto.
        /// </summary>
        public static event Action OnTimedChoiceExpired;
        
        // ============================================
        // AUDIO EVENTS
        // ============================================
        
        /// <summary>
        /// Richiesta di riprodurre un SFX.
        /// sfxId: identificatore del suono (es. "phone_ring", "glass_break")
        /// </summary>
        public static event Action<string> OnSFXRequested;
        
        /// <summary>
        /// Richiesta di avviare un'ambience.
        /// ambienceId: identificatore (es. "dispatch_night")
        /// </summary>
        public static event Action<string> OnAmbienceStart;
        
        /// <summary>
        /// Richiesta di fermare l'ambience corrente.
        /// </summary>
        public static event Action OnAmbienceStop;
        
        /// <summary>
        /// Richiesta di riprodurre una clip vocale.
        /// voiceId: identificatore (es. "iris_01")
        /// </summary>
        public static event Action<string> OnVoiceRequested;
        
        // ============================================
        // UI EVENTS
        // ============================================
        
        /// <summary>
        /// Comando UI dal file ink (es. "dead_air_screen", "return_to_menu").
        /// </summary>
        public static event Action<string> OnUICommand;
        
        // ============================================
        // STORY FLOW EVENTS
        // ============================================
        
        /// <summary>
        /// La storia è terminata (-> END raggiunto).
        /// </summary>
        public static event Action OnStoryEnd;
        
        /// <summary>
        /// Richiesta di continuare la storia (dopo click del giocatore).
        /// </summary>
        public static event Action OnContinueRequested;

        // ============================================
        // VOICE PLAYBACK EVENTS
        // ============================================

        /// <summary>
        /// Una clip vocale è iniziata.
        /// float = durata in secondi (per sync typewriter).
        /// </summary>
        public static event Action<float> OnVoiceStarted;

        /// <summary>
        /// La clip vocale è terminata.
        /// </summary>
        public static event Action OnVoiceFinished;

        // ============================================
        // TIMER EVENTS
        // ============================================

        /// <summary>
        /// Progresso del timer normalizzato [0-1].
        /// 1 = appena iniziato, 0 = scaduto.
        /// </summary>
        public static event Action<float> OnTimerProgress;

        /// <summary>
        /// Il timer è stato cancellato (giocatore ha scelto in tempo).
        /// </summary>
        public static event Action OnTimerCancelled;
        
        // ============================================
        // INVOKE METHODS
        // Incapsulano il null-check, evitano NullReferenceException
        // se nessuno è iscritto all'evento.
        // ============================================
        
        public static void DialogueLine(string text)
        {
            OnDialogueLine?.Invoke(text);
        }
        
        public static void SpeakerLine(string speaker, string text)
        {
            OnSpeakerLine?.Invoke(speaker, text);
        }
        
        public static void ChoicesPresented(List<Choice> choices)
        {
            OnChoicesPresented?.Invoke(choices);
        }
        
        public static void ChoiceSelected(int index)
        {
            OnChoiceSelected?.Invoke(index);
        }
        
        public static void TimedChoiceStarted(float timeout, int defaultIndex)
        {
            OnTimedChoiceStarted?.Invoke(timeout, defaultIndex);
        }
        
        public static void TimedChoiceExpired()
        {
            OnTimedChoiceExpired?.Invoke();
        }
        
        public static void SFXRequested(string sfxId)
        {
            OnSFXRequested?.Invoke(sfxId);
        }
        
        public static void AmbienceStart(string ambienceId)
        {
            OnAmbienceStart?.Invoke(ambienceId);
        }
        
        public static void AmbienceStop()
        {
            OnAmbienceStop?.Invoke();
        }
        
        public static void VoiceRequested(string voiceId)
        {
            OnVoiceRequested?.Invoke(voiceId);
        }
        
        public static void UICommand(string command)
        {
            OnUICommand?.Invoke(command);
        }
        
        public static void StoryEnd()
        {
            OnStoryEnd?.Invoke();
        }
        
        public static void ContinueRequested()
        {
            OnContinueRequested?.Invoke();
        }

        public static void VoiceStarted(float duration)
        {
            OnVoiceStarted?.Invoke(duration);
        }

        public static void VoiceFinished()
        {
            OnVoiceFinished?.Invoke();
        }

        public static void TimerProgress(float progress)
        {
            OnTimerProgress?.Invoke(progress);
        }

        public static void TimerCancelled()
        {
            OnTimerCancelled?.Invoke();
        }
        
        // ============================================
        // CLEANUP
        // Chiamare quando si cambia scena o si resetta il gioco.
        // Evita memory leak da subscriber non rimossi.
        // ============================================
        
        public static void ClearAllListeners()
        {
            OnDialogueLine = null;
            OnSpeakerLine = null;
            OnChoicesPresented = null;
            OnChoiceSelected = null;
            OnTimedChoiceStarted = null;
            OnTimedChoiceExpired = null;
            OnSFXRequested = null;
            OnAmbienceStart = null;
            OnAmbienceStop = null;
            OnVoiceRequested = null;
            OnUICommand = null;
            OnStoryEnd = null;
            OnContinueRequested = null;
            OnVoiceStarted = null;
            OnVoiceFinished = null;
            OnTimerProgress = null;
            OnTimerCancelled = null;
        }
    }
}
