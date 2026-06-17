using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerStatusBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI silverText;
    [SerializeField] private TextMeshProUGUI reputationText;
    [SerializeField] private TextMeshProUGUI orderText;

    [Header("Settings")]
    [SerializeField] private float refreshInterval = 0.5f;

    private void Start()
    {
        StartCoroutine(RefreshLoop());
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            RefreshDisplay();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    private void RefreshDisplay()
    {
        // Phase11A: placeholder values: Phase 13 for real data
        if (silverText != null)
            silverText.text = "Yinliang: --";
        if (reputationText != null)
            reputationText.text = "Shengwang: --";
        if (orderText != null)
            orderText.text = "Dingdan: --";
    }
}
