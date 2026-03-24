using System;
using System.Collections.Generic;
using Ink.Runtime;

namespace DeadAir.Events
{
    // Static Event Hub per la comunicazione disaccoppiata tra sistemi.
    // Nessun sistema conosce gli altri — tutti comunicano attraverso eventi.
    // 
    // Pattern: Observer via C# Events
    // Perché static: Accesso globale senza Singleton MonoBehaviour, 
    // zero allocazioni per lookup, ideale per eventi frequenti.
    public static class NarrativeEvents
    {
        // ============================================
        // DIALOGUE EVENTS
        // ============================================
        
        // Linea di testo narrativo senza speaker (pensieri di Ward, descrizioni).
        public static event Action<string> OnDialogueLine;
        
        // Linea con speaker identificato (Ward o Iris).
        // speaker: "ward" o "iris"
        // text: il contenuto della linea
        public static event Action<string, string> OnSpeakerLine;
        
        // Scelte disponibili per il giocatore.
        public static event Action<List<Choice>> OnChoicesPresented;
        
        // Il giocatore ha selezionato una scelta.
        public static event Action<int> OnChoiceSelected;
        
        // ============================================
        // TIMED CHOICE EVENTS
        // ============================================
        
        // Una scelta con timer è iniziata.
        // timeout: secondi disponibili
        // defaultIndex: indice della scelta forzata se il timer scade
        public static event Action<float, int> OnTimedChoiceStarted;
        
        // Il timer della scelta è scaduto.
        public static event Action OnTimedChoiceExpired;
        
        // ============================================
        // AUDIO EVENTS
        // ============================================
        
        // Richiesta di riprodurre un SFX.
        // sfxId: identificatore del suono (es. "phone_ring", "glass_break")
        public static event Action<string> OnSFXRequested;
        
        // Richiesta di avviare un'ambience.
        // ambienceId: identificatore (es. "dispatch_night")
        public static event Action<string> OnAmbienceStart;
        
        /// Richiesta di fermare l'ambience corrente.
        public static event Action OnAmbienceStop;
        
        // ============================================
        // UI EVENTS
        // ============================================
        
        // Comando UI dal file ink (es. "dead_air_screen", "return_to_menu").
        public static event Action<string> OnUICommand;
        
        // ============================================
        // STORY FLOW EVENTS
        // ============================================
        
        /// La storia è terminata (-> END raggiunto).
        public static event Action OnStoryEnd;
        
        /// Richiesta di continuare la storia (dopo click del giocatore).
        public static event Action OnContinueRequested;
        
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
            OnUICommand = null;
            OnStoryEnd = null;
            OnContinueRequested = null;
        }
    }
}
