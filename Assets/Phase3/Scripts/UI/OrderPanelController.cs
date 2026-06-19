using UnityEngine;
using UnityEngine.UI;

public class OrderPanelController : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Text orderNameText;
    [SerializeField] private Text targetShapeText;
    [SerializeField] private Text rewardSilverText;
    [SerializeField] private Text rewardReputationText;

    private void Start()
    {
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptClicked);
        }
        Refresh();
    }

    private void OnDestroy()
    {
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveListener(OnAcceptClicked);
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void ShowOrder(OrderData order)
    {
        if (order == null)
        {
            SetText(orderNameText, "未选择");
            SetText(targetShapeText, "-");
            SetText(rewardSilverText, "0");
            SetText(rewardReputationText, "0");
            return;
        }

        SetText(orderNameText, order.orderName);
        SetText(targetShapeText, ResolveShapeName(order.requiredShapeID));
        SetText(rewardSilverText, order.baseGold.ToString());
        SetText(rewardReputationText, order.baseReputation.ToString());
    }

    public void Refresh()
    {
        if (orderManager == null)
        {
            return;
        }

        ShowOrder(orderManager.GetCurrentOrder());
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }

    // 器型 ID → 中文名，依据 ShapeType.cs 与 ShapeConfigSO.nameCN
    private string ResolveShapeName(string shapeID)
    {
        switch (shapeID)
        {
            case "SHAPE_001": return "碗";
            case "SHAPE_002": return "盘";
            case "SHAPE_003": return "梅瓶";
            case "SHAPE_004": return "玉壶春";
            case "SHAPE_005": return "罐";
            default: return shapeID;
        }
    }

    private void OnAcceptClicked()
    {
        if (gameManager != null)
        {
            gameManager.GoToShape();
        }
    }
}
