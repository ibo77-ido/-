using UnityEngine;

public interface ICharacter
{
    Transform Transform { get; }
    CharacterState CurrentState { get; }
}

public enum CharacterState
{
    Idle,
    Moving,
    Interacting,
    UIOpen,
    Working
}