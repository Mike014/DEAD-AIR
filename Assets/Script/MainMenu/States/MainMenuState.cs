using UnityEngine;

public class MainMenuState : BaseMenuState
{
    public MainMenuState(MenuController controller) 
        : base(controller.MainMenuPanel)
    {
    }

    public override bool HandleBack(MenuController controller)
    {
        // MainMenu è lo stato root.
        // Back qui viene ignorato (non fare Pop).
        return false;
    }
}