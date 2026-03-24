using UnityEngine;
using UnityEngine.UI;

namespace DeadAir.UI
{
    /// <summary>
    /// Effetto flicker "risoluzione/densità" per Image UI di tipo Tiled o Sliced.
    /// Modifica il parametro pixelsPerUnitMultiplier per far "respirare" o glitchare la texture.
    /// ATTENZIONE: Questo script causa il ricalcolo della mesh del Canvas ogni frame. Alto costo CPU.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ImageTypeFlicker : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================
        
        [Header("Setup Check")]
        [Tooltip("Se attivo, avvisa nell'Inspector se l'Image Type non è compatibile (Tiled/Sliced).")]
        [SerializeField] private bool _validateSetupOnStart = true;

        [Header("Subtle Oscillation")]
        [Tooltip("Moltiplicatore PPU minimo dell'oscillazione costante.")]
        [SerializeField, Min(0.1f)] private float _minMultiplier = 0.8f;
        
        [Tooltip("Moltiplicatore PPU massimo dell'oscillazione costante.")]
        [SerializeField, Min(0.1f)] private float _maxMultiplier = 1.2f;
        
        [Tooltip("Velocità dell'oscillazione (più alto = più veloce).")]
        [SerializeField, Range(1f, 30f)] private float _oscillationSpeed = 12f;
        
        [Header("Random Glitch")]
        [Tooltip("Probabilità di glitch per frame (0-1).")]
        [SerializeField, Range(0f, 0.1f)] private float _glitchChance = 0.03f;
        
        [Tooltip("Moltiplicatore PPU durante il glitch (es: molto alto per densità fitta, molto basso per pixel enormi).")]
        [SerializeField, Min(0.01f)] private float _glitchMultiplier = 5.0f;
        
        [Tooltip("Durata del glitch in secondi.")]
        [SerializeField, Range(0.01f, 0.2f)] private float _glitchDuration = 0.06f;
        
        // ============================================
        // PRIVATE STATE
        // ============================================
        
        private Image _image;
        private float _glitchTimer;
        private bool _isGlitching;
        
        // ============================================
        // UNITY LIFECYCLE
        // ============================================
        
        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            if (_validateSetupOnStart)
            {
                ValidateImageType();
            }
        }
        
        private void Update()
        {
            if (_image == null) return;
            
            float targetMultiplier;
            
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
                    ApplyMultiplier(_glitchMultiplier);
                    return; // Uscita anticipata per risparmiare calcoli sinusoidali
                }
            }
            
            // Gestione stato: Trigger nuovo glitch
            if (Random.value < _glitchChance)
            {
                _isGlitching = true;
                _glitchTimer = _glitchDuration;
                ApplyMultiplier(_glitchMultiplier);
                return;
            }
            
            // Calcolo oscillazione di base
            float oscillation = Mathf.Sin(Time.time * _oscillationSpeed);
            
            // Mappa da [-1, 1] a [_minMultiplier, _maxMultiplier]
            targetMultiplier = Mathf.Lerp(_minMultiplier, _maxMultiplier, (oscillation + 1f) * 0.5f);
            
            ApplyMultiplier(targetMultiplier);
        }
        
        // ============================================
        // PRIVATE METHODS
        // ============================================
        
        private void ApplyMultiplier(float multiplier)
        {
            // Questa assegnazione interna invoca SetVerticesDirty() e forzerà il rebuild del Canvas.
            _image.pixelsPerUnitMultiplier = multiplier;
        }

        private void ValidateImageType()
        {
            if (_image.type != Image.Type.Sliced && _image.type != Image.Type.Tiled)
            {
                Debug.LogWarning($"[DeadAir.UI] {gameObject.name} ha TerminalImageTypeFlicker ma l'Image Type è {_image.type}. L'effetto PPU funziona solo con Sliced o Tiled. L'effetto sarà invisibile.", gameObject);
            }
        }

        // Editor-only check for workflow feedback
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_minMultiplier > _maxMultiplier) _minMultiplier = _maxMultiplier;
        }
#endif
        
        // ============================================
        // PUBLIC API
        // ============================================
        
        public void TriggerGlitch()
        {
            _isGlitching = true;
            _glitchTimer = _glitchDuration;
        }
    }
}