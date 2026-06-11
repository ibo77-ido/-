/// <summary>
/// Interaction entry handler for Workstation.
/// Workstation only knows "someone pressed E at me",
/// it does not know about Gameplay, Session, or Runtime.
/// </summary>
public interface IInteractionEntryHandler
{
    void OnWorkstationInteracted(AreaType areaType);
}
