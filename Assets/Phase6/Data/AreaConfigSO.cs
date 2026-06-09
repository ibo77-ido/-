using UnityEngine;

[CreateAssetMenu(fileName = "AreaConfig", menuName = "Phase6/Area Config")]
public class AreaConfigSO : ScriptableObject
{
    [Header("Identity")]
    public AreaType areaType = AreaType.Order;
    public string areaName = "Order Area";

    [Header("Bounds")]
    public Vector3 boundsCenter = Vector3.zero;
    public Vector3 boundsSize = new Vector3(20f, 1f, 26f);
}