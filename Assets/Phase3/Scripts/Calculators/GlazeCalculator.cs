using UnityEngine;
using System.Collections.Generic;

public struct GlazeInput
{
    public float copper;              // 范围 [0, 0.02]
    public float iron;                // 范围 [0, 0.02]
    public float cobalt;              // 范围 [0, 0.02]
    public float? firingTempMax;      // 用于影青/冬青二级判定
}

public struct GlazeScoreResult
{
    public float overallScore;         // 0~100
    public string matchedGlazeID;      // 最佳匹配釉色ID
    public string matchedGlazeNameCN;
    public string matchedGlazeNameEN;
    public float dMin;                 // 最短欧几里得距离
    public bool requiresTempConfirm;   // 影青/冬青平局标记
}

public static class GlazeCalculator
{
    private const float DMax = 0.02f;
    private const float TempThreshold = 1280f;

    public static GlazeScoreResult Calculate(GlazeInput input, GlazeConfigSO[] templates, GameConfigSO config)
    {
        float dMax = config != null ? config.glazeDMax : DMax;

        float bestDistance = float.MaxValue;
        int bestIndex = -1;
        bool requiresTempConfirm = false;

        // Level 1: Cu/Fe/Co 欧几里得距离判定
        // 收集影青(GLAZE_001)和冬青(GLAZE_005)的候选结果用于二级判定
        float yingqingDist = float.MaxValue;
        float dongqingDist = float.MaxValue;
        int yingqingIndex = -1;
        int dongqingIndex = -1;

        for (int i = 0; i < templates.Length; i++)
        {
            if (templates[i] == null) continue;

            float d = EuclideanDistance(input, templates[i]);

            // 记录影青和冬青的距离
            if (templates[i].glazeID == "GLAZE_001") { yingqingDist = d; yingqingIndex = i; }
            if (templates[i].glazeID == "GLAZE_005") { dongqingDist = d; dongqingIndex = i; }

            if (d < bestDistance)
            {
                bestDistance = d;
                bestIndex = i;
            }
        }

        // Level 2: 影青/冬青平局 → 温度判定
        if (bestIndex >= 0 && yingqingIndex >= 0 && dongqingIndex >= 0)
        {
            string bestID = templates[bestIndex].glazeID;
            if ((bestID == "GLAZE_001" || bestID == "GLAZE_005") &&
                Mathf.Abs(yingqingDist - dongqingDist) < 0.0001f)
            {
                requiresTempConfirm = true;

                if (input.firingTempMax.HasValue)
                {
                    if (input.firingTempMax.Value <= TempThreshold)
                    {
                        bestIndex = yingqingIndex;
                    }
                    else
                    {
                        bestIndex = dongqingIndex;
                    }
                    requiresTempConfirm = false;
                }
            }
        }

        string matchedID = bestIndex >= 0 ? templates[bestIndex].glazeID : "UNKNOWN";
        string matchedCN = bestIndex >= 0 ? templates[bestIndex].nameCN : "未知";
        string matchedEN = bestIndex >= 0 ? templates[bestIndex].nameEN : "Unknown";

        float score = 100f * Mathf.Max(0f, 1f - bestDistance / dMax);

        return new GlazeScoreResult
        {
            overallScore = score,
            matchedGlazeID = matchedID,
            matchedGlazeNameCN = matchedCN,
            matchedGlazeNameEN = matchedEN,
            dMin = bestDistance,
            requiresTempConfirm = requiresTempConfirm
        };
    }

    private static float EuclideanDistance(GlazeInput input, GlazeConfigSO template)
    {
        float dc = input.copper - template.copper;
        float df = input.iron - template.iron;
        float dco = input.cobalt - template.cobalt;
        return Mathf.Sqrt(dc * dc + df * df + dco * dco);
    }
}
