using UnityEngine;

public class CreditsState : BaseMenuState
{
    public CreditsState(MenuController controller) 
        : base(controller.CreditsPanel)
    {
    }
    
    // HandleBack usa il default di BaseMenuState (return true = Pop).
}