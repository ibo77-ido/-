/// <summary>
/// Interface for coordinating Phase3 progression with an external runtime.
/// Defined in Phase3 because it declares a capability Phase3 needs.
/// Implemented by GameplayBridgeManager (Phase8) to grant or deny auto-advance
/// and receive module completion notifications.
///
/// Dependency direction: Phase8 implements Phase3's interface.
/// Phase3 never references Phase6 or Phase8 types — only this interface.
/// </summary>
public interface IGameplayProgressionAuthority
{
    bool CanAutoAdvanceGameplay();
    void NotifyGameplayModuleCompleted(GameState completedState);
}
