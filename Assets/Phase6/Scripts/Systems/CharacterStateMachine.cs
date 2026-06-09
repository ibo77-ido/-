using UnityEngine;

public class CharacterStateMachine : MonoBehaviour
{
    public CharacterState CurrentState { get; private set; } = CharacterState.Idle;

    public bool SetState(CharacterState newState)
    {
        if (!CanTransitionTo(newState))
        {
            Debug.Log($"[CharacterStateMachine] Transition blocked: {CurrentState} -> {newState}");
            return false;
        }
        CurrentState = newState;
        Debug.Log($"[CharacterStateMachine] State: {newState}");
        return true;
    }

    public bool CanTransitionTo(CharacterState target)
    {
        switch (CurrentState)
        {
            case CharacterState.Idle:
                return target == CharacterState.Moving || target == CharacterState.Interacting || target == CharacterState.UIOpen;
            case CharacterState.Moving:
                return target == CharacterState.Idle || target == CharacterState.Interacting || target == CharacterState.UIOpen;
            case CharacterState.Interacting:
            case CharacterState.UIOpen:
                return target == CharacterState.Idle;
            case CharacterState.Working:
                return target == CharacterState.Idle;
            default:
                return false;
        }
    }
}