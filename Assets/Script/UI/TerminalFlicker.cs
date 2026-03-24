using UnityEngine;
using TMPro;

namespace DeadAir.UI
{
    /// <summary>
    /// Effetto flicker da terminale CRT per TextMeshPro.
    /// Combina oscillazione sottile costante + glitch random occasionali.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TerminalFlicker : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================
        
        [Header("Subtle Oscillation")]
        [Tooltip("Alpha minimo dell'oscillazione costante")]
        [SerializeField, Range(0.5f, 1f)] private float _minAlpha = 0.85f;
        
        [Tooltip("Alpha massimo dell'oscillazione costante")]
        [SerializeField, Range(0.5f, 1f)] private float _maxAlpha = 1f;
        
        [Tooltip("Velocità dell'oscillazione (più alto = più veloce)")]
        [SerializeField, Range(1f, 20f)] private float _oscillationSpeed = 8f;
        
        [Header("Random Glitch")]
        [Tooltip("Probabilità di glitch per frame (0-1)")]
        [SerializeField, Range(0f, 0.1f)] private float _glitchChance = 0.02f;
        
        [Tooltip("Alpha durante il glitch")]
        [SerializeField, Range(0f, 0.5f)] private float _glitchAlpha = 0.3f;
        
        [Tooltip("Durata del glitch in secondi")]
        [SerializeField, Range(0.01f, 0.2f)] private float _glitchDuration = 0.05f;
        
        // ============================================
        // PRIVATE STATE
        // ============================================
        
        private TextMeshProUGUI _text;
        private Color _baseColor;
        private float _glitchTimer;
        private bool _isGlitching;
        
        // ============================================
        // UNITY LIFECYCLE
        // ============================================
        
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _baseColor = _text.color;
        }
        
        private void Update()
        {
            if (_text == null) return;
            
            float targetAlpha;
            
            // Glitch in corso?
            if (_isGlitching)
            {
                _glitchTimer -= Time.deltaTime;
                
                if (_glitchTimer <= 0f)
                {
                    _isGlitching = false;
                }
                else
                {
                    targetAlpha = _glitchAlpha;
                    ApplyAlpha(targetAlpha);
                    return;
                }
            }
            
            // Check per nuovo glitch random
            if (Random.value < _glitchChance)
            {
                _isGlitching = true;
                _glitchTimer = _glitchDuration;
                ApplyAlpha(_glitchAlpha);
                return;
            }
            
            // Oscillazione sottile costante (sine wave)
            float oscillation = Mathf.Sin(Time.time * _oscillationSpeed);
            
            // Mappa da [-1, 1] a [_minAlpha, _maxAlpha]
            targetAlpha = Mathf.Lerp(_minAlpha, _maxAlpha, (oscillation + 1f) * 0.5f);
            
            ApplyAlpha(targetAlpha);
        }
        
        // ============================================
        // PRIVATE METHODS
        // ============================================
        
        private void ApplyAlpha(float alpha)
        {
            Color c = _baseColor;
            c.a = alpha;
            _text.color = c;
        }
        
        // ============================================
        // PUBLIC API
        // ============================================
        
        /// <summary>
        /// Forza un glitch immediato.
        /// </summary>
        public void TriggerGlitch()
        {
            _isGlitching = true;
            _glitchTimer = _glitchDuration;
        }
        
        /// <summary>
        /// Aggiorna il colore base (chiamare se cambi colore runtime).
        /// </summary>
        public void UpdateBaseColor(Color newColor)
        {
            _baseColor = newColor;
        }
    }
}
