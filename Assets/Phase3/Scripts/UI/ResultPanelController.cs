using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResultPanelController : MonoBehaviour
{
    [System.Serializable]
    public class RevealStep
    {
        public Sprite sprite;
        public GameObject group;
        public bool showNextOrderButton;
    }

    [System.Serializable]
    public class ProductSpriteBinding
    {
        public string orderID;
        public Sprite sprite;
    }

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

    [Header("Reveal Sequence")]
    public Image stageImage;
    public Sprite stage1Sprite;
    public Sprite stage2Sprite;
    public Sprite stage3Sprite;
    public GameObject matchGroup;
    public GameObject gradeGroup;
    public GameObject rewardGroup;
    public float revealInterval = 3f;
    public RevealStep[] revealSteps;

    [Header("Product Reveal")]
    public GameObject productRevealGroup;
    public Image productCeramicImage;
    public float productRevealDuration = 3f;
    public ProductSpriteBinding[] productSprites;
    public Button skipRevealWaitButton;

    [Header("Internal Debug")]
    public Button debugPauseButton;
    public Text debugPauseText;
    public string debugPauseLabel = "暂停播放";
    public string debugResumeLabel = "继续播放";

    [Header("Exit Gameplay Event")]
    [SerializeField] private UnityEvent onExitGameplay = new UnityEvent();

    private Coroutine revealRoutine;
    private bool revealPending;
    private bool isRevealPaused;
    private bool skipWaitRequested;
    private ResultData pendingResultData;

    /// <summary>
    /// Read-only access to the exit gameplay event.
    /// GameplayBridgeManager subscribes to this at runtime.
    /// Phase3 never knows about Bridge - only exposes the event.
    /// </summary>
    public UnityEvent OnExitGameplayEvent => onExitGameplay;

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
        { "S", new Color(1f, 0.84f, 0f) },
        { "A", new Color(0.6f, 0.35f, 0.7f) },
        { "B", new Color(0.2f, 0.6f, 0.86f) },
        { "C", new Color(0.18f, 0.8f, 0.44f) },
        { "D", new Color(0.95f, 0.77f, 0.06f) },
        { "E", new Color(0.58f, 0.65f, 0.65f) }
    };

    private void Start()
    {
        if (nextOrderButton != null)
            nextOrderButton.onClick.AddListener(OnNextOrderClicked);
        if (exitGameplayButton != null)
            exitGameplayButton.onClick.AddListener(OnExitGameplayClicked);
        if (debugPauseButton != null)
            debugPauseButton.onClick.AddListener(ToggleRevealPause);
        if (skipRevealWaitButton != null)
            skipRevealWaitButton.onClick.AddListener(OnSkipRevealWaitClicked);

        UpdateDebugPauseLabel();
    }

    private void OnDestroy()
    {
        StopRevealRoutine();

        if (nextOrderButton != null)
            nextOrderButton.onClick.RemoveListener(OnNextOrderClicked);
        if (exitGameplayButton != null)
            exitGameplayButton.onClick.RemoveListener(OnExitGameplayClicked);
        if (debugPauseButton != null)
            debugPauseButton.onClick.RemoveListener(ToggleRevealPause);
        if (skipRevealWaitButton != null)
            skipRevealWaitButton.onClick.RemoveListener(OnSkipRevealWaitClicked);
    }

    private void OnDisable()
    {
        StopRevealRoutine();
    }

    private void OnEnable()
    {
        if (revealPending)
            StartRevealRoutine();
    }

    public void ShowResult()
    {
        if (resultSystem == null) return;

        ResultData data = resultSystem.CalculateResult();
        pendingResultData = data;
        ApplyResultText(data);

        isRevealPaused = false;
        skipWaitRequested = false;
        UpdateDebugPauseLabel();
        StopRevealRoutine();

        if (isActiveAndEnabled)
            StartRevealRoutine(data);
        else
            revealPending = true;
    }

