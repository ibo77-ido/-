using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResultPanelController : MonoBehaviour
{
    [SerializeField] private ResultSystem resultSystem;
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Text gradeText;
    [SerializeField] private Text shapeMatchText;
    [SerializeField] private Text glazeMatchText;
    [SerializeField] private Text fireScoreText;
    [SerializeField] private Text silverRewardText;
    [SerializeField] private Text reputationRewardText;
    [SerializeField] private Text orderResultText;
    [SerializeField] private Button nextOrderButton;
    [SerializeField] private Button exitGameplayButton;

    [Header("Exit Gameplay Event")]
    [SerializeField] private UnityEvent onExitGameplay = new UnityEvent();

    private static readonly Dictionary<string, string> GradeDisplayNames = new Dictionary<string, string>
    {
        { "S", "贡品" },
        { "A", "精品" },
        { "B", "佳品" },
        { "C", "良品" },
        { "D", "次品" },
        { "E", "废品" }
    };

    private static readonly Dictionary<string, Color> GradeDisplayColors = new Dictionary<string, Color>
    {
        { "S", new Color(1f, 0.84f, 0f) },       // 金色
        { "A", new Color(0.6f, 0.35f, 0.7f) },   // 紫色
        { "B", new Color(0.2f, 0.6f, 0.86f) },   // 蓝色
        { "C", new Color(0.18f, 0.8f, 0.44f) },  // 绿色
        { "D", new Color(0.95f, 0.77f, 0.06f) }, // 黄色
        { "E", new Color(0.58f, 0.65f, 0.65f) }  // 灰色
    };

    private void Start()
    {
        if (nextOrderButton != null)
            nextOrderButton.onClick.AddListener(OnNextOrderClicked);
        if (exitGameplayButton != null)
            exitGameplayButton.onClick.AddListener(OnExitGameplayClicked);
    }

    private void OnDestroy()
    {
        if (nextOrderButton != null)
            nextOrderButton.onClick.RemoveListener(OnNextOrderClicked);
        if (exitGameplayButton != null)
            exitGameplayButton.onClick.RemoveListener(OnExitGameplayClicked);
    }

    public void ShowResult()
    {
        if (resultSystem == null) return;

        ResultData data = resultSystem.CalculateResult();

        // Grade display
        if (gradeText != null)
        {
            string displayName = GradeDisplayNames.ContainsKey(data.grade)
                ? GradeDisplayNames[data.grade] : data.grade;
            gradeText.text = displayName;
            gradeText.color = GradeDisplayColors.ContainsKey(data.grade)
                ? GradeDisplayColors[data.grade] : Color.white;
        }

        // Scores
        SetText(shapeMatchText, $"器型匹配：{data.shapeScore:F1}%");
        SetText(glazeMatchText, $"釉料匹配：{data.glazeScore:F1}%");
        SetText(fireScoreText, $"火候评分：{data.fireScore:F1}%");

        // Order result
        if (data.orderResult == "Fail")
            SetText(orderResultText, $"订单失败({data.failReason})");
        else
            SetText(orderResultText, $"订单结果：{data.orderResult}");

        // Rewards
        SetText(silverRewardText, $"银两：{data.goldReward}");
        SetText(reputationRewardText, $"声望：{data.reputationReward}");
    }

    private void OnNextOrderClicked()
    {
        if (gameManager != null)
            gameManager.GoToNextOrder();
    }

    private void OnExitGameplayClicked()
    {
        onExitGameplay.Invoke();
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null) targetText.text = value;
    }
}
