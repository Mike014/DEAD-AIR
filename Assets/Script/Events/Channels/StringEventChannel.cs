using System;
using UnityEngine;

namespace DeadAir.Events
{
    [CreateAssetMenu(
        fileName = "New String Event Channel",
        menuName = "DEAD AIR/Events/String Event Channel"
    )]
    public class StringEventChannel : ScriptableObject
    {
        // Campo privato evento C#
        private event Action<string> onEventRaised;
        
        /// <summary>
        /// Pubblica l'evento. Chiamato da Publishers (es. StoryManager).
        /// </summary>
        public void RaiseEvent(string value)
        {
            onEventRaised?.Invoke(value);
        }
        
        /// <summary>
        /// Registra un listener. Chiamato da Observers (es. DialogueUI).
        /// </summary>
        public void Subscribe(Action<string> listener)
        {
            onEventRaised += listener;
        }
        
        /// <summary>
        /// Deregistra un listener. CRITICO per evitare memory leak.
        /// </summary>
        public void Unsubscribe(Action<string> listener)
        {
            onEventRaised -= listener;
        }
    }
}