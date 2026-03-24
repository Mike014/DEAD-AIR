using UnityEngine;
using DeadAir.Events;

namespace DeadAir.Narrative
{
    /// <summary>
    /// Gestisce il timer delle scelte a tempo.
    /// Quando il timer scade, seleziona automaticamente la scelta di default.
    /// 
    /// Responsabilità SINGOLA:
    /// - Countdown del timer
    /// - Notifica progresso (per UI)
    /// - Trigger scelta default allo scadere
    /// 
    /// NON responsabile di:
    /// - Decidere quale scelta è default (viene da ink)
    /// - Rendering della barra timer (→ DialogueUI)
    /// </summary>
    public class TimedChoiceHandler : MonoBehaviour
    {
        // ============================================
        // EVENTS (per UI timer bar)
        // ============================================
        
        /// <summary>
        /// Progresso del timer normalizzato [0-1].
        /// 1 = appena iniziato, 0 = scaduto.
        /// </summary>
        public static event System.Action<float> OnTimerProgress;
        
        /// <summary>
        /// Il timer è stato cancellato (giocatore ha scelto in tempo).
        /// </summary>
        public static event System.Action OnTimerCancelled;
        
        // ============================================
        // PRIVATE STATE
        // ============================================
        
        private bool _isRunning;
        private float _timeRemaining;
        private float _totalTime;
        private int _defaultChoiceIndex;
        
        // ============================================
        // UNITY LIFECYCLE
        // ============================================
        
        private void OnEnable()
        {
            NarrativeEvents.OnTimedChoiceStarted += HandleTimedChoiceStarted;
            NarrativeEvents.OnChoiceSelected += HandleChoiceSelected;
        }
        
        private void OnDisable()
        {
            NarrativeEvents.OnTimedChoiceStarted -= HandleTimedChoiceStarted;
            NarrativeEvents.OnChoiceSelected -= HandleChoiceSelected;
        }
        
        private void Update()
        {
            if (!_isRunning)
                return;
            
            // Countdown
            _timeRemaining -= Time.deltaTime;
            
            // Calcola progresso normalizzato
            float progress = Mathf.Clamp01(_timeRemaining / _totalTime);
            OnTimerProgress?.Invoke(progress);
            
            // Timer scaduto
            if (_timeRemaining <= 0f)
            {
                TimerExpired();
            }
        }
        
        // ============================================
        // EVENT HANDLERS
        // ============================================
        
        /// <summary>
        /// Una timed choice è iniziata — avvia il countdown.
        /// </summary>
        private void HandleTimedChoiceStarted(float timeout, int defaultIndex)
        {
            _totalTime = timeout;
            _timeRemaining = timeout;
            _defaultChoiceIndex = defaultIndex;
            _isRunning = true;
            
            Debug.Log($"[TimedChoiceHandler] Timer avviato: {timeout}s, default: {defaultIndex}");
            
            // Notifica UI che il timer è al 100%
            OnTimerProgress?.Invoke(1f);
        }
        
        /// <summary>
        /// Il giocatore ha selezionato una scelta — cancella il timer.
        /// </summary>
        private void HandleChoiceSelected(int index)
        {
            if (_isRunning)
            {
                CancelTimer();
            }
        }
        
        // ============================================
        // TIMER LOGIC
        // ============================================
        
        /// <summary>
        /// Timer scaduto — seleziona la scelta di default.
        /// </summary>
        private void TimerExpired()
        {
            _isRunning = false;
            
            Debug.Log($"[TimedChoiceHandler] Timer SCADUTO — selezione automatica: {_defaultChoiceIndex}");
            
            // Notifica che il timer è scaduto
            NarrativeEvents.TimedChoiceExpired();
            
            // Seleziona automaticamente la scelta di default
            // Questo triggera StoryManager.HandleChoiceSelected()
            NarrativeEvents.ChoiceSelected(_defaultChoiceIndex);
        }
        
        /// <summary>
        /// Cancella il timer (giocatore ha scelto in tempo).
        /// </summary>
        private void CancelTimer()
        {
            _isRunning = false;
            
            Debug.Log("[TimedChoiceHandler] Timer cancellato — giocatore ha scelto.");
            
            OnTimerCancelled?.Invoke();
        }
        
        // ============================================
        // PUBLIC API (per debug/testing)
        // ============================================
        
        /// <summary>
        /// Forza lo scadere del timer. Utile per testing.
        /// </summary>
        public void ForceExpire()
        {
            if (_isRunning)
            {
                _timeRemaining = 0f;
            }
        }
        
        /// <summary>
        /// Restituisce se il timer è attivo.
        /// </summary>
        public bool IsRunning => _isRunning;
        
        /// <summary>
        /// Restituisce il tempo rimanente.
        /// </summary>
        public float TimeRemaining => _timeRemaining;
        
        /// <summary>
        /// Restituisce il progresso normalizzato [0-1].
        /// </summary>
        public float Progress => _totalTime > 0f ? Mathf.Clamp01(_timeRemaining / _totalTime) : 0f;
    }
}