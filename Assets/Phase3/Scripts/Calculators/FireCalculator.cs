using UnityEngine;
using System.Collections.Generic;

public struct FireInput
{
    public float[] temperatureReadings;   // 温度读数序列
    public float[] timeStamps;            // 对应时间戳
    public float[] stageDurations;        // [8] S1~S8 实际时长（小时）
    public float reductionSwitchTemp;     // 还原气氛切换温度
    public bool reductionWasSwitched;     // 是否已切换还原气氛
    public bool[] flameJudgments;         // [4] FS1~FS4 是否错误
    public float stopFireTemp;            // 停火温度
    public float stopFireTime;            // 停火时间（从开始到停火的总时长）
    public float kilnOpenTemp;            // 开窑温度
    public List<string> triggeredDefectIds; // 已触发的缺陷 ID 列表（D1~D11）
}

public struct FireScoreResult
{
    public float fireScore;              // 0~100
    public string temperatureZone;       // 欠烧/正常/过烧
    public float tempPenalty;            // T
    public float durationPenalty;        // D
    public float flamePenalty;           // F
    public float defectPenalty;          // P
    public List<PenaltySource> penalties;
    public bool isFatalDefect;
}

public enum FeedbackLevel
{
    Info,
    Warning,
    Error
}

public struct PenaltySource
{
    public string sourceId;
    public string sourceNameCN;
    public string descriptionCN;
    public float penalty;
    public string suggestionCN;
    public FeedbackLevel feedbackLevel;
}

public static class FireCalculator
{
    // 温度区间阈值
    private const float UnderfiredMax = 1000f;
    private const float NormalMin = 1000f;
    private const float NormalMax = 1300f;

    // 6个温度检查点 ID
    private static readonly string[] CpIds = { "CP1", "CP2", "CP3", "CP4", "CP5", "CP6" };

    public static FireScoreResult Calculate(FireInput input, FireConfigSO config)
    {
        float tempPenalty = CalcTempPenalty(input, config);
        float durationPenalty = CalcDurationPenalty(input, config);
        float flamePenalty = CalcFlamePenalty(input, config);
        List<PenaltySource> penalties = new List<PenaltySource>();
        float defectPenalty = CalcDefectPenalty(input, config, out bool isFatal, penalties);

        // PenaltySource 机制：同一 Source 的 CP 扣分与缺陷扣分不叠加
        ApplyPenaltySourceNonStacking(penalties, tempPenalty);

        float totalPenalty = tempPenalty + durationPenalty + flamePenalty + defectPenalty;
        float fireScore = Mathf.Max(0f, 100f - totalPenalty);

        // 致命缺陷强制归零
        if (isFatal)
        {
            fireScore = 0f;
        }

        string tempZone = DetermineTemperatureZone(input.stopFireTemp);

        // Debug 观测输出
        Debug.Log($"[FireCalculator] T={tempPenalty:F2} D={durationPenalty:F2} F={flamePenalty:F2} P={defectPenalty:F2} | Total={totalPenalty:F2} | Score={fireScore:F1} | Zone={tempZone} | Fatal={isFatal}");

        return new FireScoreResult
        {
            fireScore = fireScore,
            temperatureZone = tempZone,
            tempPenalty = tempPenalty,
            durationPenalty = durationPenalty,
            flamePenalty = flamePenalty,
            defectPenalty = defectPenalty,
            penalties = penalties,
            isFatalDefect = isFatal
        };
    }

    private static float CalcTempPenalty(FireInput input, FireConfigSO config)
    {
        // CP1: ≤100°C/h 升温速率 → 上限 5
        // CP2: 573±20°C 石英转化 → 上限 3
        // CP3: 1000±30°C 气氛切换 → 上限 7
        // CP4: 1280~1320°C 成熟窗口 → 上限 10
        // CP5: 573°C 冷却 → 上限 3
        // CP6: 226°C 开窑时机 → 上限 7
        // 简化实现：基于停火温度判定
        float penalty = 0f;

        float temp = input.stopFireTemp;

        if (temp < UnderfiredMax)
        {
            // 欠烧：离 1000°C 越远扣分越多
            float ratio = (UnderfiredMax - temp) / UnderfiredMax;
            penalty = Mathf.Clamp(ratio * 35f, 0f, 35f);
        }
        else if (temp > NormalMax)
        {
            // 过烧：离 1300°C 越远扣分越多
            float overRatio = (temp - NormalMax) / 200f;
            penalty = Mathf.Clamp(overRatio * 35f, 0f, 35f);
        }
        else
        {
            // 正常区间内有小扣分（越接近 1280~1320 目标窗口扣分越少）
            float targetCenter = 1300f;
            float deviation = Mathf.Abs(temp - targetCenter);
            penalty = (deviation / 100f) * 5f; // 最大 5 分
        }

        return Mathf.Clamp(penalty, 0f, 35f);
    }

