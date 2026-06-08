using UnityEngine;

[CreateAssetMenu(fileName = "OrderData", menuName = "Phase3/Data/Order Data")]
public class OrderData : ScriptableObject
{
    public string orderName;

    // 需求参数（仅保存 ID，不引用玩家配方）
    public string orderID;
    public string requiredShapeID;      // "SHAPE_001" ~ "SHAPE_005"
    public string requiredGlazeID;      // "GLAZE_001" ~ "GLAZE_005"

    [Range(1, 5)] public int difficulty = 1;
    public int baseGold;
    public int baseReputation;
}
