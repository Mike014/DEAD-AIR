using System;
using UnityEngine;

namespace DeadAir.Events
{
    [CreateAssetMenu(
        fileName = "New String String Event Channel",
        menuName = "DEAD AIR/Events/String String Event Channel"
    )]
    public class StringStringEventChannel : ScriptableObject
    {
        // Campo privato evento C#
        private event Action<string, string> onEventRaised;
        
        /// <summary>
        /// Pubblica l'evento. Chiamato da Publishers (es. StoryManager).
        /// </summary>
        public void RaiseEvent(string value1, string value2)
        {
            onEventRaised?.Invoke(value1, value2);
        }
        
        /// <summary>
        /// Registra un listener. Chiamato da Observers (es. DialogueUI).
        /// </summary>
        public void Subscribe(Action<string, string> listener)
        {
            onEventRaised += listener;
        }
        
        /// <summary>
        /// Deregistra un listener. CRITICO per evitare memory leak.
        /// </summary>
        public void Unsubscribe(Action<string, string> listener)
        {
            onEventRaised -= listener;
        }
    }
}