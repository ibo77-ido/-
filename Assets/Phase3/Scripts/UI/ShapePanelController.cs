using UnityEngine;
using UnityEngine.UI;

public class ShapePanelController : MonoBehaviour
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

    private void OnEnable()
    {
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

        float mouth = GetSliderValue(mouthSlider);
        float neck = GetSliderValue(neckSlider);
        float shoulder = GetSliderValue(shoulderSlider);
        float belly = GetSliderValue(bellySlider);
        float foot = GetSliderValue(footSlider);

        SetText(mouthValueText, mouth.ToString("0.00"));
        SetText(neckValueText, neck.ToString("0.00"));
        SetText(shoulderValueText, shoulder.ToString("0.00"));
        SetText(bellyValueText, belly.ToString("0.00"));
        SetText(footValueText, foot.ToString("0.00"));

        float match = shapeSystem.Calculate(
            new ShapeInput { mouth = mouth, neck = neck, shoulder = shoulder, belly = belly, foot = foot }
        ).overallScore;
        SetText(shapeMatchText, "Shape Match: " + match.ToString("F1") + "%");
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
        ResetSlider(mouthSlider);
        ResetSlider(neckSlider);
        ResetSlider(shoulderSlider);
        ResetSlider(bellySlider);
        ResetSlider(footSlider);
        SetText(mouthValueText, "0.00");
        SetText(neckValueText, "0.00");
        SetText(shoulderValueText, "0.00");
        SetText(bellyValueText, "0.00");
        SetText(footValueText, "0.00");
        SetText(shapeMatchText, "Shape Match: 0.0%");
    }

    private static void ResetSlider(Slider slider)
    {
        if (slider != null) slider.value = 0f;
    }

    private void OnToGlazeClicked()
    {
        if (gameManager != null)
        {
            gameManager.GoToGlaze();
        }
    }
}
