using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FiringPanelController : MonoBehaviour
{
	[SerializeField] private FiringSystem firingSystem;
	[SerializeField] private Text temperatureText;
	[SerializeField] private Slider windSlider;
	[SerializeField] private Button fuelButton;
	[SerializeField] private Button windowButton;
	[SerializeField] private Text windowButtonText;
	[SerializeField] private Button stopButton;
	[SerializeField] private Text zoneText;
	[SerializeField] private Text fireScoreText;
	[SerializeField] private Text statusText;
	[SerializeField] private Button openKilnButton;
	[SerializeField] private ShapeSystem shapeSystem;
	[SerializeField] private GlazeSystem glazeSystem;
	[SerializeField] private GameManager gameManager;

	[Header("Art - Videos")]
	[SerializeField] private VideoPlayer fireStartPlayer;
	[SerializeField] private VideoPlayer firingLoopPlayer;
	[SerializeField] private VideoPlayer closingKilnPlayer;
	[SerializeField] private VideoPlayer temperatureVideoPlayer;
	[SerializeField] private RawImage firingVideoImage;
	[SerializeField] private RawImage temperatureVideoImage;

	[Header("Layout")]
	public bool usePhase3ScaledWindow = true;
	public bool useBridgeScaledWindow = true;
	public bool applyCapturedLayoutInPlayMode = false;
	public bool controlTemperatureLayout = false;
	public bool controlWindDoorLayout = false;
	public bool controlButtonLayout = false;
	public Vector2 scaledWindowReferenceSize = new Vector2(1920f, 1080f);
	public Vector2 firingWindowSize = new Vector2(560f, 300f);
	public Vector2 phase3WindowPosition = new Vector2(-590f, -330f);
	public Vector2 phase3WindowScale = new Vector2(0.2532156f, 0.2532156f);
	public Vector2 temperatureBarSize = new Vector2(300f, 32f);
	public Vector2 temperatureBarPosition = new Vector2(0f, -440f);
	public Vector2 windDoorSize = new Vector2(52f, 52f);
	public Vector2 windDoorPosition = new Vector2(515f, -185f);
	public Vector2 buttonSize = new Vector2(46f, 46f);
	public Vector2 fuelButtonPosition = new Vector2(-500f, -390f);
	public Vector2 stopButtonPosition = new Vector2(500f, -390f);
	public Vector2 openButtonPosition = new Vector2(0f, -108f);

	[Header("Art - Static")]
	[SerializeField] private Image staticFallbackImage;

	[Header("Art - Wind Door")]
	[SerializeField] private Image windDoorImage;
	[SerializeField] private Sprite[] windDoorFrames;
	[SerializeField] private RectTransform windDoorHoverArea;

	[Header("Art - Buttons")]
	[SerializeField] private GameObject fuelButtonRoot;
	[SerializeField] private GameObject stopButtonRoot;
	[SerializeField] private GameObject openButtonRoot;

	private enum FiringUiState { Idle, Heating, Cooling, Result }
	private FiringUiState currentState = FiringUiState.Idle;

	private bool lastIsFiring = false;
	private bool lastHasActiveWind = false;
	private int lastWholeTemperature = -1;
	private FiringSystem.FireZone lastZone = (FiringSystem.FireZone)(-1);
	private int lastWindFrame = -1;

	[Header("Temperature Bar")]
	[SerializeField] private float fallbackTemperatureMax = 1300f;

	private float temperatureBarProgress = 0f;
	private bool isReversing = false;
	private int lastTemperatureBarFrame = -1;
	private bool hasTriggeredForceOpen = false;
	private bool hasStartedMainVideo = false;

	private void OnEnable()
	{
		HideLegacyControls();
		if (firingSystem != null && firingSystem.WindValue > 0f && IsFiringPanelStateActive())
		{
			ResetRuntimeCaches();
			lastIsFiring = true;
			lastHasActiveWind = true;
			if (!firingSystem.IsFiring)
			{
				firingSystem.BeginFiringByWind();
			}
			EnterState(FiringUiState.Heating, true);
		}
		else
		{
			lastHasActiveWind = false;
			EnterState(FiringUiState.Idle, true);
		}
		Refresh();
	}

	private void OnValidate()
	{
		// Layout is intentionally manual. Use the inspector buttons when a one-shot layout write is needed.
	}

	[ContextMenu("Apply Firing Layout Now")]
	public void ApplyFiringLayoutNow()
	{
		NormalizeArtLayout();
	}

	public void CenterBridgeWindowOnly()
	{
		RectTransform panelRect = transform as RectTransform;
		if (useBridgeScaledWindow)
		{
			SetScaledFixedWindow(panelRect, Vector2.zero, scaledWindowReferenceSize, phase3WindowScale);
			return;
		}

		Vector2 windowSize = firingWindowSize;
		if (windowSize.x <= 0f || windowSize.y <= 0f)
		{
			windowSize = new Vector2(560f, 300f);
		}
		SetCenteredWindow(panelRect, windowSize, Vector2.zero);
	}

	[ContextMenu("Reset Phase3 Firing Window Defaults")]
	public void ResetPhase3FiringWindowDefaults()
	{
		usePhase3ScaledWindow = true;
		useBridgeScaledWindow = true;
		applyCapturedLayoutInPlayMode = false;
		controlTemperatureLayout = false;
		controlWindDoorLayout = false;
		controlButtonLayout = false;
		scaledWindowReferenceSize = new Vector2(1920f, 1080f);
		firingWindowSize = new Vector2(560f, 300f);
		phase3WindowPosition = new Vector2(-590f, -330f);
		phase3WindowScale = new Vector2(0.2532156f, 0.2532156f);
		temperatureBarSize = new Vector2(300f, 32f);
		temperatureBarPosition = new Vector2(0f, -440f);
		windDoorSize = new Vector2(52f, 52f);
		windDoorPosition = new Vector2(515f, -185f);
		buttonSize = new Vector2(46f, 46f);
		fuelButtonPosition = new Vector2(-500f, -390f);
		stopButtonPosition = new Vector2(500f, -390f);
		openButtonPosition = new Vector2(0f, -108f);
		NormalizeArtLayout();
	}

	[ContextMenu("Capture Current Manual UI Layout")]
	public void CaptureCurrentManualUiLayout()
	{
		bool usesScaledWindow = UsesScaledLayout();
		applyCapturedLayoutInPlayMode = false;
		controlTemperatureLayout = false;
		controlWindDoorLayout = false;
		controlButtonLayout = false;

		RectTransform panelRect = transform as RectTransform;
		if (panelRect != null && !IsUnderGameplayCanvasRoot())
		{
			phase3WindowPosition = panelRect.anchoredPosition;
			phase3WindowScale = new Vector2(panelRect.localScale.x, panelRect.localScale.y);
			if (panelRect.anchorMin == panelRect.anchorMax)
			{
				firingWindowSize = panelRect.sizeDelta;
				usePhase3ScaledWindow = false;
			}
		}

		if (temperatureVideoImage != null)
		{
			RectTransform rect = temperatureVideoImage.rectTransform;
			temperatureBarSize = GetVisualSize(rect, usesScaledWindow);
			temperatureBarPosition = GetVisualPosition(rect, usesScaledWindow);
		}

		RectTransform fuelRect = GetRectTransform(fuelButtonRoot);
		if (fuelRect != null)
		{
			buttonSize = GetVisualSize(fuelRect, usesScaledWindow);
			fuelButtonPosition = GetVisualPosition(fuelRect, usesScaledWindow);
		}

		RectTransform stopRect = GetRectTransform(stopButtonRoot);
		if (stopRect != null)
		{
			if (fuelRect == null) buttonSize = GetVisualSize(stopRect, usesScaledWindow);
			stopButtonPosition = GetVisualPosition(stopRect, usesScaledWindow);
		}
	}

	private void Start()
	{
		if (windSlider != null)
		{
			windSlider.value = 0f;
			windSlider.onValueChanged.AddListener(OnWindSliderChanged);
			OnWindSliderChanged(0f);
		}
		if (fuelButton != null)
		{
			fuelButton.onClick.AddListener(OnFuelButtonClicked);
		}
		if (windowButton != null)
		{
			windowButton.onClick.AddListener(OnWindowButtonClicked);
		}
		if (stopButton != null)
		{
			stopButton.onClick.AddListener(OnStopButtonClicked);
		}
		if (openKilnButton != null)
		{
			openKilnButton.onClick.AddListener(OnOpenKilnButtonClicked);
		}

		HideLegacyControls();
	}

	private void NormalizeArtLayout()
	{
		RectTransform panelRect = transform as RectTransform;
		Vector2 windowSize = firingWindowSize;
		if (windowSize.x <= 0f || windowSize.y <= 0f)
		{
			windowSize = new Vector2(560f, 300f);
		}
		bool isBridgePanel = IsUnderGameplayCanvasRoot();
		bool usesScaledWindow = UsesScaledLayout(isBridgePanel);
		bool shouldDrivePanelLayout = isBridgePanel || !Application.isPlaying || applyCapturedLayoutInPlayMode;
		if (shouldDrivePanelLayout && isBridgePanel && useBridgeScaledWindow)
		{
			SetScaledFixedWindow(panelRect, Vector2.zero, scaledWindowReferenceSize, phase3WindowScale);
		}
		else if (shouldDrivePanelLayout && isBridgePanel)
		{
			SetCenteredWindow(panelRect, windowSize, Vector2.zero);
		}
		else if (shouldDrivePanelLayout && usesScaledWindow)
		{
			SetScaledStretchWindow(panelRect, phase3WindowPosition, phase3WindowScale);
		}
		else if (shouldDrivePanelLayout)
		{
			SetCenteredWindow(panelRect, windowSize, phase3WindowPosition);
		}

		Transform artRoot = transform.Find("ArtRoot_Firing");
		SetFullStretch(artRoot as RectTransform);

		if (staticFallbackImage != null)
		{
			SetFullStretch(staticFallbackImage.rectTransform);
			staticFallbackImage.preserveAspect = true;
		}
		if (firingVideoImage != null)
		{
			SetFullStretch(firingVideoImage.rectTransform);
			firingVideoImage.uvRect = new Rect(0f, 0f, 1f, 1f);
		}
		if (temperatureVideoImage != null)
		{
			if (ShouldApplyLayout(controlTemperatureLayout))
			{
				SetCenteredWindow(temperatureVideoImage.rectTransform, GetCompensatedSize(temperatureBarSize, usesScaledWindow), GetCompensatedPosition(temperatureBarPosition, usesScaledWindow));
			}
			temperatureVideoImage.transform.SetAsLastSibling();
			temperatureVideoImage.enabled = true;
		}
		if (ShouldApplyLayout(controlWindDoorLayout))
		{
			SetCenteredWindow(windDoorImage != null ? windDoorImage.rectTransform : null, GetCompensatedSize(windDoorSize, usesScaledWindow), GetCompensatedPosition(windDoorPosition, usesScaledWindow));
			SetCenteredWindow(windDoorHoverArea, GetCompensatedSize(windDoorSize, usesScaledWindow), GetCompensatedPosition(windDoorPosition, usesScaledWindow));
		}
		if (ShouldApplyLayout(controlButtonLayout))
		{
			SetCenteredWindow(GetRectTransform(fuelButtonRoot), GetCompensatedSize(buttonSize, usesScaledWindow), GetCompensatedPosition(fuelButtonPosition, usesScaledWindow));
			SetCenteredWindow(GetRectTransform(stopButtonRoot), GetCompensatedSize(buttonSize, usesScaledWindow), GetCompensatedPosition(stopButtonPosition, usesScaledWindow));
			SetCenteredWindow(GetRectTransform(openButtonRoot), GetCompensatedSize(buttonSize, usesScaledWindow), GetCompensatedPosition(openButtonPosition, usesScaledWindow));
		}
	}

	private bool IsUnderGameplayCanvasRoot()
	{
		Transform current = transform.parent;
		while (current != null)
		{
			if (current.name == "GameplayCanvasRoot")
			{
				return true;
			}
			current = current.parent;
		}
		return false;
	}

	private bool UsesScaledLayout()
	{
		return UsesScaledLayout(IsUnderGameplayCanvasRoot());
	}

	private bool UsesScaledLayout(bool isBridgePanel)
	{
		return (!isBridgePanel && usePhase3ScaledWindow) || (isBridgePanel && useBridgeScaledWindow);
	}

	private bool ShouldApplyLayout(bool explicitControl)
	{
		return explicitControl || (Application.isPlaying && applyCapturedLayoutInPlayMode);
	}

	private bool IsFiringPanelStateActive()
	{
		return gameManager == null || gameManager.CurrentState == GameState.Firing;
	}

	private static void SetCenteredWindow(RectTransform rect, Vector2 size, Vector2 anchoredPosition)
	{
		if (rect == null) return;

		rect.localScale = Vector3.one;
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;
	}

	private static void SetScaledStretchWindow(RectTransform rect, Vector2 anchoredPosition, Vector2 scale)
	{
		if (rect == null) return;

		if (scale.x <= 0f) scale.x = 0.2532156f;
		if (scale.y <= 0f) scale.y = scale.x;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = Vector2.zero;
		rect.localScale = new Vector3(scale.x, scale.y, 1f);
	}

	private static void SetScaledFixedWindow(RectTransform rect, Vector2 anchoredPosition, Vector2 size, Vector2 scale)
	{
		if (rect == null) return;

		if (size.x <= 0f) size.x = 1920f;
		if (size.y <= 0f) size.y = 1080f;
		if (scale.x <= 0f) scale.x = 0.2532156f;
		if (scale.y <= 0f) scale.y = scale.x;
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;
		rect.localScale = new Vector3(scale.x, scale.y, 1f);
	}

	private Vector2 GetCompensatedSize(Vector2 value, bool compensate)
	{
		if (!compensate) return value;
		return new Vector2(value.x / GetSafeLayoutScaleX(), value.y / GetSafeLayoutScaleY());
	}

	private Vector2 GetCompensatedPosition(Vector2 value, bool compensate)
	{
		if (!compensate) return value;
		return new Vector2(value.x / GetSafeLayoutScaleX(), value.y / GetSafeLayoutScaleY());
	}

	private Vector2 GetVisualSize(RectTransform rect, bool isCompensated)
	{
		if (rect == null) return Vector2.zero;
		if (!isCompensated) return rect.sizeDelta;
		return new Vector2(rect.sizeDelta.x * GetSafeLayoutScaleX(), rect.sizeDelta.y * GetSafeLayoutScaleY());
	}

	private Vector2 GetVisualPosition(RectTransform rect, bool isCompensated)
	{
		if (rect == null) return Vector2.zero;
		if (!isCompensated) return rect.anchoredPosition;
		return new Vector2(rect.anchoredPosition.x * GetSafeLayoutScaleX(), rect.anchoredPosition.y * GetSafeLayoutScaleY());
	}

	private float GetSafeLayoutScaleX()
	{
		return phase3WindowScale.x > 0f ? phase3WindowScale.x : 0.2532156f;
	}

	private float GetSafeLayoutScaleY()
	{
		if (phase3WindowScale.y > 0f) return phase3WindowScale.y;
		return GetSafeLayoutScaleX();
	}

	private static RectTransform GetRectTransform(GameObject target)
	{
		return target != null ? target.GetComponent<RectTransform>() : null;
	}

	private static void SetFullStretch(RectTransform rect)
	{
		if (rect == null) return;

		rect.localScale = Vector3.one;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
		rect.anchoredPosition = Vector2.zero;
		rect.sizeDelta = Vector2.zero;
	}

	private void OnDestroy()
	{
		UnregisterVideoCallbacks();
	}

	private void Update()
	{
		Refresh();
		UpdateWindDoorScroll();
		RefreshWindDoorFrame();
		UpdateTemperatureVideo();
	}

	/// <summary>
	/// 唯一状态入口。负责一次性显隐、视频播放/停止、按钮启用。
	/// </summary>
	private void EnterState(FiringUiState newState, bool force = false)
	{
		if (!force && currentState == newState) return;
		Debug.Log($"[FiringPanel] State: {currentState} -> {newState}");
		currentState = newState;

		switch (newState)
		{
		case FiringUiState.Idle:
			if (fuelButtonRoot != null) fuelButtonRoot.SetActive(true);
			if (stopButtonRoot != null) stopButtonRoot.SetActive(true);
			if (openButtonRoot != null) openButtonRoot.SetActive(false);
			SetWindDoorVisible(true);
			ShowClosedWindVisualState(true);
			break;

		case FiringUiState.Heating:
			if (fuelButtonRoot != null) fuelButtonRoot.SetActive(true);
			if (stopButtonRoot != null) stopButtonRoot.SetActive(true);
			if (openButtonRoot != null) openButtonRoot.SetActive(false);
			SetWindDoorVisible(true);
			InitTemperatureVideo();
			if (firingSystem != null && firingSystem.WindValue > 0f && !hasStartedMainVideo)
			{
				PlayFireStartSequence();
			}
			break;

			case FiringUiState.Cooling:
				if (fuelButtonRoot != null) fuelButtonRoot.SetActive(false);
				if (stopButtonRoot != null) stopButtonRoot.SetActive(false);
				if (openButtonRoot != null) openButtonRoot.SetActive(false);
				SetWindDoorVisible(false);
				SetTemperatureVideoVisible(false);
				StopMainVideo();
				PlayClosingKiln();
				break;

			case FiringUiState.Result:
				if (fuelButtonRoot != null) fuelButtonRoot.SetActive(false);
				if (stopButtonRoot != null) stopButtonRoot.SetActive(false);
				if (openButtonRoot != null) openButtonRoot.SetActive(false);
				SetFiringVideoVisible(false);
				SetStaticFallbackVisible(false);
				SetTemperatureVideoVisible(false);
				SetWindDoorVisible(false);
				break;
		}
	}

	/// <summary>
	/// 连续刷新。只做温度/风门/文本等帧级更新，不做状态切换。
	/// </summary>
	public void Refresh()
	{
		if (firingSystem == null) return;

		bool hasActiveWind = firingSystem.WindValue > 0f;
		if (hasActiveWind != lastHasActiveWind)
		{
			lastHasActiveWind = hasActiveWind;
			if (hasActiveWind && IsFiringPanelStateActive())
			{
				if (firingSystem != null && !firingSystem.IsFiring)
				{
					firingSystem.BeginFiringByWind();
				}
				lastIsFiring = true;
				EnterState(FiringUiState.Heating, true);
			}
			else if (!hasActiveWind && firingSystem.IsFiring)
			{
				lastIsFiring = true;
				ShowClosedWindVisualState(false);
			}
			else if (!hasActiveWind)
			{
				ShowClosedWindVisualState(false);
			}
		}

		UpdateTexts();
	}

	private void UpdateWindDoorScroll()
	{
		if (firingSystem == null) return;
		if (currentState != FiringUiState.Idle && currentState != FiringUiState.Heating) return;

		float scroll = Input.mouseScrollDelta.y;
		if (Mathf.Abs(scroll) < 0.01f) return;
		if (!IsPointerOverWindDoor()) return;

		// 步进 1/8 = 0.125
		float currentWind = firingSystem.WindValue;
		float step = 1f / 8f;
		if (scroll > 0)
		{
			currentWind += step;
		}
		else
		{
			currentWind -= step;
		}
		currentWind = Mathf.Clamp01(currentWind);
		firingSystem.SetWindValue(currentWind);
	}

	private bool IsPointerOverWindDoor()
	{
		Vector2 pointerPosition = Input.mousePosition;
		if (IsScreenPointInsideRect(windDoorHoverArea, pointerPosition))
		{
			return true;
		}

		return windDoorImage != null && IsScreenPointInsideRect(windDoorImage.rectTransform, pointerPosition);
	}

	private static bool IsScreenPointInsideRect(RectTransform rect, Vector2 screenPoint)
	{
		if (rect == null || !rect.gameObject.activeInHierarchy) return false;
		// ScreenSpaceOverlay 模式 camera 传 null
		return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, null);
	}

	private void RefreshWindDoorFrame()
	{
		if (windDoorImage == null || windDoorFrames == null || windDoorFrames.Length == 0) return;
		if (firingSystem == null) return;

		// windValue 0~1 → frameIndex 0~8
		int frameIndex = Mathf.RoundToInt(firingSystem.WindValue * 8f);
		frameIndex = Mathf.Clamp(frameIndex, 0, windDoorFrames.Length - 1);

		if (frameIndex != lastWindFrame)
		{
			windDoorImage.sprite = windDoorFrames[frameIndex];
			lastWindFrame = frameIndex;
		}
	}

	private void InitTemperatureVideo()
	{
		if (temperatureVideoPlayer == null) return;
		SetTemperatureVideoVisible(true);
		temperatureVideoPlayer.prepareCompleted -= OnTemperatureVideoPrepared;
		temperatureVideoPlayer.prepareCompleted += OnTemperatureVideoPrepared;
		temperatureVideoPlayer.Stop();
		temperatureVideoPlayer.Prepare();
		if (temperatureVideoPlayer.isPrepared)
		{
			OnTemperatureVideoPrepared(temperatureVideoPlayer);
		}
		temperatureBarProgress = 0f;
		isReversing = false;
		hasTriggeredForceOpen = false;
		lastTemperatureBarFrame = -1;
	}

	private void OnTemperatureVideoPrepared(VideoPlayer vp)
	{
		temperatureVideoPlayer.prepareCompleted -= OnTemperatureVideoPrepared;
		temperatureVideoPlayer.frame = 0;
		temperatureVideoPlayer.Play();
		temperatureVideoPlayer.Pause();
	}

	private void UpdateTemperatureVideo()
	{
		if (temperatureVideoPlayer == null || firingSystem == null) return;
		if (currentState != FiringUiState.Heating) return;
		if (hasTriggeredForceOpen) return;
		if (!temperatureVideoPlayer.isPrepared) return;

		int totalFrames = (int)temperatureVideoPlayer.frameCount;
		if (totalFrames <= 0) return;

		bool wasReversing = isReversing;
		isReversing = firingSystem.IsTemperatureDropping;
		float maxTemperature = GetTemperatureBarMax();
		temperatureBarProgress = Mathf.Clamp01(firingSystem.CurrentTemperature / maxTemperature);

		int targetFrame = (int)(temperatureBarProgress * totalFrames);
		targetFrame = Mathf.Clamp(targetFrame, 0, totalFrames - 1);

		if (targetFrame != lastTemperatureBarFrame)
		{
			temperatureVideoPlayer.frame = targetFrame;
			lastTemperatureBarFrame = targetFrame;
		}

		if ((isReversing || wasReversing) && temperatureBarProgress <= 0f && !hasTriggeredForceOpen)
		{
			TriggerForceUnderfiredOpen();
		}
	}

	private float GetTemperatureBarMax()
	{
		if (glazeSystem != null && glazeSystem.TargetTemperatureMax > 0f)
		{
			return glazeSystem.TargetTemperatureMax;
		}
		return Mathf.Max(1f, fallbackTemperatureMax);
	}

	private void TriggerForceUnderfiredOpen()
	{
		hasTriggeredForceOpen = true;
		Debug.Log("[FiringPanel] Temperature bar reversed to 0, force underfired open.");
		firingSystem.ForceUnderfiredOpen();

		StopMainVideo();
		if (closingKilnPlayer != null) closingKilnPlayer.Stop();

		if (gameManager != null)
		{
			EnterState(FiringUiState.Result, true);
			gameManager.GoToResult();
		}
	}

	private void UpdateTexts()
	{
		// 温度文本（整数度缓存，旧控件 inactive 时跳过）
		int wholeTemp = Mathf.RoundToInt(firingSystem.CurrentTemperature);
		if (wholeTemp != lastWholeTemperature)
		{
			lastWholeTemperature = wholeTemp;
			if (temperatureText != null && temperatureText.gameObject.activeInHierarchy)
			{
				string tempDisplay = firingSystem.IsWindowOpen
					? wholeTemp + "°C"
					: "???°C";
				temperatureText.text = "Temperature: " + tempDisplay;
			}
		}

		// 窗口按钮文本
		if (windowButtonText != null && windowButtonText.gameObject.activeInHierarchy)
		{
			SetText(windowButtonText, firingSystem.IsWindowOpen ? "Close" : "Open");
		}

		// 火候区间（缓存）
		var zone = firingSystem.GetCurrentZone();
		if (zone != lastZone)
		{
			lastZone = zone;
			string zoneLabel;
			Color zoneColor;
			switch (zone)
			{
				case FiringSystem.FireZone.Underfired:
					zoneLabel = "欠烧";
					zoneColor = Color.red;
					break;
				case FiringSystem.FireZone.Overfired:
					zoneLabel = "过烧";
					zoneColor = new Color(1f, 0.55f, 0f);
					break;
				default:
					zoneLabel = "正常";
					zoneColor = Color.green;
					break;
			}
			if (zoneText != null && zoneText.gameObject.activeInHierarchy)
			{
				zoneText.text = zoneLabel;
				zoneText.color = zoneColor;
			}
		}

		// 火候分数
		if (fireScoreText != null && fireScoreText.gameObject.activeInHierarchy)
		{
			string scoreDisplay = firingSystem.IsWindowOpen
				? firingSystem.GetFireScore().ToString("0")
				: "???";
			SetText(fireScoreText, "Fire Score: " + scoreDisplay);
		}

		// 状态文本
		if (statusText != null && statusText.gameObject.activeInHierarchy)
		{
			SetText(statusText, firingSystem.IsFiring ? "烧制中..." : "烧制完成");
		}

		// 旧版 openKilnButton.interactable 逻辑（兼容）
		if (openKilnButton != null && firingSystem.IsFiring)
		{
			openKilnButton.interactable = false;
		}
	}

	private void PlayFireStartSequence()
	{
		if (fireStartPlayer == null)
		{
			Debug.LogWarning("[FiringPanel] Fire start VideoPlayer is missing. Falling back to loop video.");
			PlayFiringLoop();
			return;
		}
		hasStartedMainVideo = true;

		SetStaticFallbackVisible(true);
		SetFiringVideoVisible(false);

		StopAndClearMainVideoPlayers();
		PrepareMainVideoFromStart(fireStartPlayer, OnFireStartPrepared, OnFireStartFinished);
	}

	private void OnFireStartPrepared(VideoPlayer vp)
	{
		fireStartPlayer.prepareCompleted -= OnFireStartPrepared;
		if (currentState != FiringUiState.Heating) return;

		SeekToFirstFrame(fireStartPlayer);
		SetFiringVideoVisible(true);
		SetStaticFallbackVisible(false);
		fireStartPlayer.Play();
	}

	private void OnFireStartFinished(VideoPlayer vp)
	{
		fireStartPlayer.loopPointReached -= OnFireStartFinished;
		fireStartPlayer.Stop();
		if (currentState == FiringUiState.Heating)
		{
			PlayFiringLoop();
		}
	}

	private void PlayFiringLoop()
	{
		if (firingLoopPlayer == null)
		{
			Debug.LogWarning("[FiringPanel] Firing loop VideoPlayer is missing.");
			return;
		}

		SetStaticFallbackVisible(true);
		SetFiringVideoVisible(false);

		StopAndClearMainVideoPlayers();
		firingLoopPlayer.isLooping = true;
		PrepareMainVideoFromStart(firingLoopPlayer, OnFiringLoopPrepared, null);
	}

	private void OnFiringLoopPrepared(VideoPlayer vp)
	{
		firingLoopPlayer.prepareCompleted -= OnFiringLoopPrepared;
		if (currentState != FiringUiState.Heating) return;

		SeekToFirstFrame(firingLoopPlayer);
		SetFiringVideoVisible(true);
		SetStaticFallbackVisible(false);
		firingLoopPlayer.Play();
	}

	private void StopMainVideo()
	{
		StopAndClearMainVideoPlayers();
		SetFiringVideoVisible(false);
		SetStaticFallbackVisible(true);
	}

	private void PlayClosingKiln()
	{
		if (closingKilnPlayer == null)
		{
			Debug.LogWarning("[FiringPanel] Closing kiln VideoPlayer is missing. Going to result immediately.");
			OnClosingKilnFinished(null);
			return;
		}

		SetStaticFallbackVisible(true);
		SetFiringVideoVisible(false);

		StopAndClearMainVideoPlayers();
		closingKilnPlayer.isLooping = false;
		PrepareMainVideoFromStart(closingKilnPlayer, OnClosingKilnPrepared, OnClosingKilnFinished);
	}

	private void OnClosingKilnPrepared(VideoPlayer vp)
	{
		closingKilnPlayer.prepareCompleted -= OnClosingKilnPrepared;
		if (currentState != FiringUiState.Cooling) return;

		SeekToFirstFrame(closingKilnPlayer);
		SetFiringVideoVisible(true);
		SetStaticFallbackVisible(false);
		closingKilnPlayer.Play();
	}

	private void OnClosingKilnFinished(VideoPlayer vp)
	{
		if (closingKilnPlayer != null)
		{
			closingKilnPlayer.loopPointReached -= OnClosingKilnFinished;
			closingKilnPlayer.Stop();
		}
		SetFiringVideoVisible(false);
		SetStaticFallbackVisible(true);
		if (gameManager != null)
		{
			EnterState(FiringUiState.Result, true);
			gameManager.GoToResult();
		}
	}

	private void UnregisterVideoCallbacks()
	{
		if (fireStartPlayer != null) fireStartPlayer.prepareCompleted -= OnFireStartPrepared;
		if (fireStartPlayer != null) fireStartPlayer.loopPointReached -= OnFireStartFinished;
		if (firingLoopPlayer != null) firingLoopPlayer.prepareCompleted -= OnFiringLoopPrepared;
		if (fireStartPlayer != null) fireStartPlayer.errorReceived -= OnMainVideoError;
		if (firingLoopPlayer != null) firingLoopPlayer.errorReceived -= OnMainVideoError;
		if (closingKilnPlayer != null) closingKilnPlayer.prepareCompleted -= OnClosingKilnPrepared;
		if (closingKilnPlayer != null) closingKilnPlayer.loopPointReached -= OnClosingKilnFinished;
		if (closingKilnPlayer != null) closingKilnPlayer.errorReceived -= OnMainVideoError;
		if (temperatureVideoPlayer != null) temperatureVideoPlayer.prepareCompleted -= OnTemperatureVideoPrepared;
	}

	private void StopAndClearMainVideoPlayers()
	{
		if (fireStartPlayer != null)
		{
			fireStartPlayer.prepareCompleted -= OnFireStartPrepared;
			fireStartPlayer.loopPointReached -= OnFireStartFinished;
			fireStartPlayer.errorReceived -= OnMainVideoError;
			fireStartPlayer.Stop();
		}
		if (firingLoopPlayer != null)
		{
			firingLoopPlayer.prepareCompleted -= OnFiringLoopPrepared;
			firingLoopPlayer.errorReceived -= OnMainVideoError;
			firingLoopPlayer.Stop();
		}
		if (closingKilnPlayer != null)
		{
			closingKilnPlayer.prepareCompleted -= OnClosingKilnPrepared;
			closingKilnPlayer.loopPointReached -= OnClosingKilnFinished;
			closingKilnPlayer.errorReceived -= OnMainVideoError;
			closingKilnPlayer.Stop();
		}
	}

	private void PrepareMainVideoFromStart(VideoPlayer player, VideoPlayer.EventHandler preparedHandler, VideoPlayer.EventHandler finishedHandler)
	{
		if (player == null || preparedHandler == null) return;

		EnsureVideoPlayerCanPrepare(player);
		if (!player.isActiveAndEnabled)
		{
			Debug.LogError($"[FiringPanel] Cannot prepare disabled VideoPlayer: {player.name}. Check inactive parents under Panel_Firing.");
			if (player == fireStartPlayer && currentState == FiringUiState.Heating)
			{
				PlayFiringLoop();
			}
			else if (player == closingKilnPlayer && currentState == FiringUiState.Cooling)
			{
				OnClosingKilnFinished(player);
			}
			return;
		}

		player.playOnAwake = false;
		player.waitForFirstFrame = true;
		player.prepareCompleted -= preparedHandler;
		player.prepareCompleted += preparedHandler;
		player.errorReceived -= OnMainVideoError;
		player.errorReceived += OnMainVideoError;
		if (finishedHandler != null)
		{
			player.loopPointReached -= finishedHandler;
			player.loopPointReached += finishedHandler;
		}

		player.Prepare();
		if (player.isPrepared)
		{
			preparedHandler(player);
		}
	}

	private static void SeekToFirstFrame(VideoPlayer player)
	{
		if (player == null) return;
		if (player.canSetTime) player.time = 0d;
		if (player.frameCount > 0) player.frame = 0;
	}

	private void EnsureVideoPlayerCanPrepare(VideoPlayer player)
	{
		if (player == null) return;

		Transform current = player.transform;
		while (current != null)
		{
			if (!current.gameObject.activeSelf)
			{
				current.gameObject.SetActive(true);
			}
			if (current == transform)
			{
				break;
			}
			current = current.parent;
		}

		player.enabled = true;
		if (firingVideoImage != null)
		{
			firingVideoImage.gameObject.SetActive(true);
		}
	}

	private void OnMainVideoError(VideoPlayer source, string message)
	{
		string sourceName = source != null ? source.name : "Unknown";
		Debug.LogError($"[FiringPanel] VideoPlayer error on {sourceName}: {message}");

		if (source == fireStartPlayer && currentState == FiringUiState.Heating)
		{
			PlayFiringLoop();
		}
		else if (source == closingKilnPlayer && currentState == FiringUiState.Cooling)
		{
			OnClosingKilnFinished(source);
		}
	}

	private void HideLegacyControls()
	{
		if (windSlider != null) windSlider.gameObject.SetActive(false);
		if (windowButton != null) windowButton.gameObject.SetActive(false);
		if (zoneText != null) zoneText.gameObject.SetActive(false);
		if (fireScoreText != null) fireScoreText.gameObject.SetActive(false);
		if (statusText != null) statusText.gameObject.SetActive(false);
		if (temperatureText != null) temperatureText.gameObject.SetActive(false);
	}

	private void SetFiringVideoVisible(bool visible)
	{
		if (firingVideoImage == null) return;
		if (visible && !firingVideoImage.gameObject.activeSelf)
		{
			firingVideoImage.gameObject.SetActive(true);
		}
		firingVideoImage.enabled = visible;
	}

	private void SetStaticFallbackVisible(bool visible)
	{
		if (staticFallbackImage != null) staticFallbackImage.enabled = visible;
	}

	private void SetTemperatureVideoVisible(bool visible)
	{
		if (temperatureVideoImage != null) temperatureVideoImage.enabled = visible;
	}

	private void SetWindDoorVisible(bool visible)
	{
		if (windDoorImage != null) windDoorImage.enabled = visible;
		if (windDoorHoverArea != null) windDoorHoverArea.gameObject.SetActive(visible);
	}

	private void ResetVisualsWithoutPlayback()
	{
		ShowClosedWindVisualState(true);
	}

	private void ShowClosedWindVisualState(bool resetTemperatureVideo)
	{
		StopMainVideo();
		if (closingKilnPlayer != null) closingKilnPlayer.Stop();
		hasStartedMainVideo = false;
		if (resetTemperatureVideo)
		{
			ResetTemperatureVideoToZero();
		}
		else
		{
			SetTemperatureVideoVisible(true);
		}
		SetStaticFallbackVisible(true);
		SetFiringVideoVisible(false);
		SetWindDoorVisible(true);
		if (fuelButtonRoot != null) fuelButtonRoot.SetActive(true);
		if (stopButtonRoot != null) stopButtonRoot.SetActive(true);
		if (openButtonRoot != null) openButtonRoot.SetActive(false);
	}

	private void ResetTemperatureVideoToZero()
	{
		temperatureBarProgress = 0f;
		isReversing = false;
		hasTriggeredForceOpen = false;
		lastTemperatureBarFrame = -1;
		SetTemperatureVideoVisible(true);
		if (temperatureVideoPlayer == null) return;

		temperatureVideoPlayer.prepareCompleted -= OnTemperatureVideoPrepared;
		temperatureVideoPlayer.Stop();
		if (temperatureVideoPlayer.isPrepared)
		{
			temperatureVideoPlayer.frame = 0;
			temperatureVideoPlayer.Play();
			temperatureVideoPlayer.Pause();
			lastTemperatureBarFrame = 0;
		}
	}

	private void ResetRuntimeCaches()
	{
		lastWholeTemperature = -1;
		lastZone = (FiringSystem.FireZone)(-1);
		lastWindFrame = -1;
		lastTemperatureBarFrame = -1;
		lastHasActiveWind = false;
		hasStartedMainVideo = false;
	}

	private void OnWindSliderChanged(float value)
	{
		if (firingSystem != null)
		{
			firingSystem.SetWindValue(value);
		}
	}

	private void OnFuelButtonClicked()
	{
		if (firingSystem != null)
		{
			firingSystem.AddFuel();
		}
	}

	private void OnWindowButtonClicked()
	{
		if (firingSystem != null)
		{
			firingSystem.ToggleWindow();
		}
	}

	private void OnStopButtonClicked()
	{
		if (firingSystem == null || !firingSystem.IsFiring) return;
		firingSystem.StopFiring();
		EnterState(FiringUiState.Cooling);
	}

	private void OnOpenKilnButtonClicked()
	{
		if (firingSystem == null || firingSystem.IsFiring) return;

		var shapeResult = shapeSystem?.LastResult;
		var glazeResult = glazeSystem?.LastResult;
		Debug.Log($"[OpenKiln] Shape Match: {shapeResult?.overallScore ?? 0f:F1}% | Glaze Match: {glazeResult?.overallScore ?? 0f:F1}% | Fire Score: {firingSystem.GetFireScore():F1}");
		openKilnButton.interactable = false;

		if (gameManager != null)
		{
			EnterState(FiringUiState.Result, true);
			gameManager.GoToResult();
		}
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
		if (windSlider != null) windSlider.SetValueWithoutNotify(0f);
		if (stopButton != null) stopButton.interactable = true;
		if (openKilnButton != null) openKilnButton.interactable = false;

		StopMainVideo();
		if (closingKilnPlayer != null) closingKilnPlayer.Stop();
		lastIsFiring = firingSystem != null && firingSystem.IsFiring;
		lastHasActiveWind = firingSystem != null && firingSystem.WindValue > 0f;
		ResetRuntimeCaches();
		EnterState(FiringUiState.Idle, true);
	}
}
