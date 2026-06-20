using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Phase11ResultRewardBridge : MonoBehaviour
{
    [SerializeField] private ResultSystem resultSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Phase11PlayerEconomyLedger economyLedger;

    private bool hasGrantedForCurrentResultState;
    private Coroutine resolveRoutine;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        StartResolveRoutine();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (resolveRoutine != null)
        {
            StopCoroutine(resolveRoutine);
            resolveRoutine = null;
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

    private bool HasRequiredReferences()
    {
        return resultSystem != null && gameManager != null && economyLedger != null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveReferences();
        StartResolveRoutine();
    }

    private void StartResolveRoutine()
    {
        if (resolveRoutine == null && !HasRequiredReferences())
        {
            resolveRoutine = StartCoroutine(ResolveUntilReady());
        }
    }

    private IEnumerator ResolveUntilReady()
    {
        while (!HasRequiredReferences())
        {
            ResolveReferences();
            yield return null;
        }

        resolveRoutine = null;
    }

    private void ResolveReferences()
    {
        if (resultSystem == null)
        {
            resultSystem = FindFirstObjectByType<ResultSystem>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (economyLedger == null)
        {
            economyLedger = Phase11PlayerEconomyLedger.GetOrCreate();
        }
    }
}
