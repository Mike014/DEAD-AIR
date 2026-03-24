using UnityEngine;
using UnityEngine.UI; // Necessario per accedere alla classe base Graphic

namespace DeadAir.UI
{
    /// <summary>
    /// Effetto flicker da terminale CRT per componenti UI.
    /// Sfrutta il polimorfismo di Graphic per supportare Image, TextMeshProUGUI e RawImage.
    /// </summary>
    [RequireComponent(typeof(Graphic))] // Garantiamo che ci sia almeno un componente UI
    public class TerminalFlicker : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================
        
        [Header("Subtle Oscillation")]
        [Tooltip("Alpha minimo dell'oscillazione costante")]
        [SerializeField, Range(0f, 1f)] private float _minAlpha = 0.85f;
        
        [Tooltip("Alpha massimo dell'oscillazione costante")]
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 1f;
        
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
        
        private Graphic _uiElement; // L'astrazione chiave: punta alla classe base
        private Color _baseColor;
        private float _glitchTimer;
        private bool _isGlitching;
        
        // ============================================
        // UNITY LIFECYCLE
        // ============================================
        
        private void Awake()
        {
            // Otteniamo il componente Graphic (sarà un Image o un TMP)
            _uiElement = GetComponent<Graphic>();
            
            // Memorizziamo il colore impostato nell'Inspector
            _baseColor = _uiElement.color;
        }
        
        private void Update()
        {
            if (_uiElement == null) return;
            
            float targetAlpha;
            
            // Gestione stato: Glitch in corso
            if (_isGlitching)
            {
                _glitchTimer -= Time.deltaTime;
                
                if (_glitchTimer <= 0f)
                {
                    _isGlitching = false;
                }
                else
                {
                    ApplyAlpha(_glitchAlpha);
                    return; // Uscita anticipata per non calcolare l'onda sinusoidale
                }
            }
            
            // Gestione stato: Trigger nuovo glitch
            if (Random.value < _glitchChance)
            {
                _isGlitching = true;
                _glitchTimer = _glitchDuration;
                ApplyAlpha(_glitchAlpha);
                return;
            }
            
            // Calcolo oscillazione di base
            float oscillation = Mathf.Sin(Time.time * _oscillationSpeed);
            targetAlpha = Mathf.Lerp(_minAlpha, _maxAlpha, (oscillation + 1f) * 0.5f);
            
            ApplyAlpha(targetAlpha);
        }
        
        // ============================================
        // PRIVATE METHODS
        // ============================================
        
        private void ApplyAlpha(float alpha)
        {
            // Lavoriamo su una copia locale della struct Color
            Color newColor = _baseColor;
            newColor.a = alpha;
            
            // L'assegnazione altera i vertici della UI
            _uiElement.color = newColor; 
        }
        
        // ============================================
        // PUBLIC API
        // ============================================
        
        public void TriggerGlitch()
        {
            _isGlitching = true;
            _glitchTimer = _glitchDuration;
        }
        
        public void UpdateBaseColor(Color newColor)
        {
            _baseColor = newColor;
        }
    }
}