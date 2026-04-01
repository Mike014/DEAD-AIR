// Contratto per tutti gli stati del menu.
public interface IMenuState
{
    void Enter(MenuController controller);
    void Exit(MenuController controller);
    void Update(MenuController controller);
    bool HandleBack(MenuController controller);
}