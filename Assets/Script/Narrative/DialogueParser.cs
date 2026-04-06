#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        // ============================================
        // PARSED RESULT STRUCT
        // ============================================

        /// <summary>
        /// Risultato del parsing di una linea di dialogo.
        /// 
        /// READONLY STRUCT perché:
        /// - Immutabile dopo creazione (no mutazioni accidentali di copie)
        /// - Passata frequentemente tra metodi (zero allocazioni heap)
        /// - Dati piccoli e temporanei (semantica di valore)
        /// 
        /// PROPRIETÀ NULLABLE perché:
        /// - Speaker/Voice/SFX possono essere assenti
        /// - Nullable esplicita l'opzionalità (meglio di stringa vuota)
        /// - Elimina ridondanza dei bool HasX (derivati automaticamente)
        /// </summary>
        public readonly struct ParsedLine
        {
            // ============================================
            // DATI PRIMARI
            // ============================================

            public string Text { get; init; }

            // Speaker
            public string? Speaker { get; init; }
            public SpeakerType SpeakerType { get; init; }

            // Audio
            public string? VoiceId { get; init; }
            public string? SFX { get; init; }
            public string? Ambience { get; init; }
            public bool IsAmbienceStop { get; init; }

            // UI
            public UICommandType UICommand { get; init; }

            /// <summary>True se la linea ha uno speaker identificato.</summary>
            public bool HasSpeaker => Speaker != null;

            /// <summary>True se la linea ha un voice clip associato.</summary>
            public bool HasVoice => VoiceId != null;

            /// <summary>True se la linea ha un effetto sonoro.</summary>
            public bool HasSFX => SFX != null;

            /// <summary>True se la linea ha ambience (start o stop).</summary>
            public bool HasAmbience => Ambience != null || IsAmbienceStop;

            /// <summary>True se la linea ha un comando UI.</summary>
            public bool HasUICommand => UICommand != UICommandType.None;
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
                SpeakerType = SpeakerType.Unknown,
                VoiceId = null,
                SFX = null,
                Ambience = null,
                IsAmbienceStop = false,
                UICommand = UICommandType.None
            };

            if (tags == null || tags.Count == 0)
                return result;

           // Single pass attraverso i tag
            foreach (string tag in tags)
            {
                string trimmedTag = tag.Trim().ToLowerInvariant();

                // ============================================
                // SPEAKER TAG
                // ============================================
                if (trimmedTag.StartsWith(TAG_SPEAKER))
                {
                    string? speakerValue = ExtractValue(trimmedTag, TAG_SPEAKER);
                    
                    SpeakerType speakerType = speakerValue?.ToLowerInvariant() switch
                    {
                        "ward" => SpeakerType.Ward,
                        "iris" => SpeakerType.Iris,
                        _ => SpeakerType.Unknown
                    };

                    // Crea nuova struct copiando tutti i campi (C# 9 compatibile)
                    result = new ParsedLine
                    {
                        Text = result.Text,
                        Speaker = speakerValue,
                        SpeakerType = speakerType,
                        VoiceId = result.VoiceId,
                        SFX = result.SFX,
                        Ambience = result.Ambience,
                        IsAmbienceStop = result.IsAmbienceStop,
                        UICommand = result.UICommand
                    };
                }
                // ============================================
                // VOICE TAG
                // ============================================
                else if (trimmedTag.StartsWith(TAG_VOICE))
                {
                    string? voiceValue = ExtractValue(trimmedTag, TAG_VOICE);
                    
                    result = new ParsedLine
                    {
                        Text = result.Text,
                        Speaker = result.Speaker,
                        SpeakerType = result.SpeakerType,
                        VoiceId = voiceValue,
                        SFX = result.SFX,
                        Ambience = result.Ambience,
                        IsAmbienceStop = result.IsAmbienceStop,
                        UICommand = result.UICommand
                    };
                }
                // ============================================
                // SFX TAG
                // ============================================
                else if (trimmedTag.StartsWith(TAG_SFX))
                {
                    string? sfxValue = ExtractValue(trimmedTag, TAG_SFX);
                    
                    result = new ParsedLine
                    {
                        Text = result.Text,
                        Speaker = result.Speaker,
                        SpeakerType = result.SpeakerType,
                        VoiceId = result.VoiceId,
                        SFX = sfxValue,
                        Ambience = result.Ambience,
                        IsAmbienceStop = result.IsAmbienceStop,
                        UICommand = result.UICommand
                    };
                }
                // ============================================
                // AMBIENCE TAG
                // ============================================
                else if (trimmedTag.StartsWith(TAG_AMB))
                {
                    string? ambienceValue = ExtractValue(trimmedTag, TAG_AMB);

                    if (ambienceValue?.ToLowerInvariant() == "stop")
                    {
                        result = new ParsedLine
                        {
                            Text = result.Text,
                            Speaker = result.Speaker,
                            SpeakerType = result.SpeakerType,
                            VoiceId = result.VoiceId,
                            SFX = result.SFX,
                            Ambience = result.Ambience,
                            IsAmbienceStop = true,  // Solo questo cambia
                            UICommand = result.UICommand
                        };
                    }
                    else
                    {
                        result = new ParsedLine
                        {
                            Text = result.Text,
                            Speaker = result.Speaker,
                            SpeakerType = result.SpeakerType,
                            VoiceId = result.VoiceId,
                            SFX = result.SFX,
                            Ambience = ambienceValue,  // Solo questo cambia
                            IsAmbienceStop = result.IsAmbienceStop,
                            UICommand = result.UICommand
                        };
                    }
                }
                // ============================================
                // UI TAG
                // ============================================
                else if (trimmedTag.StartsWith(TAG_UI))
                {
                    string? uiValue = ExtractValue(trimmedTag, TAG_UI);
                    
                    UICommandType commandType = uiValue?.ToLowerInvariant() switch
                    {
                        "dead_air_screen" => UICommandType.DeadAirScreen,
                        "return_to_menu" => UICommandType.ReturnToMenu,
                        _ => UICommandType.None
                    };

                    result = new ParsedLine
                    {
                        Text = result.Text,
                        Speaker = result.Speaker,
                        SpeakerType = result.SpeakerType,
                        VoiceId = result.VoiceId,
                        SFX = result.SFX,
                        Ambience = result.Ambience,
                        IsAmbienceStop = result.IsAmbienceStop,
                        UICommand = commandType
                    };
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
        private static string? ExtractValue(string tag, string prefix)
        {
            if (tag.Length <= prefix.Length)
            {
                return null;
            }

            string value = tag.Substring(prefix.Length).Trim();

            // Stringa vuota dopo trim = nessun valore reale
            return value.Length > 0 ? value : null;
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
