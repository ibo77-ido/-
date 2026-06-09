using UnityEngine;

[CreateAssetMenu(fileName = "WorkstationConfig", menuName = "Phase6/Workstation Config")]
public class WorkstationConfigSO : ScriptableObject
{
    [Header("Identity")]
    public string stationName = "Workstation";
    public AreaType areaType = AreaType.Order;

    [Header("Interaction")]
    [Range(0.5f, 5f)] public float interactionDistance = 1.5f;
    public string promptText = "Press E to interact";

    [Header("Visual")]
    public GameObject defaultVisualPrefab;
    [Range(0.1f, 10f)] public float scale = 1f;
}