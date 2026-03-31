using System.Collections.Generic;
using UnityEngine;
using DeadAir.Events;

namespace DeadAir.Audio
{
    /// <summary>
    /// Gestisce la riproduzione delle voci dei personaggi.
    /// Separato da AudioManager per gestire la sincronizzazione con il typewriter.
    /// Comunica via Event Channels (ScriptableObject).
    /// 
    /// Responsabilità:
    /// - Riprodurre clip vocali per ID (es. "iris_01")
    /// - Notificare quando la voce finisce (per sync UI)
    /// - Lookup clip da dizionario
    /// 
    /// Design: Le clip vocali sono mappate per ID esplicito dal tag #voice:xxx
    /// </summary>
    public class VoiceManager : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================
        
        [Header("Audio Source")]
        [SerializeField] private AudioSource _voiceSource;
        
        [Header("Voice Clips")]
        [SerializeField] private VoiceEntry[] _voiceClips;
        
        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float _voiceVolume = 1f;
        
        // ============================================
        // EVENT CHANNELS (Dependency Injection)
        // ============================================
        
        [Header("Input Channels (VoiceManager è Observer)")]
        [SerializeField] private StringEventChannel voiceRequestedChannel;
        [SerializeField] private VoidEventChannel voiceStopChannel;
        
        [Header("Output Channels (VoiceManager è Publisher)")]
        [SerializeField] private FloatEventChannel voiceStartedChannel;
        [SerializeField] private VoidEventChannel voiceFinishedChannel;
        
        // ============================================
        // PRIVATE STATE
        // ============================================
        
        private Dictionary<string, AudioClip> _voiceLookup;
        private bool _isPlaying;
        
        // ============================================
        // SERIALIZABLE STRUCT
        // ============================================
        
        /// <summary>
        /// Entry per voice clip.
        /// ID deve corrispondere al tag nel file ink (es. "iris_01")
        /// </summary>
        [System.Serializable]
        public struct VoiceEntry
        {
            public string id;       // es. "iris_01", "iris_02"
            public AudioClip clip;
        }
        
        // ============================================
        // UNITY LIFECYCLE
        // ============================================
        
        private void Awake()
        {
            BuildLookupTable();
            ConfigureAudioSource();
        }
        
        private void OnEnable()
        {
            // Subscribe ai channels (VoiceManager è Observer)
            if (voiceRequestedChannel != null)
                voiceRequestedChannel.Subscribe(HandleVoiceRequested);
            
            if (voiceStopChannel != null)
                voiceStopChannel.Subscribe(StopVoice);
        }
        
        private void OnDisable()
        {
            // Unsubscribe dai channels
            if (voiceRequestedChannel != null)
                voiceRequestedChannel.Unsubscribe(HandleVoiceRequested);
            
            if (voiceStopChannel != null)
                voiceStopChannel.Unsubscribe(StopVoice);
        }
        
        private void Update()
        {
            // AudioSource.isPlaying è più robusto di Time.time:
            // gestisce pause, stop esterni e variazioni di timeScale
            if (_isPlaying && _voiceSource != null && !_voiceSource.isPlaying)
            {
                _isPlaying = false;
                
                // Pubblica evento: voice finished (VoiceManager è Publisher)
                if (voiceFinishedChannel != null)
                    voiceFinishedChannel.RaiseEvent();
            }
        }
        
        // ============================================
        // INITIALIZATION
        // ============================================
        
        private void BuildLookupTable()
        {
            _voiceLookup = new Dictionary<string, AudioClip>();
            
            if (_voiceClips != null)
            {
                foreach (var entry in _voiceClips)
                {
                    if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                    {
                        _voiceLookup[entry.id.ToLowerInvariant()] = entry.clip;
                    }
                }
            }
            
            Debug.Log($"[VoiceManager] Loaded {_voiceLookup.Count} voice clips.");
        }
        
        private void ConfigureAudioSource()
        {
            if (_voiceSource != null)
            {
                _voiceSource.playOnAwake = false;
                _voiceSource.loop = false;
                _voiceSource.volume = _voiceVolume;
            }
        }
        
        // ============================================
        // EVENT HANDLERS (Observer Reactions)
        // ============================================
        
        /// <summary>
        /// Quando arriva una richiesta di voice con ID esplicito.
        /// Subscribed a voiceRequestedChannel.
        /// </summary>
        private void HandleVoiceRequested(string voiceId)
        {
            if (string.IsNullOrEmpty(voiceId))
                return;
            
            PlayVoiceById(voiceId);
        }
        
        // ============================================
        // VOICE PLAYBACK
        // ============================================
        
        /// <summary>
        /// Riproduce una clip vocale per ID.
        /// </summary>
        public void PlayVoiceById(string voiceId)
        {
            if (string.IsNullOrEmpty(voiceId))
                return;
            
            string key = voiceId.ToLowerInvariant();
            
            if (_voiceLookup.TryGetValue(key, out AudioClip clip))
            {
                PlayVoice(clip);
                Debug.Log($"[VoiceManager] Playing: {voiceId}");
            }
            else
            {
                Debug.LogWarning($"[VoiceManager] Voice clip non trovata: {voiceId}");
            }
        }
        
        /// <summary>
        /// Riproduce una clip vocale.
        /// Pubblica evento VoiceStarted con durata.
        /// </summary>
        public void PlayVoice(AudioClip clip)
        {
            if (clip == null || _voiceSource == null)
            {
                if (_voiceSource == null)
                    Debug.LogWarning("[VoiceManager] _voiceSource non assegnato nell'Inspector!");
                return;
            }

            if (_voiceSource.isPlaying)
                _voiceSource.Stop();

            _voiceSource.clip = clip;
            _voiceSource.volume = _voiceVolume;
            _voiceSource.Play();

            _isPlaying = true;

            // Pubblica evento: voice started (VoiceManager è Publisher)
            if (voiceStartedChannel != null)
                voiceStartedChannel.RaiseEvent(clip.length);
        }
        
        /// <summary>
        /// Ferma la voce corrente.
        /// Subscribed a voiceStopChannel.
        /// </summary>
        public void StopVoice()
        {
            if (_voiceSource != null && _voiceSource.isPlaying)
            {
                _voiceSource.Stop();
                _isPlaying = false;
                
                // Pubblica evento: voice finished
                if (voiceFinishedChannel != null)
                    voiceFinishedChannel.RaiseEvent();
            }
        }
        
        // ============================================
        // PUBLIC API
        // ============================================
        
        public void SetVolume(float volume)
        {
            _voiceVolume = Mathf.Clamp01(volume);
            if (_voiceSource != null)
            {
                _voiceSource.volume = _voiceVolume;
            }
        }
        
        public bool IsPlaying => _isPlaying;

        public float RemainingTime => (_isPlaying && _voiceSource != null)
            ? Mathf.Max(0f, _voiceSource.clip.length - _voiceSource.time)
            : 0f;
    }
}