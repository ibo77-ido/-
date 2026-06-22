using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SafePhase3RuntimeInstaller
{
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.NonPublic;

    public static GameManager EnsureInstalled(Transform parent)
    {
        GameManager existing = UnityEngine.Object.FindObjectOfType<GameManager>(true);
        if (existing != null)
        {
            return existing;
        }

        GameObject root = new GameObject("SafePhase3Runtime");
        if (parent != null)
        {
            root.transform.SetParent(parent, false);
        }

        GameObject systemsRoot = CreateChild(root.transform, "Systems");
        OrderManager orderManager = systemsRoot.AddComponent<OrderManager>();
        ShapeSystem shapeSystem = systemsRoot.AddComponent<ShapeSystem>();
        GlazeSystem glazeSystem = systemsRoot.AddComponent<GlazeSystem>();
        FiringSystem firingSystem = systemsRoot.AddComponent<FiringSystem>();
        ResultSystem resultSystem = systemsRoot.AddComponent<ResultSystem>();

        GameObject gameManagerObject = CreateChild(root.transform, "GameManager");
        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

        Canvas canvas = CreateCanvas(root.transform);
        GameObject panelOrder = CreateModulePanel(canvas.transform, "Panel_Order", "接单", "接受订单", gameManager.GoToShape);
        GameObject panelShape = CreateModulePanel(canvas.transform, "Panel_Shape", "拉胚", "完成拉胚", gameManager.GoToGlaze);
        GameObject panelGlaze = CreateModulePanel(canvas.transform, "Panel_Glaze", "配釉", "完成配釉", gameManager.GoToFiring);
        GameObject panelFiring = CreateModulePanel(canvas.transform, "Panel_Firing", "烧窑", "完成烧窑", gameManager.GoToResult);
        GameObject panelResult = CreateModulePanel(canvas.transform, "Panel_Result", "结算", "回到窑厂", gameManager.GoToNextOrder);

        SetPrivateField(gameManager, "autoStartInPhase3Scene", false);
        SetPrivateField(gameManager, "panelOrder", panelOrder);
        SetPrivateField(gameManager, "panelShape", panelShape);
        SetPrivateField(gameManager, "panelGlaze", panelGlaze);
        SetPrivateField(gameManager, "panelFiring", panelFiring);
        SetPrivateField(gameManager, "panelResult", panelResult);
        SetPrivateField(gameManager, "orderManager", orderManager);
        SetPrivateField(gameManager, "shapeSystem", shapeSystem);
        SetPrivateField(gameManager, "glazeSystem", glazeSystem);
        SetPrivateField(gameManager, "firingSystem", firingSystem);
        SetPrivateField(gameManager, "resultSystem", resultSystem);

        SetPrivateField(shapeSystem, "orderManager", orderManager);
        SetPrivateField(glazeSystem, "orderManager", orderManager);
        SetPrivateField(glazeSystem, "firingSystem", firingSystem);
        SetPrivateField(resultSystem, "shapeSystem", shapeSystem);
        SetPrivateField(resultSystem, "glazeSystem", glazeSystem);
        SetPrivateField(resultSystem, "firingSystem", firingSystem);
        SetPrivateField(resultSystem, "orderManager", orderManager);

        panelOrder.SetActive(false);
        panelShape.SetActive(false);
        panelGlaze.SetActive(false);
        panelFiring.SetActive(false);
        panelResult.SetActive(false);

        EnsureEventSystem();
        Debug.Log("[SafePhase3RuntimeInstaller] Installed safe Phase3 runtime modules.");
        return gameManager;
    }

    private static Canvas CreateCanvas(Transform parent)
    {
        GameObject canvasObject = CreateChild(parent, "GameplayCanvasRoot");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static GameObject CreateModulePanel(Transform parent, string name, string title, string buttonText, Action buttonAction)
    {
        GameObject panel = CreateUiObject(parent, name);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.08f, 0.06f, 0.04f, 0.92f);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(760f, 420f);

        Text titleText = CreateText(panel.transform, "Title", title, 46, new Vector2(0f, 120f));
        titleText.color = new Color(1f, 0.86f, 0.55f, 1f);

        Text bodyText = CreateText(panel.transform, "Body", "Phase3 模块已安全接入 Phase9。", 26, new Vector2(0f, 40f));
        bodyText.color = Color.white;

        Button button = CreateButton(panel.transform, "ContinueButton", buttonText, new Vector2(0f, -100f));
        if (buttonAction != null)
        {
            button.onClick.AddListener(() => buttonAction());
        }

        return panel;
    }

    private static Text CreateText(Transform parent, string name, string value, int fontSize, Vector2 position)
    {
        GameObject textObject = CreateUiObject(parent, name);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(640f, 90f);
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position)
    {
        GameObject buttonObject = CreateUiObject(parent, name);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.74f, 0.42f, 0.16f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.9f, 0.56f, 0.22f, 1f);
        colors.pressedColor = new Color(0.45f, 0.22f, 0.08f, 1f);
        button.colors = colors;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(260f, 72f);

        Text text = CreateText(buttonObject.transform, "Text", label, 28, Vector2.zero);
        text.color = Color.white;
        text.raycastTarget = false;
        text.GetComponent<RectTransform>().sizeDelta = rect.sizeDelta;
        return button;
    }

    private static GameObject CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static GameObject CreateUiObject(Transform parent, string name)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            return;
        }

        FieldInfo field = target.GetType().GetField(fieldName, InstanceFields);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
