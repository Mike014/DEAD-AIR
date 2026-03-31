using System;
using UnityEngine;

namespace DeadAir.Events
{
    [CreateAssetMenu(
        fileName = "New Void Event Channel",
        menuName = "DEAD AIR/Events/Void Event Channel"
    )]
    public class VoidEventChannel : ScriptableObject
    {
        // Campo privato evento C#
        private event Action onEventRaised;
        
        /// <summary>
        /// Pubblica l'evento. Chiamato da Publishers (es. StoryManager).
        /// </summary>
        public void RaiseEvent()
        {
            onEventRaised?.Invoke();
        }
        
        /// <summary>
        /// Registra un listener. Chiamato da Observers (es. DialogueUI).
        /// </summary>
        public void Subscribe(Action listener)
        {
            onEventRaised += listener;
        }
        
        /// <summary>
        /// Deregistra un listener. CRITICO per evitare memory leak.
        /// </summary>
        public void Unsubscribe(Action listener)
        {
            onEventRaised -= listener;
        }
    }
}