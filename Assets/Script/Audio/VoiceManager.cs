using System.Collections.Generic;
using UnityEngine;
using DeadAir.Events;

namespace DeadAir.Audio
{
    /// <summary>
    /// Gestisce la riproduzione delle voci dei personaggi.
    /// Separato da AudioManager per gestire la sincronizzazione con il typewriter.
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
        // Gli eventi OnVoiceStarted e OnVoiceFinished sono in NarrativeEvents
        // per mantenere il contratto dell'event hub centralizzato.
        
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
            NarrativeEvents.OnVoiceRequested += HandleVoiceRequested;
        }
        
        private void OnDisable()
        {
            NarrativeEvents.OnVoiceRequested -= HandleVoiceRequested;
        }
        
        private void Update()
        {
            // AudioSource.isPlaying è più robusto di Time.time:
            // gestisce pause, stop esterni e variazioni di timeScale
            if (_isPlaying && _voiceSource != null && !_voiceSource.isPlaying)
            {
                _isPlaying = false;
                NarrativeEvents.VoiceFinished();
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
        // EVENT HANDLERS
        // ============================================
        
        /// <summary>
        /// Quando arriva una richiesta di voice con ID esplicito.
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

            NarrativeEvents.VoiceStarted(clip.length);
        }
        
        /// <summary>
        /// Ferma la voce corrente.
        /// </summary>
        public void StopVoice()
        {
            if (_voiceSource != null && _voiceSource.isPlaying)
            {
                _voiceSource.Stop();
                _isPlaying = false;
                NarrativeEvents.VoiceFinished();
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
