namespace DeadAir.Narrative
{
    /// <summary>
    /// Comandi UI speciali attivati da tag.
    /// None = nessun comando (valore default).
    /// </summary>
    public enum UICommandType
    {
        None = 0, // Default
        DeadAirScreen = 1, // #ui:dead_air_screen
        ReturnToMenu = 2 // #ui:return_to_menu
        // Aggiungi il comando UI
    }
}