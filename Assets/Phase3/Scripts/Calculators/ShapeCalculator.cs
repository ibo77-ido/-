using UnityEngine;

public struct ShapeInput
{
    public float mouth;
    public float neck;
    public float shoulder;
    public float belly;
    public float foot;
}

public struct ShapeScoreResult
{
    public float overallScore;         // 0~100
    public string matchedShapeID;      // 最佳匹配器型ID
    public string matchedShapeNameCN;
    public string matchedShapeNameEN;
    public float[] partScores;         // [5] 各部位得分，用于雷达图
    public float rawError;             // RawShapeError
}

public static class ShapeCalculator
{
    private static readonly float[] Weights = { 0.15f, 0.15f, 0.30f, 0.30f, 0.10f };
    private const float DMax = 0.35f;

    public static ShapeScoreResult Calculate(ShapeInput input, ShapeConfigSO[] templates, GameConfigSO config)
    {
        float dMax = config != null ? config.shapeDMax : DMax;
        float[] weights = config != null && config.shapeWeights != null && config.shapeWeights.Length == 5
            ? config.shapeWeights : Weights;

        float bestError = float.MaxValue;
        int bestIndex = -1;

        for (int i = 0; i < templates.Length; i++)
        {
            if (templates[i] == null) continue;

            float error = CalculateRawError(input, templates[i], weights);
            if (error < bestError)
            {
                bestError = error;
                bestIndex = i;
            }
        }

        string matchedID = bestIndex >= 0 ? templates[bestIndex].shapeID : "UNKNOWN";
        string matchedCN = bestIndex >= 0 ? templates[bestIndex].nameCN : "未知";
        string matchedEN = bestIndex >= 0 ? templates[bestIndex].nameEN : "Unknown";

        float score = 100f * Mathf.Max(0f, 1f - bestError / dMax);

        float[] partScores = new float[5];
        if (bestIndex >= 0)
        {
            var t = templates[bestIndex];
            partScores[0] = 100f * (1f - Mathf.Abs(input.mouth - t.mouthRatio));
            partScores[1] = 100f * (1f - Mathf.Abs(input.neck - t.neckRatio));
            partScores[2] = 100f * (1f - Mathf.Abs(input.shoulder - t.shoulderRatio));
            partScores[3] = 100f * (1f - Mathf.Abs(input.belly - t.bellyRatio));
            partScores[4] = 100f * (1f - Mathf.Abs(input.foot - t.footRatio));
        }

        return new ShapeScoreResult
        {
            overallScore = score,
            matchedShapeID = matchedID,
            matchedShapeNameCN = matchedCN,
            matchedShapeNameEN = matchedEN,
            partScores = partScores,
            rawError = bestError
        };
    }

    private static float CalculateRawError(ShapeInput input, ShapeConfigSO template, float[] weights)
    {
        return weights[0] * Mathf.Abs(input.mouth - template.mouthRatio)
             + weights[1] * Mathf.Abs(input.neck - template.neckRatio)
             + weights[2] * Mathf.Abs(input.shoulder - template.shoulderRatio)
             + weights[3] * Mathf.Abs(input.belly - template.bellyRatio)
             + weights[4] * Mathf.Abs(input.foot - template.footRatio);
    }
}
