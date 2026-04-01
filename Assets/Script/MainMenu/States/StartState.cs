using UnityEngine;

public class StartState : BaseMenuState
{
    public StartState(MenuController controller) 
        : base(controller.StartPanel)
    {
    }
    
    // HandleBack usa il default di BaseMenuState (return true = Pop).
}
