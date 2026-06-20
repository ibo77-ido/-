using UnityEngine;
using UnityEngine.UI;

public class MainPanelOutsideClickCloser : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform panelBounds;
    [SerializeField] private Button openButton;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (panelBounds == null && panelRoot != null)
        {
            panelBounds = panelRoot.GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        if (openButton != null)
        {
            openButton.onClick.AddListener(TogglePanel);
        }
    }

    private void OnDisable()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(TogglePanel);
        }
    }

    private void Update()
    {
        if (panelRoot == null || panelBounds == null || !panelRoot.activeSelf)
        {
            return;
        }

        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (!RectTransformUtility.RectangleContainsScreenPoint(panelBounds, Input.mousePosition, null))
        {
            ClosePanel();
        }
    }

    public void OpenPanel()
    {
        SetPanelActive(true);
    }

    public void ClosePanel()
    {
        SetPanelActive(false);
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
        {
            return;
        }

        SetPanelActive(!panelRoot.activeSelf);
    }

    private void SetPanelActive(bool active)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(active);
        }
    }
}
