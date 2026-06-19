using UnityEngine;

public class GlazeSystem : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private GlazeConfigSO[] glazeTemplates;
    [SerializeField] private GameConfigSO gameConfig;
    [SerializeField] private FiringSystem firingSystem;

    public string TargetGlazeName { get; private set; }
    public float TargetCopper { get; private set; }
    public float TargetIron { get; private set; }
    public float TargetCobalt { get; private set; }
    public float TargetTemperatureMax { get; private set; }
    public bool HasTarget { get; private set; }

    public GlazeScoreResult LastResult { get; private set; }

    private GlazeConfigSO currentTarget;

    private void Awake()
    {
        LoadTargetFromCurrentOrder();
    }

    public bool LoadTargetFromCurrentOrder()
    {
        HasTarget = false;
        TargetTemperatureMax = 0f;

        if (orderManager == null) return false;

        OrderData currentOrder = orderManager.GetCurrentOrder();
        if (currentOrder == null || string.IsNullOrEmpty(currentOrder.requiredGlazeID))
            return false;

        currentTarget = FindTemplateByID(currentOrder.requiredGlazeID);
        if (currentTarget == null) return false;

        TargetGlazeName = currentTarget.nameCN;
        TargetCopper = currentTarget.copper;
        TargetIron = currentTarget.iron;
        TargetCobalt = currentTarget.cobalt;
        TargetTemperatureMax = currentTarget.temperatureMax;
        HasTarget = true;
        return true;
    }

    public GlazeScoreResult Calculate(GlazeInput input)
    {
        // 注入烧制温度用于影青/冬青 Level 2 平局判定
        if (!input.firingTempMax.HasValue && firingSystem != null)
        {
            input.firingTempMax = firingSystem.CurrentTemperature;
        }
        LastResult = GlazeCalculator.Calculate(input, glazeTemplates, gameConfig);
        return LastResult;
    }

    private GlazeConfigSO FindTemplateByID(string glazeID)
    {
        if (glazeTemplates == null) return null;
        foreach (var t in glazeTemplates)
        {
            if (t != null && t.glazeID == glazeID) return t;
        }
        return null;
    }
}
