using UnityEngine;

[CreateAssetMenu(fileName = "AssetScaleConfig", menuName = "Phase6/Asset Scale Config")]
public class AssetScaleConfigSO : ScriptableObject
{
    [Header("Scale Settings")]
    [Range(0.1f, 5f)] public float characterScale = 1f;
    [Range(0.1f, 5f)] public float workstationScale = 1f;
    [Range(0.1f, 5f)] public float buildingScale = 1f;
}