using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Phase3/Data/Game Config")]
public class GameConfigSO : ScriptableObject
{
    // Shape 评分参数
    [Header("Shape Score Parameters")]
    public float shapeDMax = 0.35f;
    public float[] shapeWeights = { 0.15f, 0.15f, 0.30f, 0.30f, 0.10f };

    // Glaze 评分参数
    [Header("Glaze Score Parameters")]
    public float glazeDMax = 0.02f;

    // Result 权重
    [Header("Result Weights")]
    public float shapeWeight = 0.35f;
    public float glazeWeight = 0.25f;
    public float fireWeight = 0.40f;

    // Grade 阈值
    [Header("Grade Thresholds")]
    public float gradeS_Min = 95f;
    public float gradeA_Min = 85f;
    public float gradeB_Min = 70f;
    public float gradeC_Min = 50f;
    public float gradeD_Min = 30f;

    // 奖励乘数
    [Header("Reward Multipliers")]
    public float[] qualityGoldMultipliers = { 2.0f, 1.5f, 1.2f, 1.0f, 0.5f, 0.0f };
    public float[] qualityRepMultipliers = { 1.5f, 1.2f, 1.0f, 0.8f, 0.3f, 0.0f };
    public float[] orderRepMultipliers = { 1.5f, 1.2f, 1.0f, 0.0f };
    public float[] difficultyGoldMultipliers = { 1.0f, 1.2f, 1.5f, 1.8f, 2.2f };
    public float[] difficultyRepMultipliers = { 1.0f, 1.1f, 1.3f, 1.5f, 1.8f };

    // Runtime Safety
    [Header("Runtime Safety")]
    public int goldMax = 9999999;
    public int repMax = 999999;
}
