using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuickBarButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Button button;

    private AreaType areaType;
    private Action<AreaType> onClick;

    public void Initialize(HUDButtonData data, Action<AreaType> callback)
    {
        areaType = data.areaType;
        onClick = callback;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;
        if (labelText != null)
            labelText.text = data.label;

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        SfxPlayer.Play(SfxId.UiConfirm);
        onClick?.Invoke(areaType);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}
