using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DeadAir.Narrative
{
    /// <summary>
    /// Parsing del testo e dei tag provenienti dal file ink.
    /// Logica pura, nessuna dipendenza Unity — facilmente testabile.
    /// 
    /// Responsabilità:
    /// - Estrarre speaker dalle linee (#speaker:ward, #speaker:iris)
    /// - Parsare tag audio (#sfx:xxx, #amb:xxx)
    /// - Parsare tag voice (#voice:iris_01)
    /// - Parsare tag timer (#timed_choice, #timeout:x, #default:x)
    /// - Parsare tag UI (#ui:xxx)
    /// </summary>
    public static class DialogueParser
    {
        // ============================================
        // TAG PREFIXES
        // ============================================

        private const string TAG_SPEAKER = "speaker:";
        private const string TAG_SFX = "sfx:";
        private const string TAG_AMB = "amb:";
        private const string TAG_UI = "ui:";
        private const string TAG_VOICE = "voice:";
        private const string TAG_TIMED_CHOICE = "timed_choice";
        private const string TAG_TIMEOUT = "timeout:";
        private const string TAG_DEFAULT = "default:";

        // ============================================
        // PARSED RESULT STRUCT
        // ============================================

        /// <summary>
        /// Risultato del parsing di una linea di dialogo.
        /// Struct invece di class: zero allocazioni heap, 
        /// ideale per chiamate frequenti.
        /// </summary>
        public struct ParsedLine
        {
            public string Text;
            public string Speaker;          // null se nessuno speaker
            public bool HasSpeaker;

            public string SFX;              // null se nessun sfx
            public bool HasSFX;

            public string Ambience;         // null se nessuna ambience
            public bool HasAmbience;
            public bool IsAmbienceStop;     // true se #amb:stop

            public string UICommand;        // null se nessun comando UI
            public bool HasUICommand;

            public string VoiceId;          // null se nessun voice clip
            public bool HasVoice;
        }

        /// <summary>
        /// Risultato del parsing dei tag di una scelta con timer.
        /// </summary>
        public struct TimedChoiceData
        {
            public bool IsTimedChoice { get; set; }
            public float Timeout { get; set; }
            public int DefaultIndex { get; set; }
            public int choiceIndex { get; set; }
        }

        // ============================================
        // MAIN PARSING METHODS
        // ============================================

        /// <summary>
        /// Parsa una lista di tag ink e restituisce i dati strutturati.
        /// 
        /// Perché List<string>: ink restituisce i tag come lista,
        /// iteriamo una sola volta per estrarre tutto.
        /// </summary>
        public static ParsedLine ParseTags(List<string> tags, string lineText)
        {
            var result = new ParsedLine
            {
                Text = lineText?.Trim() ?? string.Empty,
                Speaker = null,
                HasSpeaker = false,
                SFX = null,
                HasSFX = false,
                Ambience = null,
                HasAmbience = false,
                IsAmbienceStop = false,
                UICommand = null,
                HasUICommand = false,
                VoiceId = null,
                HasVoice = false
            };

            if (tags == null || tags.Count == 0)
                return result;

            // Single pass attraverso i tag
            foreach (string tag in tags)
            {
                string trimmedTag = tag.Trim().ToLowerInvariant();

                // Speaker tag
                if (trimmedTag.StartsWith(TAG_SPEAKER))
                {
                    result.Speaker = ExtractValue(trimmedTag, TAG_SPEAKER);
                    result.HasSpeaker = !string.IsNullOrEmpty(result.Speaker);
                }
                // Voice tag
                else if (trimmedTag.StartsWith(TAG_VOICE))
                {
                    result.VoiceId = ExtractValue(trimmedTag, TAG_VOICE);
                    result.HasVoice = !string.IsNullOrEmpty(result.VoiceId);
                }
                // SFX tag
                else if (trimmedTag.StartsWith(TAG_SFX))
                {
                    result.SFX = ExtractValue(trimmedTag, TAG_SFX);
                    result.HasSFX = !string.IsNullOrEmpty(result.SFX);
                }
                // Ambience tag
                else if (trimmedTag.StartsWith(TAG_AMB))
                {
                    string ambienceValue = ExtractValue(trimmedTag, TAG_AMB);

                    if (ambienceValue == "stop")
                    {
                        result.IsAmbienceStop = true;
                        result.HasAmbience = true;
                    }
                    else
                    {
                        result.Ambience = ambienceValue;
                        result.HasAmbience = !string.IsNullOrEmpty(result.Ambience);
                    }
                }
                // UI tag
                else if (trimmedTag.StartsWith(TAG_UI))
                {
                    result.UICommand = ExtractValue(trimmedTag, TAG_UI);
                    result.HasUICommand = !string.IsNullOrEmpty(result.UICommand);
                }
            }

            return result;
        }

        /// <summary>
        /// Parsa i tag di una scelta per verificare se è una timed choice.
        /// Usato quando si presentano le scelte al giocatore.
        /// </summary>
        public static TimedChoiceData ParseTimedChoiceTags(List<string> tags)
        {
            var result = new TimedChoiceData
            {
                IsTimedChoice = false,
                Timeout = 4f,       // default 4 secondi
                DefaultIndex = 0    // default prima scelta
            };

            if (tags == null || tags.Count == 0)
                return result;

            foreach (string tag in tags)
            {
                string trimmedTag = tag.Trim().ToLowerInvariant();

                // 🔍 DEBUG TEMPORANEO
                Debug.Log($"[DialogueParser] Parsing tag: '{tag}' -> '{trimmedTag}'");
                Debug.Log($"[DialogueParser] Is timed_choice? {trimmedTag == TAG_TIMED_CHOICE}");

                // Timed choice flag
                if (trimmedTag == TAG_TIMED_CHOICE)
                {
                    Debug.Log("[DialogueParser] ✅ TIMED CHOICE TROVATA!");
                    result.IsTimedChoice = true;
                }
                // Timeout value
                else if (trimmedTag.StartsWith(TAG_TIMEOUT))
                {
                    string timeoutStr = ExtractValue(trimmedTag, TAG_TIMEOUT);
                    if (float.TryParse(timeoutStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float timeout))
                    {
                        result.Timeout = timeout;
                    }
                }
                // Default choice index
                else if (trimmedTag.StartsWith(TAG_DEFAULT))
                {
                    string defaultStr = ExtractValue(trimmedTag, TAG_DEFAULT);
                    if (int.TryParse(defaultStr, out int defaultIndex))
                    {
                        result.DefaultIndex = defaultIndex;
                    }
                }
            }

            return result;
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        /// <summary>
        /// Estrae il valore dopo il prefisso del tag.
        /// Es: "speaker:iris" -> "iris"
        /// 
        /// Perché Substring invece di Split: 
        /// - Zero allocazioni array
        /// - Più veloce per operazione semplice
        /// </summary>
        private static string ExtractValue(string tag, string prefix)
        {
            if (tag.Length <= prefix.Length)
                return string.Empty;

            return tag.Substring(prefix.Length).Trim();
        }

        /// <summary>
        /// Verifica se una linea è solo puntini di sospensione.
        /// Usato per pause narrative ("...").
        /// </summary>
        public static bool IsEllipsis(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            string trimmed = text.Trim();

            // Controlla se è solo punti (., .., ..., etc)
            foreach (char c in trimmed)
            {
                if (c != '.')
                    return false;
            }

            return trimmed.Length > 0;
        }

        /// <summary>
        /// Pulisce il testo rimuovendo spazi extra e normalizzando.
        /// </summary>
        public static string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Trim e normalizza spazi multipli
            return System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"\s+", " ");
        }
    }
}
