using UnityEngine;
using UnityEngine.UI;

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

    private void OnEnable()
    {
        RegisterListeners();
        Refresh();
    }

    private void Start()
    {
        RegisterListeners();
        if (toFiringButton != null)
        {
            toFiringButton.onClick.AddListener(OnToFiringClicked);
        }
        Refresh();
    }

    private void OnDisable()
    {
        UnregisterListeners();
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

        float copper = GetSliderValue(copperSlider);
        float iron = GetSliderValue(ironSlider);
        float cobalt = GetSliderValue(cobaltSlider);

        SetText(copperValueText, copper.ToString("0.000"));
        SetText(ironValueText, iron.ToString("0.000"));
        SetText(cobaltValueText, cobalt.ToString("0.000"));

        var result = glazeSystem.Calculate(
            new GlazeInput { copper = copper, iron = iron, cobalt = cobalt }
        );
        SetText(glazeMatchText, "Glaze Match: " + result.overallScore.ToString("F1") + "%");
    }

    private void RegisterListeners()
    {
        AddListener(copperSlider);
        AddListener(ironSlider);
        AddListener(cobaltSlider);
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
        ResetSlider(copperSlider);
        ResetSlider(ironSlider);
        ResetSlider(cobaltSlider);
        SetText(copperValueText, "0.000");
        SetText(ironValueText, "0.000");
        SetText(cobaltValueText, "0.000");
        SetText(glazeMatchText, "Glaze Match: 0.0%");
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
}