    private static float CalcDurationPenalty(FireInput input, FireConfigSO config)
    {
        // 总时长和 S1~S8 各阶段时长检查
        float totalDuration = 0f;
        float recommendedTotal = 0f;

        if (input.stageDurations != null && config != null && config.stages != null)
        {
            for (int i = 0; i < Mathf.Min(input.stageDurations.Length, config.stages.Length); i++)
            {
                totalDuration += input.stageDurations[i];
                recommendedTotal += config.stages[i].durationMedian;
            }
        }

        if (totalDuration <= 0f) return 0f;

        float ratio = totalDuration / recommendedTotal;
        float penalty;

        if (ratio < 0.5f) penalty = 20f;           // 严重不足
        else if (ratio < 0.8f) penalty = 10f;       // 偏短
        else if (ratio > 1.5f) penalty = 15f;        // 偏长
        else if (ratio > 1.2f) penalty = 5f;         // 略长
        else penalty = 0f;                           // 正常

        return Mathf.Clamp(penalty, 0f, 20f);
    }

    private static float CalcFlamePenalty(FireInput input, FireConfigSO config)
    {
        if (input.flameJudgments == null) return 0f;

        float penalty = 0f;
        int[] flamePenalties = { 3, 8, 3, 6 }; // FS1~FS4 固定扣分

        for (int i = 0; i < Mathf.Min(input.flameJudgments.Length, 4); i++)
        {
            if (input.flameJudgments[i])
            {
                penalty += flamePenalties[i];
            }
        }

        return Mathf.Clamp(penalty, 0f, 20f);
    }

    private static float CalcDefectPenalty(FireInput input, FireConfigSO config, out bool isFatal, List<PenaltySource> penalties)
    {
        float total = 0f;
        isFatal = false;

        if (config?.defects == null) return 0f;
        if (input.triggeredDefectIds == null || input.triggeredDefectIds.Count == 0) return 0f;

        var idSet = new HashSet<string>(input.triggeredDefectIds);
        foreach (var defect in config.defects)
        {
            if (!idSet.Contains(defect.defectId)) continue;

            if (defect.isFatal) isFatal = true;
            total += defect.penaltyPoints;

            penalties.Add(new PenaltySource
            {
                sourceId = defect.defectId,
                sourceNameCN = defect.nameCN,
                descriptionCN = defect.isFatal ? "致命缺陷" : "一般缺陷",
                penalty = defect.penaltyPoints,
                suggestionCN = defect.isFatal ? "烧制失败" : "注意控制烧制参数",
                feedbackLevel = defect.isFatal ? FeedbackLevel.Error : FeedbackLevel.Warning
            });
        }

        return Mathf.Clamp(total, 0f, 25f);
    }

    private static void ApplyPenaltySourceNonStacking(List<PenaltySource> penalties, float cpPenalty)
    {
        // V1.1 PenaltySource 机制：同一 Source 的 CP 扣分与缺陷扣分不叠加
        // 取 max(CP扣分, 缺陷扣分)
        // 当前简化实现：CP 扣分以整体 tempPenalty 计入，未拆分为独立 PenaltySource
        // 当 CP 也拆为独立 PenaltySource 后，按 sourceId 前缀分组取 max
        // TODO: Phase5 完善 — 重构 CalcTempPenalty 输出按 CP 拆分的 PenaltySource
    }

    private static string DetermineTemperatureZone(float temp)
    {
        if (temp < UnderfiredMax) return "欠烧";
        if (temp <= NormalMax) return "正常";
        return "过烧";
    }
}
