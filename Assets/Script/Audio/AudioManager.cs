using System.Collections.Generic;
using UnityEngine;
using DeadAir.Events;

namespace DeadAir.Audio
{
    /// <summary>
    /// Gestisce la riproduzione di SFX e Ambience.
    /// Ascolta gli eventi dai channels ScriptableObject e riproduce l'audio appropriato.
    /// 
    /// Responsabilità:
    /// - Riprodurre SFX one-shot
    /// - Gestire loop ambience (start/stop/crossfade)
    /// - Lookup clip da dizionario
    /// 
    /// NON responsabile di:
    /// - Voice/dialoghi (→ VoiceManager)
    /// - Sync con typewriter (→ VoiceManager)
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // ============================================
        // SINGLETON (semplice, per audio persistente)
        // ============================================
        
        public static AudioManager Instance { get; private set; }
        
        // ============================================
        // SERIALIZED FIELDS
        // ============================================
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _ambienceSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _videoSource;
        
        [Header("SFX Clips")]
        [SerializeField] private SFXEntry[] _sfxClips;
        
        [Header("Ambience Clips")]
        [SerializeField] private AmbienceEntry[] _ambienceClips;
        
        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float _ambienceVolume = 0.5f;
        [SerializeField] private float _ambienceFadeDuration = 1f;
        
        // ============================================
        // EVENT CHANNELS (Dependency Injection)
        // ============================================
        
        [Header("Audio Event Channels (Observer)")]
        [SerializeField] private StringEventChannel sfxRequestedChannel;
        [SerializeField] private StringEventChannel ambienceStartChannel;
        [SerializeField] private VoidEventChannel ambienceStopChannel;
        
        // ============================================
        // PRIVATE STATE
        // ============================================
        
        private Dictionary<string, AudioClip> _sfxLookup;
        private Dictionary<string, AudioClip> _ambienceLookup;
        private Coroutine _ambienceFadeCoroutine;
        
        // ============================================
        // SERIALIZABLE STRUCTS (per Inspector)
        // ============================================
        
        /// <summary>
        /// Entry per SFX. Permette di mappare ID string a clip nell'Inspector.
        /// </summary>
        [System.Serializable]
        public struct SFXEntry
        {
            public string id;           // es. "phone_ring", "glass_break"
            public AudioClip clip;
        }
        
        /// <summary>
        /// Entry per Ambience.
        /// </summary>
        [System.Serializable]
        public struct AmbienceEntry
        {
            public string id;           // es. "dispatch_night"
            public AudioClip clip;
        }
        
