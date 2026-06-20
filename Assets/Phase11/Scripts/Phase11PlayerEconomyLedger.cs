using System;
using UnityEngine;

public class Phase11PlayerEconomyLedger : MonoBehaviour
{
    private static Phase11PlayerEconomyLedger instance;

    [SerializeField] private int startingSilver;
    [SerializeField] private int startingReputation;
    [SerializeField] private bool dontDestroyOnLoad = true;

    private int totalSilver;
    private int totalReputation;

    public static Phase11PlayerEconomyLedger Instance => instance;
    public int TotalSilver => totalSilver;
    public int TotalReputation => totalReputation;

    public event Action<int, int> TotalsChanged;

    public static Phase11PlayerEconomyLedger GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindObjectOfType<Phase11PlayerEconomyLedger>();
        if (instance != null)
        {
            return instance;
        }

        GameObject go = new GameObject("Phase11PlayerEconomyLedger");
        instance = go.AddComponent<Phase11PlayerEconomyLedger>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        totalSilver = Mathf.Max(0, startingSilver);
        totalReputation = Mathf.Max(0, startingReputation);

        if (dontDestroyOnLoad && transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void AddRewards(int silverDelta, int reputationDelta)
    {
        int safeSilverDelta = Mathf.Max(0, silverDelta);
        int safeReputationDelta = Mathf.Max(0, reputationDelta);

        if (safeSilverDelta == 0 && safeReputationDelta == 0)
        {
            return;
        }

        totalSilver = SafeAdd(totalSilver, safeSilverDelta);
        totalReputation = SafeAdd(totalReputation, safeReputationDelta);
        TotalsChanged?.Invoke(totalSilver, totalReputation);
    }

    public void ResetTotals(int silver = 0, int reputation = 0)
    {
        totalSilver = Mathf.Max(0, silver);
        totalReputation = Mathf.Max(0, reputation);
        TotalsChanged?.Invoke(totalSilver, totalReputation);
    }

    private static int SafeAdd(int current, int delta)
    {
        if (int.MaxValue - current < delta)
        {
            return int.MaxValue;
        }

        return current + delta;
    }
}
