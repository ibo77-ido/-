public interface IInteractable
{
    void Interact(ICharacter actor);
    AreaType AreaType { get; }
}