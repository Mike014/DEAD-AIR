using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    // Riferimento al controller (assegnato da Inspector).
    [SerializeField] private MenuController _menuController;
    
    // ============================================
    // BOTTONI MAIN MENU
    // ============================================
    
    public void OnStartGameClicked()
    {
        _menuController.StartGame();
    }
    
    public void OnOptionsClicked()
    {
        _menuController.PushState(new OptionsState(_menuController));
    }
    
    public void OnCreditsClicked()
    {
        _menuController.PushState(new CreditsState(_menuController));
    }
    
    public void OnQuitClicked()
    {
        _menuController.QuitGame();
    }
    
    // ============================================
    // BOTTONI OPTIONS / CREDITS
    // ============================================
    
    public void OnBackClicked()
    {
        // Torna al pannello precedente (Pop dallo stack).
        _menuController.PopState();
    }
}
