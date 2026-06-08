using UnityEngine;

public enum GlazeColorType
{
    Oxidation,   // 氧化釉——影青、霁红
    Reduction,   // 还原釉——青花、甜白
    Dual         // 双性釉——冬青
}

[CreateAssetMenu(fileName = "Glaze_Yingqing", menuName = "Phase3/Data/Glaze Config")]
public class GlazeConfigSO : ScriptableObject
{
    public string glazeID;
    public string nameCN;
    public string nameEN;

    [Range(0f, 0.02f)] public float copper;
    [Range(0f, 0.02f)] public float iron;
    [Range(0f, 0.02f)] public float cobalt;

    public float temperatureMin;
    public float temperatureMax;

    public GlazeColorType colorType;

    public Color displayColor;           // P0: 釉色展示色值

    [TextArea(2, 4)] public string description;  // P1: 釉色简介

    [Range(1, 5)] public int difficulty;
}
