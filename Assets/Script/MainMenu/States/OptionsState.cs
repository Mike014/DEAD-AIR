using UnityEngine;

public class OptionsState : BaseMenuState
{
    public OptionsState(MenuController controller) 
        : base(controller.OptionsPanel)
    {
    }
    
    // HandleBack usa il default di BaseMenuState (return true = Pop).
}