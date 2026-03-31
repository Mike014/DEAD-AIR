using System.Collections.Generic;
using UnityEngine;
using DeadAir.Events;

namespace DeadAir.Audio
{
    /// <summary>
    /// Gestisce la riproduzione di SFX e Ambience usando AudioClipLibrary SO.
    /// Supporta Singleton cross-scene MA con ricaricamento libraries per scena.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // ============================================
        // SINGLETON
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
        
        [Header("Audio Libraries (ScriptableObject)")]
        [Tooltip("Libraries SFX da caricare per questa scena")]
        [SerializeField] private AudioClipLibrary[] _sfxLibraries;
        
        [Tooltip("Libraries Ambience da caricare per questa scena")]
        [SerializeField] private AudioClipLibrary[] _ambienceLibraries;
        
        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float _ambienceVolume = 0.5f;
        [SerializeField] private float _ambienceFadeDuration = 1f;
        
        // ============================================
        // EVENT CHANNELS
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
        // UNITY LIFECYCLE
        // ============================================
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                // Esiste già un AudioManager, ma questa scena potrebbe avere libraries diverse
                // Reload le libraries invece di distruggere
                Debug.Log($"[AudioManager] Singleton già esistente. Ricarico libraries per scena corrente.");
                Instance.ReloadLibraries(_sfxLibraries, _ambienceLibraries);
                Instance.ReassignChannels(sfxRequestedChannel, ambienceStartChannel, ambienceStopChannel);
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            ConfigureAudioSources();
            BuildLookupTables();
        }
        
        private void OnEnable()
        {
            SubscribeToChannels();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromChannels();
        }
        
        // ============================================
        // INITIALIZATION
        // ============================================
        
        private void BuildLookupTables()
        {
            _sfxLookup = new Dictionary<string, AudioClip>();
            _ambienceLookup = new Dictionary<string, AudioClip>();
            
            LoadLibraries();
        }
        
        /// <summary>
        /// Carica tutte le libraries assegnate nell'Inspector.
        /// </summary>
        private void LoadLibraries()
        {
            // Load SFX libraries
            if (_sfxLibraries != null)
            {
                foreach (var library in _sfxLibraries)
                {
                    if (library != null)
                    {
                        library.PopulateDictionary(_sfxLookup, "SFX");
                    }
                }
            }
            
            // Load Ambience libraries
            if (_ambienceLibraries != null)
            {
                foreach (var library in _ambienceLibraries)
                {
                    if (library != null)
                    {
                        library.PopulateDictionary(_ambienceLookup, "Ambience");
                    }
                }
            }
            
            Debug.Log($"[AudioManager] Totale caricato: {_sfxLookup.Count} SFX, {_ambienceLookup.Count} Ambience");
        }
        
        /// <summary>
        /// Ricarica le libraries (chiamato quando cambia scena).
        /// </summary>
        public void ReloadLibraries(AudioClipLibrary[] sfxLibraries, AudioClipLibrary[] ambienceLibraries)
        {
            Debug.Log("[AudioManager] Reload libraries per nuova scena...");
            
            // Clear dictionaries esistenti
            _sfxLookup?.Clear();
            _ambienceLookup?.Clear();
            
            if (_sfxLookup == null) _sfxLookup = new Dictionary<string, AudioClip>();
            if (_ambienceLookup == null) _ambienceLookup = new Dictionary<string, AudioClip>();
            
            // Load nuove libraries
            _sfxLibraries = sfxLibraries;
            _ambienceLibraries = ambienceLibraries;
            
            LoadLibraries();
        }
        
        /// <summary>
        /// Reassegna i channels (chiamato quando cambia scena).
        /// </summary>
        public void ReassignChannels(
            StringEventChannel sfx,
            StringEventChannel ambienceStart,
            VoidEventChannel ambienceStop)
        {
            // Unsubscribe vecchi channels
            UnsubscribeFromChannels();
            
            // Assegna nuovi channels
            sfxRequestedChannel = sfx;
            ambienceStartChannel = ambienceStart;
            ambienceStopChannel = ambienceStop;
            
            // Subscribe nuovi channels
            SubscribeToChannels();
            
            Debug.Log("[AudioManager] Channels reassegnati");
        }
        
        private void SubscribeToChannels()
        {
            if (sfxRequestedChannel != null)
                sfxRequestedChannel.Subscribe(PlaySFX);
            
            if (ambienceStartChannel != null)
                ambienceStartChannel.Subscribe(StartAmbience);
            
            if (ambienceStopChannel != null)
                ambienceStopChannel.Subscribe(StopAmbience);
        }
        
        private void UnsubscribeFromChannels()
        {
            if (sfxRequestedChannel != null)
                sfxRequestedChannel.Unsubscribe(PlaySFX);
            
            if (ambienceStartChannel != null)
                ambienceStartChannel.Unsubscribe(StartAmbience);
            
            if (ambienceStopChannel != null)
                ambienceStopChannel.Unsubscribe(StopAmbience);
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
        
        public void PlaySFX(string sfxId)
        {
            if (string.IsNullOrEmpty(sfxId) || _sfxSource == null)
                return;

            string key = sfxId.ToLowerInvariant();

            if (_sfxLookup.TryGetValue(key, out AudioClip clip))
            {
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
        
        public void StartAmbience(string ambienceId)
        {
            if (string.IsNullOrEmpty(ambienceId) || _ambienceSource == null)
                return;

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
        // COROUTINES (unchanged)
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
        // PUBLIC API (unchanged)
        // ============================================
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void SetAmbienceVolume(float volume)
        {
            _ambienceVolume = Mathf.Clamp01(volume);
            if (_ambienceSource != null && _ambienceSource.isPlaying && _ambienceFadeCoroutine == null)
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