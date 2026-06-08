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

    public void Refresh()
    {
        if (orderManager == null)
        {
            return;
        }

        OrderData currentOrder = orderManager.GetCurrentOrder();
        if (currentOrder == null)
        {
            SetText(orderNameText, "订单：未选择");
            SetText(targetShapeText, "目标器型：-");
            SetText(rewardSilverText, "银两奖励：0");
            SetText(rewardReputationText, "声望奖励：0");
            return;
        }

        SetText(orderNameText, "订单：" + currentOrder.orderName);
        SetText(targetShapeText, "目标器型：" + currentOrder.requiredShapeID);
        SetText(rewardSilverText, "银两奖励：" + currentOrder.baseGold);
        SetText(rewardReputationText, "声望奖励：" + currentOrder.baseReputation);
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
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
