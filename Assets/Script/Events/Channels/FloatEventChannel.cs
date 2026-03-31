using System;
using UnityEngine;

namespace DeadAir.Events
{
    [CreateAssetMenu(
        fileName = "New Float Event Channel",
        menuName = "DEAD AIR/Events/Float Event Channel"
    )]
    public class FloatEventChannel : ScriptableObject
    {
        // Campo privato evento C#
        private event Action<float> onEventRaised;

        /// <summary>
        /// Pubblica l'evento. Chiamato da Publishers (es. StoryManager).
        /// </summary>
        public void RaiseEvent(float value)
        {
            onEventRaised?.Invoke(value);
        }

        /// <summary>
        /// Registra un listener. Chiamato da Observers (es. DialogueUI).
        /// </summary>
        public void Subscribe(Action<float> listener)
        {
            onEventRaised += listener;
        }

        /// <summary>
        /// Deregistra un listener. CRITICO per evitare memory leak.
        /// </summary>
        public void Unsubscribe(Action<float> listener)
        {
            onEventRaised -= listener;
        }
    }
}