        // ============================================
        // UNITY LIFECYCLE
        // ============================================
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildLookupTables();
            ConfigureAudioSources();
        }
        
        private void OnEnable()
        {
            // Subscribe ai channels
            if (sfxRequestedChannel != null)
                sfxRequestedChannel.Subscribe(PlaySFX);
            
            if (ambienceStartChannel != null)
                ambienceStartChannel.Subscribe(StartAmbience);
            
            if (ambienceStopChannel != null)
                ambienceStopChannel.Subscribe(StopAmbience);
        }
        
        private void OnDisable()
        {
            // Unsubscribe dai channels
            if (sfxRequestedChannel != null)
                sfxRequestedChannel.Unsubscribe(PlaySFX);
            
            if (ambienceStartChannel != null)
                ambienceStartChannel.Unsubscribe(StartAmbience);
            
            if (ambienceStopChannel != null)
                ambienceStopChannel.Unsubscribe(StopAmbience);
        }
        
        // ============================================
        // INITIALIZATION
        // ============================================
        
        private void BuildLookupTables()
        {
            // SFX lookup
            _sfxLookup = new Dictionary<string, AudioClip>();
            if (_sfxClips != null)
            {
                foreach (var entry in _sfxClips)
                {
                    if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                    {
                        _sfxLookup[entry.id.ToLowerInvariant()] = entry.clip;
                    }
                }
            }
            
            // Ambience lookup
            _ambienceLookup = new Dictionary<string, AudioClip>();
            if (_ambienceClips != null)
            {
                foreach (var entry in _ambienceClips)
                {
                    if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                    {
                        _ambienceLookup[entry.id.ToLowerInvariant()] = entry.clip;
                    }
                }
            }
            
            Debug.Log($"[AudioManager] Loaded {_sfxLookup.Count} SFX, {_ambienceLookup.Count} Ambience clips.");
        }
        
        private void ConfigureAudioSources()
        {
            if (_sfxSource != null)
            {
                _sfxSource.playOnAwake = false;
                _sfxSource.loop = false;
            }
            
            if (_ambienceSource != null)
            {
                _ambienceSource.playOnAwake = false;
                _ambienceSource.loop = true;
                _ambienceSource.volume = _ambienceVolume;
            }
        }
        
        // ============================================
        // SFX
        // ============================================
        
        /// <summary>
        /// Riproduce un SFX one-shot.
        /// Subscribed a sfxRequestedChannel.
        /// </summary>
        public void PlaySFX(string sfxId)
        {
            if (string.IsNullOrEmpty(sfxId))
                return;

            if (_sfxSource == null)
            {
                Debug.LogWarning("[AudioManager] _sfxSource non assegnato nell'Inspector!");
                return;
            }

            string key = sfxId.ToLowerInvariant();

            if (_sfxLookup.TryGetValue(key, out AudioClip clip))
            {
                // Gestione specifica per il loop
                if (key == "dead_air")
                {
                    _sfxSource.clip = clip;
                    _sfxSource.volume = _sfxVolume;
                    _sfxSource.loop = true;
                    _sfxSource.Play();
                    
                    Debug.Log($"[AudioManager] SFX Looping: {sfxId}");
                }
                else
                {
                    // Comportamento standard per suoni one-off
                    _sfxSource.PlayOneShot(clip, _sfxVolume);
                    
                    Debug.Log($"[AudioManager] SFX: {sfxId}");
                }
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX non trovato: {sfxId}");
            }
        }
        
        // ============================================
        // AMBIENCE
        // ============================================
        
        /// <summary>
        /// Avvia un loop ambience con fade in.
        /// Subscribed a ambienceStartChannel.
        /// </summary>
        public void StartAmbience(string ambienceId)
        {
            if (string.IsNullOrEmpty(ambienceId))
                return;

            if (_ambienceSource == null)
            {
                Debug.LogWarning("[AudioManager] _ambienceSource non assegnato nell'Inspector!");
                return;
            }

            string key = ambienceId.ToLowerInvariant();

            if (_ambienceLookup.TryGetValue(key, out AudioClip clip))
            {
                if (_ambienceFadeCoroutine != null)
                    StopCoroutine(_ambienceFadeCoroutine);

                _ambienceSource.clip = clip;
                _ambienceSource.Play();
                _ambienceFadeCoroutine = StartCoroutine(FadeAmbienceIn(0f, _ambienceVolume, _ambienceFadeDuration));

                Debug.Log($"[AudioManager] Ambience START: {ambienceId}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Ambience non trovata: {ambienceId}");
            }
        }
        
        /// <summary>
        /// Ferma l'ambience con fade out.
        /// Subscribed a ambienceStopChannel.
        /// </summary>
        public void StopAmbience()
        {
            if (_ambienceSource == null || !_ambienceSource.isPlaying)
                return;

            if (_ambienceFadeCoroutine != null)
                StopCoroutine(_ambienceFadeCoroutine);

            _ambienceFadeCoroutine = StartCoroutine(FadeAmbienceAndStop(_ambienceSource.volume, 0f, _ambienceFadeDuration));

            Debug.Log("[AudioManager] Ambience STOP");
        }
        
        // ============================================
        // COROUTINES
        // ============================================
        
        private System.Collections.IEnumerator FadeAmbienceIn(float from, float to, float duration)
        {
            yield return FadeAmbience(from, to, duration);
            _ambienceFadeCoroutine = null;
        }

        private System.Collections.IEnumerator FadeAmbience(float from, float to, float duration)
        {
            float elapsed = 0f;
            _ambienceSource.volume = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _ambienceSource.volume = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            _ambienceSource.volume = to;
        }

        private System.Collections.IEnumerator FadeAmbienceAndStop(float from, float to, float duration)
        {
            yield return FadeAmbience(from, to, duration);
            _ambienceSource.Stop();
            _ambienceFadeCoroutine = null;
        }
        
        // ============================================
        // PUBLIC API
        // ============================================
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void SetAmbienceVolume(float volume)
        {
            _ambienceVolume = Mathf.Clamp01(volume);
            if (_ambienceSource.isPlaying && _ambienceFadeCoroutine == null)
            {
                _ambienceSource.volume = _ambienceVolume;
            }
        }
        
        public void StopAll()
        {
            if (_ambienceFadeCoroutine != null)
            {
                StopCoroutine(_ambienceFadeCoroutine);
                _ambienceFadeCoroutine = null;
            }

            _sfxSource?.Stop();
            _ambienceSource?.Stop();
        }
    }
}