using System.Collections.Generic;
using UnityEngine;

namespace DeadAir.Audio
{
    /// <summary>
    /// ScriptableObject contenente una collezione di AudioClip con ID associati.
    /// Permette di condividere librerie audio tra scene e sistemi.
    /// 
    /// Use Cases:
    /// - SFX_Common.asset → phone_ring, glass_break, door_open (tutti giochi)
    /// - SFX_Story_Iris.asset → clip specifici solo storia Iris
    /// - Ambience_Common.asset → dispatch_night, rain, etc.
    /// </summary>
    [CreateAssetMenu(
        fileName = "New Audio Clip Library",
        menuName = "DEAD AIR/Audio/Audio Clip Library"
    )]
    public class AudioClipLibrary : ScriptableObject
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================
        
        [Header("Library Info")]
        [SerializeField] private string libraryName = "Unnamed Library";
        [TextArea(2, 4)]
        [SerializeField] private string description = "Descrizione libreria audio";
        
        [Header("Audio Clips")]
        [SerializeField] private AudioEntry[] clips;
        
        // ============================================
        // SERIALIZABLE STRUCT
        // ============================================
        
        [System.Serializable]
        public struct AudioEntry
        {
            public string id;           // es. "phone_ring", "dispatch_night"
            public AudioClip clip;
            
            [TextArea(1, 2)]
            public string notes;        // Note opzionali per designer
        }
        
        // ============================================
        // PUBLIC API
        // ============================================
        
        /// <summary>
        /// Popola un dictionary esistente con i clip di questa library.
        /// Non sostituisce il dictionary, aggiunge/sovrascrive entry.
        /// </summary>
        /// <param name="targetDictionary">Dictionary da popolare</param>
        /// <param name="logCategory">Nome categoria per log (es. "SFX", "Ambience")</param>
        public void PopulateDictionary(Dictionary<string, AudioClip> targetDictionary, string logCategory)
        {
            if (targetDictionary == null)
            {
                Debug.LogError($"[AudioClipLibrary] Dictionary nullo passato a {name}");
                return;
            }
            
            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning($"[AudioClipLibrary] {name} non contiene clip");
                return;
            }
            
            int addedCount = 0;
            
            foreach (var entry in clips)
            {
                if (string.IsNullOrEmpty(entry.id))
                {
                    Debug.LogWarning($"[AudioClipLibrary] {name} contiene entry con ID vuoto");
                    continue;
                }
                
                if (entry.clip == null)
                {
                    Debug.LogWarning($"[AudioClipLibrary] {name} entry '{entry.id}' ha clip null");
                    continue;
                }
                
                string key = entry.id.ToLowerInvariant();
                
                // Overwrite se già esiste (last-wins per priorità library)
                targetDictionary[key] = entry.clip;
                addedCount++;
            }
            
            Debug.Log($"[AudioClipLibrary] {name} → {addedCount} {logCategory} clip caricati");
        }
        
        /// <summary>
        /// Restituisce tutti i clip come dizionario (utile per testing).
        /// </summary>
        public Dictionary<string, AudioClip> GetAllClips()
        {
            var result = new Dictionary<string, AudioClip>();
            
            if (clips != null)
            {
                foreach (var entry in clips)
                {
                    if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                    {
                        result[entry.id.ToLowerInvariant()] = entry.clip;
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Verifica se questa library contiene un clip con l'ID specificato.
        /// </summary>
        public bool Contains(string clipId)
        {
            if (string.IsNullOrEmpty(clipId) || clips == null)
                return false;
            
            string key = clipId.ToLowerInvariant();
            
            foreach (var entry in clips)
            {
                if (entry.id.ToLowerInvariant() == key)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Conta quanti clip validi contiene questa library.
        /// </summary>
        public int ClipCount
        {
            get
            {
                if (clips == null) return 0;
                
                int count = 0;
                foreach (var entry in clips)
                {
                    if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                        count++;
                }
                return count;
            }
        }
    }
}