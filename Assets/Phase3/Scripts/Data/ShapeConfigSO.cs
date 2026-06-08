using UnityEngine;

[CreateAssetMenu(fileName = "Shape_Bowl", menuName = "Phase3/Data/Shape Config")]
public class ShapeConfigSO : ScriptableObject
{
    public string shapeID;           // "SHAPE_001" ~ "SHAPE_005"
    public string nameCN;
    public string nameEN;

    [Range(0f, 1f)] public float mouthRatio;
    [Range(0f, 1f)] public float neckRatio;
    [Range(0f, 1f)] public float shoulderRatio;
    [Range(0f, 1f)] public float bellyRatio;
    [Range(0f, 1f)] public float footRatio;

    // 特征描述（中文，用于 UI 展示）
    [TextArea] public string mouthFeatureCN;
    [TextArea] public string neckFeatureCN;
    [TextArea] public string shoulderFeatureCN;
    [TextArea] public string bellyFeatureCN;
    [TextArea] public string footFeatureCN;

    [TextArea(2, 4)] public string description;

    [Range(1, 5)] public int difficulty;
    public int unlockLevel;

    public ShapeType shapeType;      // 映射到现有枚举
}
