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

    [Header("Product Reveal FX")]
    public GameObject productRevealFxGroup;
    public CanvasGroup productRevealFxCanvasGroup;
    public bool enableProductRevealFx = true;
    public bool autoCreateProductRevealFx = true;
    public int productRevealFxAutoCount = 10;
    public float productRevealFxFloatDistance = 36f;
    public float productRevealFxPulseScale = 0.35f;
    public Color productRevealFxPrimaryColor = new Color(1f, 0.92f, 0.55f, 0.85f);
    public Color productRevealFxSecondaryColor = new Color(1f, 1f, 1f, 0.7f);

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

        RectTransform[] fxRects;
        Vector2[] fxBasePositions;
        Vector3[] fxBaseScales;
        InitializeProductRevealFx(out fxRects, out fxBasePositions, out fxBaseScales);

        skipWaitRequested = false;
        float elapsed = 0f;

        while (elapsed < productRevealDuration && !skipWaitRequested)
        {
            if (!isRevealPaused)
            {
                elapsed += Time.unscaledDeltaTime;
                UpdateProductRevealFx(fxRects, fxBasePositions, fxBaseScales, elapsed, productRevealDuration);
            }

            yield return null;
        }

        if (productCeramicImage != null)
        {
            productCeramicImage.transform.localRotation = Quaternion.identity;
            productCeramicImage.transform.localScale = Vector3.one;
        }

        SetGroup(productRevealGroup, false);
        ResetProductRevealFx(fxRects, fxBasePositions, fxBaseScales);
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

    private void InitializeProductRevealFx(out RectTransform[] rects, out Vector2[] basePositions, out Vector3[] baseScales)
    {
        EnsureProductRevealFxFallback();

        rects = GetProductRevealFxRects();
        basePositions = new Vector2[rects.Length];
        baseScales = new Vector3[rects.Length];

        for (int i = 0; i < rects.Length; i++)
        {
            basePositions[i] = rects[i].anchoredPosition;
            baseScales[i] = rects[i].localScale;
        }

        SetGroup(productRevealFxGroup, enableProductRevealFx);
        if (productRevealFxCanvasGroup != null)
        {
            productRevealFxCanvasGroup.alpha = 0f;
            productRevealFxCanvasGroup.interactable = false;
            productRevealFxCanvasGroup.blocksRaycasts = false;
        }
    }

    private void EnsureProductRevealFxFallback()
    {
        if (!enableProductRevealFx || !autoCreateProductRevealFx || productRevealFxGroup != null || productRevealGroup == null)
            return;

        GameObject fxGroup = new GameObject("FX_ProductSparkles", typeof(RectTransform), typeof(CanvasGroup));
        fxGroup.transform.SetParent(productRevealGroup.transform, false);
        productRevealFxGroup = fxGroup;
        productRevealFxCanvasGroup = fxGroup.GetComponent<CanvasGroup>();
        productRevealFxCanvasGroup.alpha = 0f;
        productRevealFxCanvasGroup.interactable = false;
        productRevealFxCanvasGroup.blocksRaycasts = false;

        RectTransform groupRect = fxGroup.GetComponent<RectTransform>();
        groupRect.anchorMin = Vector2.zero;
        groupRect.anchorMax = Vector2.one;
        groupRect.anchoredPosition = Vector2.zero;
        groupRect.sizeDelta = Vector2.zero;
        groupRect.pivot = new Vector2(0.5f, 0.5f);

        int count = Mathf.Clamp(productRevealFxAutoCount, 0, 24);
        for (int i = 0; i < count; i++)
            CreateProductRevealFxDot(fxGroup.transform, i, count);
    }

    private void CreateProductRevealFxDot(Transform parent, int index, int count)
    {
        GameObject dot = new GameObject("Fx_Sparkle_" + (index + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dot.transform.SetParent(parent, false);

        RectTransform rect = dot.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        float t = count <= 1 ? 0f : index / (float)(count - 1);
        float angle = t * Mathf.PI * 2f;
        float radiusX = 170f + 42f * Mathf.Sin(index * 1.31f);
        float radiusY = 120f + 34f * Mathf.Cos(index * 0.91f);
        rect.anchoredPosition = new Vector2(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY + 18f);

        float size = 8f + (index % 4) * 3f;
        rect.sizeDelta = new Vector2(size, size);
        rect.localRotation = Quaternion.Euler(0f, 0f, index % 2 == 0 ? 45f : 0f);

        Image image = dot.GetComponent<Image>();
        image.raycastTarget = false;
        image.color = index % 3 == 0 ? productRevealFxSecondaryColor : productRevealFxPrimaryColor;
    }

    private RectTransform[] GetProductRevealFxRects()
    {
        if (productRevealFxGroup == null || !enableProductRevealFx)
            return new RectTransform[0];

        List<RectTransform> rects = new List<RectTransform>();
        for (int i = 0; i < productRevealFxGroup.transform.childCount; i++)
        {
            RectTransform rect = productRevealFxGroup.transform.GetChild(i) as RectTransform;
            if (rect != null)
                rects.Add(rect);
        }

        return rects.ToArray();
    }

    private void UpdateProductRevealFx(RectTransform[] rects, Vector2[] basePositions, Vector3[] baseScales, float elapsed, float duration)
    {
        if (!enableProductRevealFx || duration <= 0f)
            return;

        float normalized = Mathf.Clamp01(elapsed / duration);
        float alpha = Mathf.Sin(normalized * Mathf.PI);

        if (productRevealFxCanvasGroup != null)
            productRevealFxCanvasGroup.alpha = alpha;

        for (int i = 0; i < rects.Length; i++)
        {
            float offsetPhase = i * 0.37f;
            float vertical = productRevealFxFloatDistance * normalized * (0.6f + 0.08f * (i % 5));
            float horizontal = Mathf.Sin((elapsed * 1.4f) + offsetPhase) * 8f;
            float scalePulse = 1f + Mathf.Sin((elapsed * 3f) + offsetPhase) * productRevealFxPulseScale;

            rects[i].anchoredPosition = basePositions[i] + new Vector2(horizontal, vertical);
            rects[i].localScale = baseScales[i] * Mathf.Max(0.1f, scalePulse);
        }
    }

    private void ResetProductRevealFx(RectTransform[] rects, Vector2[] basePositions, Vector3[] baseScales)
    {
        for (int i = 0; i < rects.Length; i++)
        {
            rects[i].anchoredPosition = basePositions[i];
            rects[i].localScale = baseScales[i];
        }

        if (productRevealFxCanvasGroup != null)
            productRevealFxCanvasGroup.alpha = 0f;

        SetGroup(productRevealFxGroup, false);
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null) targetText.text = value;
    }
}