private void ApplyResultText(ResultData data)
    {
        if (gradeText != null)
        {
            string displayName = GradeDisplayNames.ContainsKey(data.grade)
                ? GradeDisplayNames[data.grade] : data.grade;
            gradeText.text = displayName;
            gradeText.color = GradeDisplayColors.ContainsKey(data.grade)
                ? GradeDisplayColors[data.grade] : Color.white;
        }

        SetText(shapeMatchText, $"{data.shapeScore:F1}%");
        SetText(glazeMatchText, $"{data.glazeScore:F1}%");
        SetText(fireScoreText, $"{data.fireScore:F1}%");

        if (data.orderResult == "Fail")
            SetText(orderResultText, data.failReason);
        else
            SetText(orderResultText, data.orderResult);

        SetText(silverRewardText, data.goldReward.ToString());
        SetText(reputationRewardText, data.reputationReward.ToString());
    }

    private IEnumerator ProductThenRevealSequence(ResultData data)
    {
        yield return ProductRevealSequence(data);
        yield return RevealResultSequence();
    }

    private IEnumerator ProductRevealSequence(ResultData data)
    {
        HideAllRevealGroups();
        SetGroup(productRevealGroup, true);
        SetStageImageActive(false);

        if (nextOrderButton != null)
            nextOrderButton.gameObject.SetActive(false);

        if (productCeramicImage != null)
        {
            productCeramicImage.sprite = ResolveProductSprite();
            productCeramicImage.color = Color.white;
            productCeramicImage.preserveAspect = true;
            productCeramicImage.transform.localRotation = Quaternion.identity;
            productCeramicImage.transform.localScale = Vector3.one;
        }

        skipWaitRequested = false;
        float elapsed = 0f;

        while (elapsed < productRevealDuration && !skipWaitRequested)
        {
            if (!isRevealPaused)
                elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        if (productCeramicImage != null)
        {
            productCeramicImage.transform.localRotation = Quaternion.identity;
            productCeramicImage.transform.localScale = Vector3.one;
        }

        SetGroup(productRevealGroup, false);
        SetStageImageActive(true);
        skipWaitRequested = false;
    }

    private IEnumerator RevealResultSequence()
    {
        PrepareRevealStage();

        var steps = GetRevealSteps();
        for (int i = 1; i < steps.Length; i++)
        {
            yield return WaitRevealInterval(revealInterval);
            ApplyRevealStep(steps[i]);
        }

        revealRoutine = null;
    }

    private void PrepareRevealStage()
    {
        var steps = GetRevealSteps();
        if (steps.Length == 0)
            return;

        ApplyRevealStep(steps[0]);
    }

    private RevealStep[] GetRevealSteps()
    {
        if (revealSteps != null && revealSteps.Length > 0)
            return revealSteps;

        return new[]
        {
            new RevealStep { sprite = stage1Sprite, group = matchGroup, showNextOrderButton = false },
            new RevealStep { sprite = stage2Sprite, group = gradeGroup, showNextOrderButton = false },
            new RevealStep { sprite = stage3Sprite, group = rewardGroup, showNextOrderButton = true }
        };
    }

    private void ApplyRevealStep(RevealStep step)
    {
        HideAllRevealGroups();

        if (step != null)
        {
            SetStage(step.sprite);
            SetGroup(step.group, true);
        }

        if (nextOrderButton != null)
            nextOrderButton.gameObject.SetActive(step != null && step.showNextOrderButton);
    }

    private void HideAllRevealGroups()
    {
        SetGroup(matchGroup, false);
        SetGroup(gradeGroup, false);
        SetGroup(rewardGroup, false);
        SetGroup(productRevealGroup, false);

        if (revealSteps == null) return;

        for (int i = 0; i < revealSteps.Length; i++)
        {
            if (revealSteps[i] != null)
                SetGroup(revealSteps[i].group, false);
        }
    }

    private void StartRevealRoutine()
    {
        StartRevealRoutine(pendingResultData);
    }

    private void StartRevealRoutine(ResultData data)
    {
        StopRevealRoutine();
        revealPending = false;
        revealRoutine = StartCoroutine(ProductThenRevealSequence(data));
    }

    private IEnumerator WaitRevealInterval(float duration)
    {
        skipWaitRequested = false;
        float elapsed = 0f;
        while (elapsed < duration && !skipWaitRequested)
        {
            if (!isRevealPaused)
                elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        skipWaitRequested = false;
    }

    private void SetStage(Sprite sprite)
    {
        if (stageImage != null && sprite != null)
            stageImage.sprite = sprite;
    }

    private void SetStageImageActive(bool active)
    {
        if (stageImage != null)
            stageImage.gameObject.SetActive(active);
    }

    private void SetGroup(GameObject group, bool active)
    {
        if (group != null)
            group.SetActive(active);
    }

    private void StopRevealRoutine()
    {
        if (revealRoutine == null) return;

        StopCoroutine(revealRoutine);
        revealRoutine = null;
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

    private void OnSkipRevealWaitClicked()
    {
        if (revealRoutine != null)
            skipWaitRequested = true;
    }

    public void ToggleRevealPause()
    {
        isRevealPaused = !isRevealPaused;
        UpdateDebugPauseLabel();
    }

    public void SetRevealPaused(bool paused)
    {
        isRevealPaused = paused;
        UpdateDebugPauseLabel();
    }

    private void UpdateDebugPauseLabel()
    {
        if (debugPauseText != null)
            debugPauseText.text = isRevealPaused ? debugResumeLabel : debugPauseLabel;
    }

    private Sprite ResolveProductSprite()
    {
        OrderData order = orderManager != null ? orderManager.GetCurrentOrder() : null;
        if (order == null || productSprites == null) return null;

        for (int i = 0; i < productSprites.Length; i++)
        {
            if (productSprites[i] != null && productSprites[i].orderID == order.orderID)
                return productSprites[i].sprite;
        }

        return null;
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null) targetText.text = value;
    }
}
