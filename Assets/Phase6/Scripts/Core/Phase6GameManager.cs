using UnityEngine;

public class Phase6GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private TestUIRouter testUIRouter;

    public Phase6GameState CurrentState { get; private set; } = Phase6GameState.Playing;

    public void SetState(Phase6GameState newState)
    {
        if (!CanTransitionTo(newState))
        {
            Debug.LogWarning($"[Phase6GameManager] State transition blocked: {CurrentState} -> {newState}");
            return;
        }

        CurrentState = newState;
        Debug.Log($"[Phase6GameManager] State changed to: {newState}");

        if (playerCharacter != null)
        {
            CharacterState charState = newState == Phase6GameState.Playing ? CharacterState.Idle
                : newState == Phase6GameState.UIOpen ? CharacterState.UIOpen
                : CharacterState.Idle;

            var stateMachine = playerCharacter.GetComponent<CharacterStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.SetState(charState);
            }
        }
    }

    public bool CanTransitionTo(Phase6GameState target)
    {
        switch (CurrentState)
        {
            case Phase6GameState.Playing:
                return target == Phase6GameState.UIOpen;
            case Phase6GameState.UIOpen:
                return target == Phase6GameState.Playing;
            default:
                return false;
        }
    }

    public bool CanMove()
    {
        return CurrentState == Phase6GameState.Playing;
    }

    public bool CanInteract()
    {
        return CurrentState == Phase6GameState.Playing;
    }
}