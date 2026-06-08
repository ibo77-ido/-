using UnityEngine;
using System.Collections.Generic;

public struct ResultInput
{
    public ShapeScoreResult shapeResult;
    public GlazeScoreResult glazeResult;
    public FireScoreResult fireResult;
    public OrderData orderData;
    public List<PenaltySource> defectList;
}

public struct ResultData
{
    // 评分数据
    public float shapeScore;
    public float glazeScore;
    public float fireScore;
    public float finalScore;            // 加权求和
    public string grade;                // S/A/B/C/D/E (仅在Result层)

    // 匹配数据
    public string matchedShapeID;
    public string matchedGlazeID;

    // 订单结果
    public string orderResult;          // Perfect/Excellent/Normal/Fail
    public string failReason;

    // 奖励
    public int goldReward;
    public int reputationReward;

    // 缺陷
    public List<PenaltySource> defectList;

    // 安全
    public bool errorFlag;
    public string errorCode;
    public int version;
}

public static class ResultCalculator
{
    private const float ShapeWeight = 0.35f;
    private const float GlazeWeight = 0.25f;
    private const float FireWeight = 0.40f;

    public static ResultData Calculate(ResultInput input, GameConfigSO config)
    {
        // Step 0: ValidateInput
        if (config == null)
        {
            return ErrorResult("ERR_CONFIG_NULL", "GameConfig is null");
        }

        ResultData data = new ResultData();
        data.version = 1;

        // Step 1: CalcFinalScore
        float shapeScore = input.shapeResult.overallScore;
        float glazeScore = input.glazeResult.overallScore;
        float fireScore = input.fireResult.fireScore;

        float finalScore = ShapeWeight * shapeScore
                         + GlazeWeight * glazeScore
                         + FireWeight * fireScore;

        data.shapeScore = shapeScore;
        data.glazeScore = glazeScore;
        data.fireScore = fireScore;
        data.finalScore = finalScore;

        // Step 2: CalcGrade
        data.grade = CalcGrade(finalScore, config);

        // Step 3: CheckOrder (Shape/Glaze matchedID vs requiredID)
        data.matchedShapeID = input.shapeResult.matchedShapeID;
        data.matchedGlazeID = input.glazeResult.matchedGlazeID;

        bool shapeMatchOK = (input.orderData != null) &&
            (input.shapeResult.matchedShapeID == input.orderData.requiredShapeID);
        bool glazeMatchOK = (input.orderData != null) &&
            (input.glazeResult.matchedGlazeID == input.orderData.requiredGlazeID);

        // Step 4: CalcOrderResult
        bool fireOK = input.fireResult.fireScore > 0f && !input.fireResult.isFatalDefect;

        if (!shapeMatchOK || !glazeMatchOK || !fireOK)
        {
            data.orderResult = "Fail";
            if (!shapeMatchOK) data.failReason = "器型不匹配";
            else if (!glazeMatchOK) data.failReason = "釉色不匹配";
            else data.failReason = "烧制缺陷";
        }
        else
        {
            if (finalScore >= 95f) data.orderResult = "Perfect";
            else if (finalScore >= 70f) data.orderResult = "Excellent";
            else if (finalScore >= 50f) data.orderResult = "Normal";
            else data.orderResult = "Fail";
        }

        // Step 5: CalcRewards
        if (input.orderData != null && data.orderResult != "Fail")
        {
            int gradeIndex = GradeToIndex(data.grade);
            int diffIndex = Mathf.Clamp(input.orderData.difficulty - 1, 0, 4);

            float goldMult = config.qualityGoldMultipliers[gradeIndex] * config.difficultyGoldMultipliers[diffIndex];
            float repMult = config.qualityRepMultipliers[gradeIndex] * config.difficultyRepMultipliers[diffIndex];

            // OrderResult multiplier only applies to reputation
            int orderResultIndex = OrderResultToIndex(data.orderResult);
            repMult *= config.orderRepMultipliers[orderResultIndex];

            data.goldReward = Mathf.FloorToInt(input.orderData.baseGold * goldMult);
            data.reputationReward = Mathf.FloorToInt(input.orderData.baseReputation * repMult);
        }
        else
        {
            data.goldReward = 0;
            data.reputationReward = 0;
        }

        // Step 6: MapGradeDisplay — UI层处理，Calculator不负责

        // Step 7: BuildResultData
        data.defectList = input.defectList;

        // Step 8: ValidateResultData (Runtime Safety)
        if (data.goldReward > config.goldMax)
        {
            data.goldReward = config.goldMax;
            data.errorFlag = true;
            data.errorCode = "ERR_GOLD_OVERFLOW";
        }
        if (data.reputationReward > config.repMax)
        {
            data.reputationReward = config.repMax;
            data.errorFlag = true;
            data.errorCode = "ERR_REP_OVERFLOW";
        }

        return data;
    }

    private static string CalcGrade(float score, GameConfigSO config)
    {
        if (score >= config.gradeS_Min) return "S";
        if (score >= config.gradeA_Min) return "A";
        if (score >= config.gradeB_Min) return "B";
        if (score >= config.gradeC_Min) return "C";
        if (score >= config.gradeD_Min) return "D";
        return "E";
    }

    private static int GradeToIndex(string grade)
    {
        switch (grade)
        {
            case "S": return 0;
            case "A": return 1;
            case "B": return 2;
            case "C": return 3;
            case "D": return 4;
            case "E": return 5;
            default: return 5;
        }
    }

    private static int OrderResultToIndex(string orderResult)
    {
        switch (orderResult)
        {
            case "Perfect": return 0;
            case "Excellent": return 1;
            case "Normal": return 2;
            default: return 3;
        }
    }

    private static ResultData ErrorResult(string code, string message)
    {
        return new ResultData
        {
            shapeScore = 0f,
            glazeScore = 0f,
            fireScore = 0f,
            finalScore = 0f,
            grade = "E",
            matchedShapeID = "",
            matchedGlazeID = "",
            orderResult = "Fail",
            failReason = message,
            goldReward = 0,
            reputationReward = 0,
            errorFlag = true,
            errorCode = code,
            version = 0
        };
    }
}
