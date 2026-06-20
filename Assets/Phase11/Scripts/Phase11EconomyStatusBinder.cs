using TMPro;
using UnityEngine;

public class Phase11EconomyStatusBinder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI silverText;
    [SerializeField] private TextMeshProUGUI reputationText;
    [SerializeField] private Phase11PlayerEconomyLedger economyLedger;
    [SerializeField] private bool autoCreateEconomyLedger = true;

    private void OnEnable()
    {
        ResolveLedger();
        if (economyLedger != null)
        {
            economyLedger.TotalsChanged += HandleTotalsChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (economyLedger != null)
        {
            economyLedger.TotalsChanged -= HandleTotalsChanged;
        }
    }

    private void ResolveLedger()
    {
        if (economyLedger != null)
        {
            return;
        }

        economyLedger = autoCreateEconomyLedger
            ? Phase11PlayerEconomyLedger.GetOrCreate()
            : Phase11PlayerEconomyLedger.Instance;
    }

    public void Refresh()
    {
        ResolveLedger();

        int silver = economyLedger != null ? economyLedger.TotalSilver : 0;
        int reputation = economyLedger != null ? economyLedger.TotalReputation : 0;
        HandleTotalsChanged(silver, reputation);
    }

    private void HandleTotalsChanged(int silver, int reputation)
    {
        if (silverText != null)
        {
            silverText.text = silver.ToString();
        }

        if (reputationText != null)
        {
            reputationText.text = reputation.ToString();
        }
    }
}
