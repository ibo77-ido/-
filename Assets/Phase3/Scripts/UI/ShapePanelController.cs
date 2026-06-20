using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class ShapePanelController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private ShapeSystem shapeSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button toGlazeButton;
    [SerializeField] private Slider mouthSlider;
    [SerializeField] private Slider neckSlider;
    [SerializeField] private Slider shoulderSlider;
    [SerializeField] private Slider bellySlider;
    [SerializeField] private Slider footSlider;
    [SerializeField] private Text mouthValueText;
    [SerializeField] private Text neckValueText;
    [SerializeField] private Text shoulderValueText;
    [SerializeField] private Text bellyValueText;
    [SerializeField] private Text footValueText;
    [SerializeField] private Text shapeMatchText;
    [Header("Sculpt Shadow")]
    [SerializeField] private RectTransform sculptArea;
    [SerializeField] private Image sculptPanelGuideImage;
    [SerializeField] private Image targetShapeGuideImage;
    [SerializeField] private Image shadowBaseImage;
    [SerializeField] private Image shadowShoulderImage;
    [SerializeField] private Image shadowNeckImage;
    [SerializeField] private Sprite bowlTargetSprite;
    [SerializeField] private Sprite plateTargetSprite;
    [SerializeField] private Sprite meipingTargetSprite;
    [SerializeField] private Sprite yuhuChunTargetSprite;
    [SerializeField] private Sprite jarTargetSprite;
    [SerializeField] private float pressSensitivity = 0.004f;
    [SerializeField] private float shoulderZoneMin = 0.36f;
    [SerializeField] private float shoulderZoneMax = 0.68f;
    [SerializeField] private float neckZoneMin = 0.68f;
    [SerializeField] private float neckZoneMax = 1f;
    [SerializeField] private float bellyZoneMin = 0.22f;
    [SerializeField] private float bellyZoneMax = 0.36f;
    [SerializeField] private float footZoneMin = 0.04f;
    [SerializeField] private float footZoneMax = 0.22f;
    [SerializeField] private float inactiveLayerAlpha = 0.22f;
    [SerializeField] private float activeLayerAlpha = 0.55f;
    [Header("Sculpt Coupling")]
    [SerializeField] private float shoulderToBellyFactor = 0.25f;
    [SerializeField] private float shoulderToNeckFactor = 0.08f;
    [SerializeField] private float bellyToShoulderFactor = 0.12f;
    [SerializeField] private float footToBellyFactor = 0.35f;
    [Header("Match Video Bar")]
    [SerializeField] private RawImage matchVideoImage;
    [SerializeField] private VideoPlayer matchVideoPlayer;
    [SerializeField] private RenderTexture matchVideoTexture;
    [SerializeField] private Texture2D[] matchVideoFrames;

    private const float MinVisibleNeckScale = 0.62f;
    private const float MinVisibleShoulderScale = 0.24f;
    private const float NeckPressPreviewBoost = 0.24f;
    private const float ShoulderPressPreviewBoost = 0.18f;
    private const float PressFeedbackSensitivity = 0.018f;
    private const float MaxPressFeedback = 0.35f;
    private const float ActiveShadowAlpha = 0.72f;
    private const float InactiveShadowAlpha = 0.48f;
    private const float TargetGuideAlpha = 0.62f;
    private const float CurrentBodyAlpha = 0.52f;
    private const float NeckAlignWeight = 0.34f;
    private const float ShoulderAlignWeight = 0.33f;
    private const float BellyAlignWeight = 0.33f;
    private const float StartMouthRatio = 0.45f;
    private const float StartNeckRatio = 0.55f;
    private const float StartShoulderRatio = 0.25f;
    private const float StartBellyRatio = 0.25f;
    private const float StartFootRatio = 0.20f;
    private const float MeipingMouthRatio = 0.15f;
    private const float MeipingNeckRatio = 0.10f;
    private const float MeipingShoulderRatio = 0.85f;
    private const float MeipingBellyRatio = 0.55f;
    private const float MeipingFootRatio = 0.40f;

    private enum SculptZone
    {
        None,
        Shoulder,
        Neck,
        Belly,
        Foot
    }

    private SculptZone activeZone = SculptZone.None;
    private Vector2 lastPointerPosition;
    private bool isSculpting;
    private float pressFeedback;
    private float activeSideSign;
    private float pendingMatchPercent;
    private float lastAppliedMatchPercent = -1f;
    private int lastAppliedMatchFrame = -1;
    private bool matchVideoPrepared;
    private bool matchVideoPreparing;

    private void Awake()
    {
        AutoBindSculptReferences();
    }

    private void OnEnable()
    {
        AutoBindSculptReferences();
        AutoBindMatchVideoReferences();
        if (!HasMatchVideoFrameSequence())
        {
            RegisterMatchVideo();
        }
        RegisterListeners();
        Refresh();
    }

    private void Start()
    {
        RegisterListeners();
        if (toGlazeButton != null)
        {
            toGlazeButton.onClick.AddListener(OnToGlazeClicked);
        }
        Refresh();
    }

    private void OnDisable()
    {
        UnregisterListeners();
        UnregisterMatchVideo();
    }

    private void OnDestroy()
    {
        if (toGlazeButton != null)
        {
            toGlazeButton.onClick.RemoveListener(OnToGlazeClicked);
        }
    }

    public void Refresh()
    {
        if (shapeSystem == null)
        {
            return;
        }

        shapeSystem.LoadTargetFromCurrentOrder();
        RefreshTargetGuide();

        float neck = GetSliderValue(neckSlider);
        float shoulder = GetSliderValue(shoulderSlider);
        float belly = GetSliderValue(bellySlider);
        float mouth = neck;
        float foot = belly;

        SetText(mouthValueText, mouth.ToString("0.00"));
        SetText(neckValueText, neck.ToString("0.00"));
        SetText(shoulderValueText, shoulder.ToString("0.00"));
        SetText(bellyValueText, belly.ToString("0.00"));
        SetText(footValueText, foot.ToString("0.00"));

        ShapeInput currentShape = BuildMeipingAlignmentInput(neck, shoulder, belly);
        shapeSystem.Calculate(currentShape);
        float match = CalculateMeipingAlignmentMatch(neck, shoulder, belly);
        SetText(shapeMatchText, "Shape Match: " + match.ToString("F1") + "%");
        UpdateMatchVideo(match);
        RefreshShadow(mouth, neck, shoulder, belly, foot);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!TryGetSculptPoint(eventData, out Vector2 localPoint, out float normalizedY))
        {
            return;
        }

        activeZone = ResolveZone(normalizedY);
        if (activeZone == SculptZone.None)
        {
            return;
        }

        isSculpting = true;
        pressFeedback = 0f;
        activeSideSign = Mathf.Sign(localPoint.x);
        if (Mathf.Approximately(activeSideSign, 0f))
        {
            activeSideSign = 1f;
        }
        lastPointerPosition = eventData.position;
        SetActiveZoneVisual(activeZone);
        ApplySculptDelta(activeZone, Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSculpting || activeZone == SculptZone.None)
        {
            return;
        }

        Vector2 delta = eventData.position - lastPointerPosition;
        lastPointerPosition = eventData.position;
        ApplySculptDelta(activeZone, delta);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSculpting = false;
        activeZone = SculptZone.None;
        pressFeedback = 0f;
        activeSideSign = 0f;
        Refresh();
        SetActiveZoneVisual(SculptZone.None);
    }

    private void RegisterListeners()
    {
        AddListener(mouthSlider);
        AddListener(neckSlider);
        AddListener(shoulderSlider);
        AddListener(bellySlider);
        AddListener(footSlider);
    }

    private void UnregisterListeners()
    {
        RemoveListener(mouthSlider);
        RemoveListener(neckSlider);
        RemoveListener(shoulderSlider);
        RemoveListener(bellySlider);
        RemoveListener(footSlider);
    }

    private void AddListener(Slider slider)
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void RemoveListener(Slider slider)
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        Refresh();
    }

    private float GetSliderValue(Slider slider)
    {
        return slider != null ? slider.value : 0f;
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }

    public void ResetPanel()
    {
        SetSliderValue(mouthSlider, 0f);
        SetSliderValue(neckSlider, 0f);
        SetSliderValue(shoulderSlider, 0f);
        SetSliderValue(bellySlider, 0f);
        SetSliderValue(footSlider, 0f);
        SetText(mouthValueText, "0.00");
        SetText(neckValueText, "0.00");
        SetText(shoulderValueText, "0.00");
        SetText(bellyValueText, "0.00");
        SetText(footValueText, "0.00");
        SetText(shapeMatchText, "Shape Match: 0.0%");
        UpdateMatchVideo(0f);
        RefreshShadow(0f, 0f, 0f, 0f, 0f);
    }



    private void OnToGlazeClicked()
    {
        if (gameManager != null)
        {
            gameManager.GoToGlaze();
        }
    }

    private void AutoBindSculptReferences()
    {
        if (sculptArea == null)
        {
            Transform found = transform.Find("Sculpt_Area");
            sculptArea = found != null ? found as RectTransform : null;
        }

        if (shadowBaseImage == null)
        {
            shadowBaseImage = FindImage("Sculpt_Area/Img_ShadowBase");
        }

        if (sculptPanelGuideImage == null)
        {
            sculptPanelGuideImage = FindImage("Sculpt_Area/Img_MeipingGuide");
        }

        if (targetShapeGuideImage == null)
        {
            targetShapeGuideImage = FindImage("Sculpt_Area/Img_TargetShapeGuide");
        }

        if (shadowShoulderImage == null)
        {
            shadowShoulderImage = FindImage("Sculpt_Area/Img_ShadowShoulder");
        }

        if (shadowNeckImage == null)
        {
            shadowNeckImage = FindImage("Sculpt_Area/Img_ShadowNeck");
        }

        if (sculptPanelGuideImage != null)
        {
            sculptPanelGuideImage.raycastTarget = false;
            sculptPanelGuideImage.color = Color.white;
        }

        if (targetShapeGuideImage != null)
        {
            targetShapeGuideImage.raycastTarget = false;
            targetShapeGuideImage.color = new Color(0.22f, 0.62f, 1f, TargetGuideAlpha);
        }

        ConfigureCurrentShapeLayer(shadowBaseImage);
        ConfigureCurrentShapeLayer(shadowShoulderImage);
        ConfigureCurrentShapeLayer(shadowNeckImage);
    }

    private void AutoBindMatchVideoReferences()
    {
        if (matchVideoImage == null)
        {
            Transform found = transform.Find("MatchVideo_Bar/RawImage_MatchVideo");
            matchVideoImage = found != null ? found.GetComponent<RawImage>() : null;
        }

        if (matchVideoPlayer == null)
        {
            Transform found = transform.Find("MatchVideo_Bar/VideoPlayer_MatchVideo");
            matchVideoPlayer = found != null ? found.GetComponent<VideoPlayer>() : null;
        }

        if (matchVideoImage != null && matchVideoTexture != null)
        {
            matchVideoImage.texture = matchVideoTexture;
        }

        if (matchVideoPlayer != null && matchVideoTexture != null)
        {
            matchVideoPlayer.targetTexture = matchVideoTexture;
        }
    }

    private void RegisterMatchVideo()
    {
        if (matchVideoPlayer == null)
        {
            return;
        }

        matchVideoPlayer.prepareCompleted -= OnMatchVideoPrepared;
        matchVideoPlayer.prepareCompleted += OnMatchVideoPrepared;
        matchVideoPlayer.errorReceived -= OnMatchVideoError;
        matchVideoPlayer.errorReceived += OnMatchVideoError;
        matchVideoPlayer.playOnAwake = false;
        matchVideoPlayer.isLooping = false;
        matchVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        matchVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        matchVideoPlayer.waitForFirstFrame = true;

        if (!matchVideoPlayer.isPrepared)
        {
            matchVideoPrepared = false;
            PrepareMatchVideo();
        }
        else
        {
            matchVideoPrepared = true;
        }
    }

    private void UnregisterMatchVideo()
    {
        if (matchVideoPlayer != null)
        {
            matchVideoPlayer.prepareCompleted -= OnMatchVideoPrepared;
            matchVideoPlayer.errorReceived -= OnMatchVideoError;
        }
    }

    private void OnMatchVideoPrepared(VideoPlayer source)
    {
        matchVideoPrepared = true;
        matchVideoPreparing = false;
        SeekMatchVideo(pendingMatchPercent);
    }

    private void OnMatchVideoError(VideoPlayer source, string message)
    {
        matchVideoPrepared = false;
        matchVideoPreparing = false;
        Debug.LogWarning("Shape match video could not be prepared: " + message);
    }

    private void UpdateMatchVideo(float matchPercent)
    {
        pendingMatchPercent = Mathf.Clamp(matchPercent, 0f, 100f);

        if (UpdateMatchVideoFrameSequence(pendingMatchPercent))
        {
            return;
        }

        if (matchVideoPlayer == null)
        {
            AutoBindMatchVideoReferences();
            if (!HasMatchVideoFrameSequence())
            {
                RegisterMatchVideo();
            }
        }

        if (matchVideoPlayer == null)
        {
            return;
        }

        if (matchVideoPlayer.isPrepared)
        {
            matchVideoPrepared = true;
            matchVideoPreparing = false;
        }

        if (!matchVideoPrepared)
        {
            PrepareMatchVideo();
            return;
        }

        SeekMatchVideo(pendingMatchPercent);
    }

    private void PrepareMatchVideo()
    {
        if (matchVideoPlayer != null && !matchVideoPreparing)
        {
            matchVideoPreparing = true;
            matchVideoPlayer.Prepare();
        }
    }

    private bool UpdateMatchVideoFrameSequence(float matchPercent)
    {
        if (matchVideoImage == null || !HasMatchVideoFrameSequence())
        {
            return false;
        }

        int frameIndex = Mathf.RoundToInt(Mathf.Clamp01(matchPercent / 100f) * (matchVideoFrames.Length - 1));
        frameIndex = Mathf.Clamp(frameIndex, 0, matchVideoFrames.Length - 1);

        Texture2D frame = matchVideoFrames[frameIndex];
        if (frame == null)
        {
            return false;
        }

        if (lastAppliedMatchFrame != frameIndex || matchVideoImage.texture != frame)
        {
            matchVideoImage.texture = frame;
            lastAppliedMatchFrame = frameIndex;
        }

        if (matchVideoPlayer != null && matchVideoPlayer.isActiveAndEnabled)
        {
            matchVideoPlayer.Pause();
        }

        return true;
    }

    private bool HasMatchVideoFrameSequence()
    {
        return matchVideoFrames != null && matchVideoFrames.Length > 0;
    }

    private void SeekMatchVideo(float matchPercent)
    {
        if (matchVideoPlayer == null || matchVideoPlayer.clip == null)
        {
            return;
        }

        if (!matchVideoPrepared && !matchVideoPlayer.isPrepared)
        {
            return;
        }

        if (Mathf.Approximately(lastAppliedMatchPercent, matchPercent))
        {
            matchVideoPlayer.Pause();
            return;
        }

        ulong frameCount = matchVideoPlayer.frameCount > 0 ? matchVideoPlayer.frameCount : matchVideoPlayer.clip.frameCount;
        if (frameCount > 1)
        {
            long targetFrame = (long)System.Math.Round(Mathf.Clamp01(matchPercent / 100f) * (frameCount - 1));
            matchVideoPlayer.Pause();
            matchVideoPlayer.frame = targetFrame;
            lastAppliedMatchPercent = matchPercent;
            return;
        }

        double duration = matchVideoPlayer.length > 0.001 ? matchVideoPlayer.length : matchVideoPlayer.clip.length;
        if (duration <= 0.001)
        {
            return;
        }

        double targetTime = Mathf.Clamp01(matchPercent / 100f) * duration;
        matchVideoPlayer.Pause();
        matchVideoPlayer.time = targetTime;
        lastAppliedMatchPercent = matchPercent;
    }

    private Image FindImage(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<Image>() : null;
    }

    private bool TryGetSculptPoint(PointerEventData eventData, out Vector2 localPoint, out float normalizedY)
    {
        localPoint = Vector2.zero;
        normalizedY = 0f;

        if (sculptArea == null)
        {
            AutoBindSculptReferences();
            if (sculptArea == null)
            {
                return false;
            }
        }

        Camera eventCamera = eventData.pressEventCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(sculptArea, eventData.position, eventCamera, out localPoint))
        {
            return false;
        }

        Rect rect = sculptArea.rect;
        if (!rect.Contains(localPoint))
        {
            return false;
        }

        normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
        return true;
    }

    private SculptZone ResolveZone(float normalizedY)
    {
        if (normalizedY >= neckZoneMin && normalizedY <= neckZoneMax)
        {
            return SculptZone.Neck;
        }

        if (normalizedY >= shoulderZoneMin && normalizedY < shoulderZoneMax)
        {
            return SculptZone.Shoulder;
        }

        if (normalizedY >= footZoneMin && normalizedY < footZoneMax)
        {
            return SculptZone.Belly;
        }

        if (normalizedY >= bellyZoneMin && normalizedY < bellyZoneMax)
        {
            return SculptZone.Belly;
        }

        return SculptZone.None;
    }

    private void ApplySculptDelta(SculptZone zone, Vector2 pointerDelta)
    {
        float signedMovement = pointerDelta.x * activeSideSign;
        float zoneDirection = zone == SculptZone.Neck ? -1f : 1f;
        float signedDelta = signedMovement * zoneDirection * pressSensitivity;

        if (zone == SculptZone.Shoulder && shoulderSlider != null)
        {
            AddSliderValue(shoulderSlider, signedDelta);
            AddSliderValue(bellySlider, signedDelta * shoulderToBellyFactor);
            AddSliderValue(neckSlider, signedDelta * shoulderToNeckFactor);
        }
        else if (zone == SculptZone.Neck && neckSlider != null)
        {
            AddSliderValue(neckSlider, signedDelta);
            SetSliderValue(mouthSlider, GetSliderValue(neckSlider));
        }
        else if (zone == SculptZone.Belly && bellySlider != null)
        {
            AddSliderValue(bellySlider, signedDelta);
            SetSliderValue(footSlider, GetSliderValue(bellySlider));
            AddSliderValue(shoulderSlider, signedDelta * bellyToShoulderFactor);
        }
        else if (zone == SculptZone.Foot && footSlider != null)
        {
            AddSliderValue(footSlider, signedDelta);
            AddSliderValue(bellySlider, signedDelta * footToBellyFactor);
        }

        pressFeedback = Mathf.Clamp(
            Mathf.Max(pressFeedback, Mathf.Abs(signedMovement) * PressFeedbackSensitivity),
            0f,
            MaxPressFeedback
        );

        Refresh();
    }

    private static void AddSliderValue(Slider slider, float delta)
    {
        if (slider != null)
        {
            slider.value = Mathf.Clamp01(slider.value + delta);
        }
    }

    private static void SetSliderValue(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.value = Mathf.Clamp01(value);
        }
    }

    private ShapeInput BuildMeipingAlignmentInput(float neck, float shoulder, float belly)
    {
        float neckProgress = Mathf.Clamp01(neck);
        float shoulderProgress = Mathf.Clamp01(shoulder);
        float bellyProgress = Mathf.Clamp01(belly);

        return new ShapeInput
        {
            mouth = Mathf.Lerp(StartMouthRatio, MeipingMouthRatio, neckProgress),
            neck = Mathf.Lerp(StartNeckRatio, MeipingNeckRatio, neckProgress),
            shoulder = Mathf.Lerp(StartShoulderRatio, MeipingShoulderRatio, shoulderProgress),
            belly = Mathf.Lerp(StartBellyRatio, MeipingBellyRatio, bellyProgress),
            foot = Mathf.Lerp(StartFootRatio, MeipingFootRatio, bellyProgress)
        };
    }

    private float CalculateMeipingAlignmentMatch(float neck, float shoulder, float belly)
    {
        float weightedProgress =
            Mathf.Clamp01(neck) * NeckAlignWeight +
            Mathf.Clamp01(shoulder) * ShoulderAlignWeight +
            Mathf.Clamp01(belly) * BellyAlignWeight;

        return Mathf.Clamp(weightedProgress * 100f, 0f, 100f);
    }

    private void RefreshShadow(float mouth, float neck, float shoulder, float belly, float foot)
    {
        if (shadowBaseImage != null)
        {
            bool isBellyActive = activeZone == SculptZone.Belly;
            bool isFootActive = activeZone == SculptZone.Foot;
            float bodyPulse = isBellyActive ? Mathf.Min(0.08f, 0.03f + pressFeedback * 0.08f) : 0f;
            float footPulse = isFootActive ? Mathf.Min(0.12f, 0.04f + pressFeedback * 0.1f) : 0f;
            float bodyScale = Mathf.Lerp(0.78f, 1f, belly) + bodyPulse;
            float footScale = Mathf.Lerp(0.88f, 1f, foot) + footPulse;
            shadowBaseImage.rectTransform.localScale = new Vector3(bodyScale * footScale, 1f, 1f);
            SetImageAlpha(shadowBaseImage, isBellyActive || isFootActive ? ActiveShadowAlpha : CurrentBodyAlpha);
        }

        if (shadowShoulderImage != null)
        {
            float activeBoost = activeZone == SculptZone.Shoulder ? Mathf.Min(0.08f, ShoulderPressPreviewBoost * 0.2f + pressFeedback * 0.08f) : 0f;
            float shoulderScale = Mathf.Lerp(0.74f, 1f, shoulder) + activeBoost;
            shadowShoulderImage.rectTransform.localScale = new Vector3(shoulderScale, 1f, 1f);
            SetImageAlpha(shadowShoulderImage, GetShadowAlpha(activeZone == SculptZone.Shoulder));
        }

        if (shadowNeckImage != null)
        {
            bool isNeckActive = activeZone == SculptZone.Neck;
            float activeBoost = isNeckActive ? Mathf.Min(0.08f, NeckPressPreviewBoost * 0.2f + pressFeedback * 0.08f) : 0f;
            float neckScale = Mathf.Lerp(1.22f, 1f, neck) - activeBoost;
            shadowNeckImage.rectTransform.localScale = new Vector3(Mathf.Max(0.92f, neckScale), 1f, 1f);
            SetImageAlpha(shadowNeckImage, GetShadowAlpha(isNeckActive));
        }
    }

    private void RefreshTargetGuide()
    {
        if (targetShapeGuideImage == null)
        {
            AutoBindSculptReferences();
        }

        if (targetShapeGuideImage == null || shapeSystem == null || !shapeSystem.HasTarget)
        {
            return;
        }

        Sprite targetSprite = meipingTargetSprite;
        if (targetSprite == null)
        {
            targetShapeGuideImage.enabled = false;
            return;
        }

        targetShapeGuideImage.enabled = true;
        targetShapeGuideImage.sprite = targetSprite;
        targetShapeGuideImage.color = new Color(0.22f, 0.62f, 1f, TargetGuideAlpha);
        targetShapeGuideImage.preserveAspect = true;
    }



    private void SetActiveZoneVisual(SculptZone zone)
    {
        SetImageAlpha(shadowBaseImage, zone == SculptZone.Belly || zone == SculptZone.Foot ? ActiveShadowAlpha : CurrentBodyAlpha);
        SetImageAlpha(shadowShoulderImage, GetShadowAlpha(zone == SculptZone.Shoulder));
        SetImageAlpha(shadowNeckImage, GetShadowAlpha(zone == SculptZone.Neck));
    }

    private float GetShadowAlpha(bool isActive)
    {
        return isActive
            ? Mathf.Max(activeLayerAlpha, ActiveShadowAlpha)
            : Mathf.Max(inactiveLayerAlpha, InactiveShadowAlpha);
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private static void ConfigureCurrentShapeLayer(Image image)
    {
        if (image == null)
        {
            return;
        }

        image.raycastTarget = false;
        image.color = new Color(1f, 0.74f, 0.34f, CurrentBodyAlpha);
    }
}
