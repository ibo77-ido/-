using UnityEngine;
using System.Collections.Generic;

public class ResultSystem : MonoBehaviour
{
    [SerializeField] private ShapeSystem shapeSystem;
    [SerializeField] private GlazeSystem glazeSystem;
    [SerializeField] private FiringSystem firingSystem;
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private GameConfigSO gameConfig;

    public ResultData LastResult { get; private set; }

    public ResultData CalculateResult()
    {
        if (shapeSystem == null || glazeSystem == null || firingSystem == null)
        {
            LastResult = new ResultData { errorFlag = true, errorCode = "ERR_SYSTEM_NULL" };
            return LastResult;
        }

        GameConfigSO config = gameConfig;

        // Build input from the three subsystem results
        ResultInput input = new ResultInput
        {
            shapeResult = shapeSystem.LastResult,
            glazeResult = glazeSystem.LastResult,
            fireResult = firingSystem.CalculateScore(),
            orderData = orderManager != null ? orderManager.GetCurrentOrder() : null,
            defectList = null
        };

        LastResult = ResultCalculator.Calculate(input, config);
        return LastResult;
    }
}
