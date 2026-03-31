using System;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace DeadAir.Events
{
    [CreateAssetMenu(
        fileName = "New Choice List Event Channel",
        menuName = "DEAD AIR/Events/Choice List Event Channel"
    )]
    public class ChoiceListEventChannel : ScriptableObject
    {
        // IReadOnlyList invece di List per safety
        private event Action<IReadOnlyList<Choice>> onEventRaised;
        
        /// <summary>
        /// Pubblica l'evento con lista di scelte Ink.
        /// </summary>
        public void RaiseEvent(IReadOnlyList<Choice> choices)
        {
            onEventRaised?.Invoke(choices);
        }
        
        /// <summary>
        /// Registra un listener.
        /// </summary>
        public void Subscribe(Action<IReadOnlyList<Choice>> listener)
        {
            onEventRaised += listener;
        }
        
        /// <summary>
        /// Deregistra un listener. CRITICO per evitare memory leak.
        /// </summary>
        public void Unsubscribe(Action<IReadOnlyList<Choice>> listener)
        {
            onEventRaised -= listener;
        }
    }
}