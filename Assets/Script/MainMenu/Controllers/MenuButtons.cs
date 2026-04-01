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
        // Apre il pannello di selezione chiamate
        _menuController.PushState(new StartState(_menuController));
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
    // BOTTONI START PANEL (selezione chiamate)
    // ============================================
    
    public void OnDemoClicked()
    {
        _menuController.LoadDemo();
    }
    
    public void OnScene1Clicked()
    {
        _menuController.LoadScene1();
    }
    
    // ============================================
    // BOTTONI BACK (Options / Credits / Start)
    // ============================================
    
    public void OnBackClicked()
    {
        // Torna al pannello precedente (Pop dallo stack).
        _menuController.PopState();
    }
}