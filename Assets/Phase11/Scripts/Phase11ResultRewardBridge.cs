using UnityEngine;

public class Phase11ResultRewardBridge : MonoBehaviour
{
    [SerializeField] private ResultSystem resultSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Phase11PlayerEconomyLedger economyLedger;

    private bool hasGrantedForCurrentResultState;

    private void Awake()
    {
        if (resultSystem == null)
        {
            resultSystem = FindObjectOfType<ResultSystem>();
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (economyLedger == null)
        {
            economyLedger = Phase11PlayerEconomyLedger.GetOrCreate();
        }
    }

    private void Update()
    {
        if (gameManager == null || resultSystem == null)
        {
            return;
        }

        if (gameManager.CurrentState != GameState.Result)
        {
            hasGrantedForCurrentResultState = false;
            return;
        }

        if (hasGrantedForCurrentResultState)
        {
            return;
        }

        ResultData result = resultSystem.LastResult;
        if (result.version <= 0 && !result.errorFlag)
        {
            return;
        }

        if (economyLedger == null)
        {
            economyLedger = Phase11PlayerEconomyLedger.GetOrCreate();
        }

        if (economyLedger != null)
        {
            economyLedger.AddRewards(result.goldReward, result.reputationReward);
            hasGrantedForCurrentResultState = true;
        }
    }
}
