namespace DeadAir.Narrative
{
    // ============================================
    // ENUMS — Type Safety per Speaker e UI Commands
    // ============================================
    
    /// <summary>
    /// Tipo di speaker identificato dai tag.
    /// Unknown = valore di default sicuro (quando speaker non riconosciuto).
    /// </summary>
    public enum SpeakerType
    {
        Unknown = 0, // Default — sempre definisci valore 0!
        Narrator = 1, // Testo senza speaker specifico
        Ward = 2, // #speaker:ward
        Iris = 3 // #speaker:iris
        // Aggiungi lo speaker
    }
}