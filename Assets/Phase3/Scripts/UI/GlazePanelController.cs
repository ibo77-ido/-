using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GlazePanelController : MonoBehaviour
{
    [SerializeField] private GlazeSystem glazeSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button toFiringButton;
    [SerializeField] private Slider copperSlider;
    [SerializeField] private Slider ironSlider;
    [SerializeField] private Slider cobaltSlider;
    [SerializeField] private Text copperValueText;
    [SerializeField] private Text ironValueText;
    [SerializeField] private Text cobaltValueText;
    [SerializeField] private Text glazeMatchText;
    [Header("Triangle Mixer")]
    [SerializeField] private GlazeTriangleMixer triangleMixer;
    [Header("Match Video Bar")]
    [SerializeField] private RawImage matchVideoImage;
    [SerializeField] private Image matchFrameImage;
    [SerializeField] private VideoPlayer matchVideoPlayer;
    [SerializeField] private RenderTexture matchVideoTexture;
    [SerializeField] private Texture2D[] matchVideoFrames;
    [SerializeField] private Sprite[] matchFrameSprites;

    private float pendingMatchPercent;
    private float lastAppliedMatchPercent = -1f;
    private int lastAppliedMatchFrame = -1;
    private bool matchVideoPrepared;
    private bool matchVideoPreparing;

    private void OnEnable()
    {
        AutoBindTriangleMixer();
        AutoBindMatchVideoReferences();
        RegisterListeners();
        RegisterTriangleMixer();
        if (!HasMatchFrameSprites() && !HasMatchVideoFrameSequence())
        {
            RegisterMatchVideo();
        }
        Refresh();
    }

    private void Start()
    {
        AutoBindTriangleMixer();
        AutoBindMatchVideoReferences();
        RegisterListeners();
        RegisterTriangleMixer();
        if (toFiringButton != null)
        {
            toFiringButton.onClick.AddListener(OnToFiringClicked);
        }
        Refresh();
    }

    private void OnDisable()
    {
        UnregisterListeners();
        UnregisterTriangleMixer();
        UnregisterMatchVideo();
    }

    private void OnDestroy()
    {
        if (toFiringButton != null)
        {
            toFiringButton.onClick.RemoveListener(OnToFiringClicked);
        }
    }

    public void Refresh()
    {
        if (glazeSystem == null)
        {
            return;
        }

        glazeSystem.LoadTargetFromCurrentOrder();

        GlazeInput input = GetCurrentGlazeInput();
        float copper = input.copper;
        float iron = input.iron;
        float cobalt = input.cobalt;

        SetText(copperValueText, FormatPercent(triangleMixer != null ? triangleMixer.CopperWeight : copper / 0.02f));
        SetText(ironValueText, FormatPercent(triangleMixer != null ? triangleMixer.IronWeight : iron / 0.02f));
        SetText(cobaltValueText, FormatPercent(triangleMixer != null ? triangleMixer.CobaltWeight : cobalt / 0.02f));

        var result = glazeSystem.Calculate(input);
        SetText(glazeMatchText, "釉料匹配：" + result.overallScore.ToString("F1") + "%");
        UpdateMatchVideo(result.overallScore);
    }

    private void RegisterListeners()
    {
        if (triangleMixer == null)
        {
            AddListener(copperSlider);
            AddListener(ironSlider);
            AddListener(cobaltSlider);
        }
    }

    private void UnregisterListeners()
    {
        RemoveListener(copperSlider);
        RemoveListener(ironSlider);
        RemoveListener(cobaltSlider);
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

    private GlazeInput GetCurrentGlazeInput()
    {
        if (triangleMixer != null)
        {
            return triangleMixer.CurrentInput;
        }

        return new GlazeInput
        {
            copper = GetSliderValue(copperSlider),
            iron = GetSliderValue(ironSlider),
            cobalt = GetSliderValue(cobaltSlider)
        };
    }

    private float GetSliderValue(Slider slider)
    {
        return slider != null ? slider.value : 0f;
    }

    private static string FormatPercent(float value)
    {
        return Mathf.Clamp01(value).ToString("P0");
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
        ResetSlider(copperSlider);
        ResetSlider(ironSlider);
        ResetSlider(cobaltSlider);
        if (triangleMixer != null)
        {
            triangleMixer.ResetMixer();
        }
        SetText(copperValueText, "0%");
        SetText(ironValueText, "0%");
        SetText(cobaltValueText, "0%");
        SetText(glazeMatchText, "釉料匹配：0.0%");
        UpdateMatchVideo(0f);
    }

    private static void ResetSlider(Slider slider)
    {
        if (slider != null) slider.value = 0f;
    }

    private void OnToFiringClicked()
    {
        if (gameManager != null)
        {
            gameManager.GoToFiring();
        }
    }

    private void AutoBindTriangleMixer()
    {
        if (triangleMixer == null)
        {
            triangleMixer = GetComponentInChildren<GlazeTriangleMixer>(true);
        }
    }

    private void RegisterTriangleMixer()
    {
        if (triangleMixer != null)
        {
            triangleMixer.MixChanged -= OnTriangleMixChanged;
            triangleMixer.MixChanged += OnTriangleMixChanged;
        }
    }

    private void UnregisterTriangleMixer()
    {
        if (triangleMixer != null)
        {
            triangleMixer.MixChanged -= OnTriangleMixChanged;
        }
    }

    private void OnTriangleMixChanged(GlazeInput input)
    {
        Refresh();
    }

    private void AutoBindMatchVideoReferences()
    {
        if (matchVideoImage == null)
        {
            Transform found = transform.Find("MatchVideo_Bar/RawImage_MatchVideo");
            matchVideoImage = found != null ? found.GetComponent<RawImage>() : null;
        }

        if (matchFrameImage == null)
        {
            Transform found = transform.Find("MatchVideo_Bar/Image_MatchFrame");
            if (found == null)
            {
                found = transform.Find("MatchVideo_Bar/RawImage_MatchVideo");
            }
            matchFrameImage = found != null ? found.GetComponent<Image>() : null;
        }

        if (matchVideoPlayer == null)
        {
            Transform found = transform.Find("MatchVideo_Bar/VideoPlayer_MatchVideo");
            matchVideoPlayer = found != null ? found.GetComponent<VideoPlayer>() : null;
        }

        if (HasMatchFrameSprites())
        {
            if (matchVideoImage != null)
            {
                matchVideoImage.enabled = false;
            }

            if (matchFrameImage != null)
            {
                matchFrameImage.enabled = true;
                matchFrameImage.preserveAspect = true;
                matchFrameImage.raycastTarget = false;
            }
            return;
        }

        if (matchVideoImage != null && matchVideoTexture != null)
        {
            matchVideoImage.enabled = true;
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
        Debug.LogWarning("Glaze match video could not be prepared: " + message);
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

        if (!matchVideoPlayer.isActiveAndEnabled)
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
        if (matchVideoPlayer != null && matchVideoPlayer.isActiveAndEnabled && !matchVideoPreparing)
        {
            matchVideoPreparing = true;
            matchVideoPlayer.Prepare();
        }
    }

    private bool UpdateMatchVideoFrameSequence(float matchPercent)
    {
        if (UpdateMatchSpriteFrameSequence(matchPercent))
        {
            return true;
        }

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

    private bool UpdateMatchSpriteFrameSequence(float matchPercent)
    {
        if (matchFrameImage == null || !HasMatchFrameSprites())
        {
            return false;
        }

        int frameIndex = Mathf.RoundToInt(Mathf.Clamp01(matchPercent / 100f) * (matchFrameSprites.Length - 1));
        frameIndex = Mathf.Clamp(frameIndex, 0, matchFrameSprites.Length - 1);

        Sprite frame = matchFrameSprites[frameIndex];
        if (frame == null)
        {
            return false;
        }

        if (lastAppliedMatchFrame != frameIndex || matchFrameImage.sprite != frame)
        {
            matchFrameImage.sprite = frame;
            matchFrameImage.enabled = true;
            matchFrameImage.preserveAspect = true;
            matchFrameImage.raycastTarget = false;
            lastAppliedMatchFrame = frameIndex;
        }

        if (matchVideoImage != null)
        {
            matchVideoImage.enabled = false;
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

    private bool HasMatchFrameSprites()
    {
        return matchFrameSprites != null && matchFrameSprites.Length > 0;
    }

    private void SeekMatchVideo(float matchPercent)
    {
        if (matchVideoPlayer == null || matchVideoPlayer.clip == null)
        {
            return;
        }

        if (!matchVideoPlayer.isActiveAndEnabled)
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
}
