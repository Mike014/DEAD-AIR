using System;
using UnityEngine;

namespace DeadAir.Events
{
    [CreateAssetMenu(
        fileName = "New Int Event Channel",
        menuName = "DEAD AIR/Events/Int Event Channel"
    )]
    public class IntEventChannel : ScriptableObject
    {
        // Campo privato evento C#
        private event Action<int> onEventRaised;

        /// <summary>
        /// Pubblica l'evento. Chiamato da Publishers (es. StoryManager).
        /// </summary>
        public void RaiseEvent(int value)
        {
            onEventRaised?.Invoke(value);
        }

        /// <summary>
        /// Registra un listener. Chiamato da Observers (es. DialogueUI).
        /// </summary>
        public void Subscribe(Action<int> listener)
        {
            onEventRaised += listener;
        }

        /// <summary>
        /// Deregistra un listener. CRITICO per evitare memory leak.
        /// </summary>
        public void Unsubscribe(Action<int> listener)
        {
            onEventRaised -= listener;
        }
    }
}