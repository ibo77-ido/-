using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Phase6/Character Config")]
public class CharacterConfigSO : ScriptableObject
{
    [Header("Movement")]
    [Range(0.1f, 20f)] public float moveSpeed = 5f;
    [Range(0.1f, 10f)] public float stoppingDistance = 0.5f;
    [Range(0.1f, 20f)] public float acceleration = 8f;

    [Header("Interaction")]
    [Range(0.5f, 10f)] public float interactionRange = 2f;
}