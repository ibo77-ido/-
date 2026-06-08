using UnityEngine;

public class ShapeSystem : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private ShapeConfigSO[] shapeTemplates;
    [SerializeField] private GameConfigSO gameConfig;

    public ShapeType TargetShapeType { get; private set; }
    public float TargetMouth { get; private set; }
    public float TargetNeck { get; private set; }
    public float TargetShoulder { get; private set; }
    public float TargetBelly { get; private set; }
    public float TargetFoot { get; private set; }
    public bool HasTarget { get; private set; }

    public ShapeScoreResult LastResult { get; private set; }

    private ShapeConfigSO currentTarget;

    private void Awake()
    {
        LoadTargetFromCurrentOrder();
    }

    public bool LoadTargetFromCurrentOrder()
    {
        HasTarget = false;

        if (orderManager == null) return false;

        OrderData currentOrder = orderManager.GetCurrentOrder();
        if (currentOrder == null || string.IsNullOrEmpty(currentOrder.requiredShapeID))
            return false;

        currentTarget = FindTemplateByID(currentOrder.requiredShapeID);
        if (currentTarget == null) return false;

        TargetShapeType = currentTarget.shapeType;
        TargetMouth = currentTarget.mouthRatio;
        TargetNeck = currentTarget.neckRatio;
        TargetShoulder = currentTarget.shoulderRatio;
        TargetBelly = currentTarget.bellyRatio;
        TargetFoot = currentTarget.footRatio;
        HasTarget = true;
        return true;
    }

    public ShapeScoreResult Calculate(ShapeInput input)
    {
        LastResult = ShapeCalculator.Calculate(input, shapeTemplates, gameConfig);
        return LastResult;
    }

    private ShapeConfigSO FindTemplateByID(string shapeID)
    {
        if (shapeTemplates == null) return null;
        foreach (var t in shapeTemplates)
        {
            if (t != null && t.shapeID == shapeID) return t;
        }
        return null;
    }
}
