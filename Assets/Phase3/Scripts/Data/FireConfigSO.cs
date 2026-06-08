using UnityEngine;

[CreateAssetMenu(fileName = "FireConfig", menuName = "Phase3/Data/Fire Config")]
public class FireConfigSO : ScriptableObject
{
    public KilnStageConfig[] stages = new KilnStageConfig[8];
    public FlameSwitchConfig[] flameSwitches = new FlameSwitchConfig[4];
    public DefectConfig[] defects = new DefectConfig[11];
}

[System.Serializable]
public class KilnStageConfig
{
    public string stageId;
    public string stageNameCN;
    public string stageNameEN;
    public float tempStart;
    public float tempEnd;
    public float durationMin;
    public float durationMax;
    public float durationMedian;
    public string atmosphere;
    public int penaltyCap;
}

[System.Serializable]
public class FlameSwitchConfig
{
    public string switchId;          // FS1 ~ FS4
    public string fromColorCN;
    public string toColorCN;
    public int penaltyPoints;
}

[System.Serializable]
public class DefectConfig
{
    public string defectId;          // D1 ~ D11
    public string nameCN;
    public string nameEN;
    public bool isFatal;
    public int penaltyPoints;
    public bool isStackable;
    public string triggerCondition;
}
