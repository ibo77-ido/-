using UnityEngine;

[CreateAssetMenu(fileName = "HUDQuickBarConfig", menuName = "Phase11/HUD QuickBar Config")]
public class HUDQuickBarConfigSO : ScriptableObject
{
    [Header("Button Definitions")]
    public HUDButtonData[] buttons;

    [Header("Timing")]
    [Range(5f, 30f)] public float autoInteractTimeout = 15f;
}
