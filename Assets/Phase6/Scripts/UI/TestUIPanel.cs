using UnityEngine;
using UnityEngine.UI;

public class TestUIPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text titleText;
    [SerializeField] private Button closeButton;

    private TestUIRouter router;

    public void Initialize(TestUIRouter uiRouter, string title)
    {
        router = uiRouter;
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void OnCloseClicked()
    {
        if (router != null)
        {
            router.CloseUI();
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}