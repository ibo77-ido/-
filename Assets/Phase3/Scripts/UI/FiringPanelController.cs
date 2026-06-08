using UnityEngine;
using UnityEngine.UI;

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

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        if (windSlider != null)
        {
            windSlider.onValueChanged.AddListener(OnWindSliderChanged);
            OnWindSliderChanged(windSlider.value);
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
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (firingSystem == null)
        {
            return;
        }

        string tempDisplay = firingSystem.IsWindowOpen
            ? firingSystem.CurrentTemperature.ToString("0") + "°C"
            : "???°C";
        SetText(temperatureText, "Temperature: " + tempDisplay);
        SetText(windowButtonText, firingSystem.IsWindowOpen ? "Close" : "Open");

        var zone = firingSystem.GetCurrentZone();
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
        if (zoneText != null)
        {
            zoneText.text = zoneLabel;
            zoneText.color = zoneColor;
        }

        string scoreDisplay = firingSystem.IsWindowOpen
            ? firingSystem.GetFireScore().ToString("0")
            : "???";
        SetText(fireScoreText, "Fire Score: " + scoreDisplay);

        SetText(statusText, firingSystem.IsFiring ? "烧制中..." : "烧制完成");

        if (openKilnButton != null && firingSystem.IsFiring)
        {
            openKilnButton.interactable = false;
        }
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
            Refresh();
        }
    }

    private void OnStopButtonClicked()
    {
        if (firingSystem != null && firingSystem.IsFiring)
        {
            firingSystem.StopFiring();
            stopButton.interactable = false;
            if (openKilnButton != null)
            {
                openKilnButton.interactable = true;
            }
        }
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
        if (windSlider != null) windSlider.value = 0f;
        if (stopButton != null) stopButton.interactable = true;
        if (openKilnButton != null) openKilnButton.interactable = false;
    }
}
