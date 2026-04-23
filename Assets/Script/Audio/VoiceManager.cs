using System.Collections.Generic;
using UnityEngine;
using DeadAir.Events;
using System;

namespace DeadAir.Audio
{
    /// <summary>
    /// Gestisce la riproduzione delle voci dei personaggi usando AudioClipLibrary SO.
    /// Supporta multiple libraries per speaker diversi.
    /// NON Singleton: ogni scena ha il proprio VoiceManager.
    /// </summary>
    public class VoiceManager : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================

        [Header("Audio Source")]
        [SerializeField] private AudioSource _voiceSource;

        [Header("Voice Libraries (ScriptableObject)")]
        [Tooltip("Libraries voice da caricare per questa scena (es. Voice_Iris, Voice_Ward)")]
        [SerializeField] private AudioClipLibrary[] _voiceLibraries;

        [Header("Settings")]
        [SerializeField][Range(0f, 1f)] private float _voiceVolume = 1f;
        [SerializeField][Range(0f, 2f)] private float _voiceFadeDuration = 0.15f;

        // ============================================
        // EVENT CHANNELS (Dependency Injection)
        // ============================================

        [Header("Input Channels (VoiceManager è Observer)")]
        [SerializeField] private StringEventChannel voiceRequestedChannel;
        [SerializeField] private VoidEventChannel voiceStopChannel;

        [Header("Output Channels (VoiceManager è Publisher)")]
        [SerializeField] private FloatEventChannel voiceStartedChannel;
        [SerializeField] private VoidEventChannel voiceFinishedChannel;
        [SerializeField] private FadeSystem _fadeSystem;

        // ============================================
        // PRIVATE STATE
        // ============================================

        private Dictionary<string, AudioClip> _voiceLookup;
        private bool _isPlaying;

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

            LoadLibraries();
        }

        /// <summary>
        /// Carica tutte le voice libraries assegnate nell'Inspector.
        /// </summary>
        private void LoadLibraries()
        {
            if (_voiceLibraries == null || _voiceLibraries.Length == 0)
            {
                Debug.LogWarning("[VoiceManager] Nessuna Voice Library assegnata!");
                return;
            }

            foreach (var library in _voiceLibraries)
            {
                if (library != null)
                {
                    library.PopulateDictionary(_voiceLookup, "Voice");
                }
            }

            Debug.Log($"[VoiceManager] Totale caricato: {_voiceLookup.Count} voice clips");
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
                StartCoroutine(_fadeSystem.FadeOut(_voiceSource, _voiceFadeDuration, () =>
                {
                    _isPlaying = false;

                    // Pubblica evento: voice finished
                    if (voiceFinishedChannel != null)
                        voiceFinishedChannel.RaiseEvent();
                }));
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