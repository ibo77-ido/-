using System;
using System.Reflection;
using Phase10_Narrative;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class Phase9RebuiltRuntimeControls : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform mapRoot;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float arriveDistance = 0.05f;

    private Vector3 destination;
    private bool hasDestination;
    private RectTransform messagePanel;
    private Text messageText;
    private float hideMessageAt;

    private void Awake()
    {
        ResolveReferences();
        CreateHud();
    }

    private void Update()
    {
        ResolveReferences();
        HandleMouseMove();
        TickMovement();
        TickMessage();
    }

    public void EnterOrder()
    {
        MoveNear("Order-interact");
        EnterPhase3Module(AreaType.Order, "接单");
    }

    public void EnterShape()
    {
        MoveNear("Shape-interact");
        EnterPhase3Module(AreaType.Wheel, "拉坯");
    }

    public void EnterGlaze()
    {
        MoveNear("Glaze-interact");
        EnterPhase3Module(AreaType.Glaze, "上釉");
    }

    public void EnterKiln()
    {
        MoveNear("Kiln-interact");
        EnterPhase3Module(AreaType.Kiln, "烧制");
    }

    public void StartPhase10()
    {
        Phase9Phase10Bridge bridge = FindObjectOfType<Phase9Phase10Bridge>();
        if (bridge != null && bridge.SubmitGameStarted())
        {
            ShowMessage("Phase10 剧情已启动");
            return;
        }

        ShowMessage("Phase10 桥接已就绪");
    }

    private void HandleMouseMove()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector3 target;
        if (TryGetWorldPoint(Input.mousePosition, out target))
        {
            MoveTo(target);
        }
    }

    private void TickMovement()
    {
        if (!hasDestination || player == null)
        {
            return;
        }

        Vector3 current = player.position;
        Vector3 next = Vector3.MoveTowards(current, destination, moveSpeed * Time.deltaTime);
        player.position = next;

        if ((next - destination).sqrMagnitude <= arriveDistance * arriveDistance)
        {
            hasDestination = false;
        }
    }

    private void MoveNear(string targetName)
    {
        GameObject target = GameObject.Find(targetName);
        if (target != null)
        {
            MoveTo(target.transform.position);
        }
    }

    private void MoveTo(Vector3 target)
    {
        if (player == null)
        {
            return;
        }

        target.y = player.position.y;
        destination = ClampToMapBounds(target);
        hasDestination = true;
    }

    private bool TryGetWorldPoint(Vector3 screenPosition, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        if (targetCamera == null)
        {
            return false;
        }

        float planeY = player != null ? player.position.y : 0f;
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
        float enter;
        if (!plane.Raycast(ray, out enter))
        {
            return false;
        }

        worldPoint = ray.GetPoint(enter);
        worldPoint.y = planeY;
        return true;
    }

    private Vector3 ClampToMapBounds(Vector3 target)
    {
        if (mapRoot == null)
        {
            return target;
        }

        Renderer[] renderers = mapRoot.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
        {
            return target;
        }

        target.x = Mathf.Clamp(target.x, bounds.min.x, bounds.max.x);
        target.z = Mathf.Clamp(target.z, bounds.min.z, bounds.max.z);
        return target;
    }

    private void EnterNearestPhase3Module()
    {
        Phase9InteractionBridge bridge = FindObjectOfType<Phase9InteractionBridge>();
        if (bridge == null)
        {
            ShowMessage("Phase3 桥接未找到");
            return;
        }

        if (InvokeBridgeEnterNearest(bridge))
        {
            ShowMessage("Phase3 模块已打开");
        }
        else
        {
            ShowMessage("已移动到工位，靠近后按 E 进入模块");
        }
    }

    private void EnterPhase3Module(AreaType areaType, string label)
    {
        Phase9InteractionBridge bridge = FindObjectOfType<Phase9InteractionBridge>();
        if (bridge == null)
        {
            ShowMessage("Phase3 桥接未找到");
            return;
        }

        if (bridge.TryEnterGameplayModule(areaType))
        {
            ShowMessage("Phase3 " + label + "模块已打开");
        }
        else
        {
            ShowMessage("Phase3 模块暂不可用，请稍后再试");
        }
    }

    private static bool InvokeBridgeEnterNearest(Phase9InteractionBridge bridge)
    {
        MethodInfo method = typeof(Phase9InteractionBridge).GetMethod("TryEnterNearestGameplayModule", BindingFlags.Instance | BindingFlags.Public);
        object result = method != null ? method.Invoke(bridge, null) : null;
        return result is bool && (bool)result;
    }

    private void ResolveReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.Find("HeroineRoot");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (mapRoot == null)
        {
            GameObject rootObject = GameObject.Find("静态层");
            if (rootObject == null)
            {
                rootObject = GameObject.Find("闈欐€佸眰");
            }

            if (rootObject != null)
            {
                mapRoot = rootObject.transform;
            }
        }
    }

    private void CreateHud()
    {
        if (GameObject.Find("Phase9_RebuiltHUD") != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Phase9_RebuiltHUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject bar = CreatePanel(canvasObject.transform, "QuickBar", new Color(0.09f, 0.075f, 0.055f, 0.82f));
        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0f);
        barRect.anchorMax = new Vector2(0.5f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = new Vector2(0f, 20f);
        barRect.sizeDelta = new Vector2(820f, 76f);

        CreateButton(bar.transform, "接单", new Vector2(-320f, 0f), EnterOrder);
        CreateButton(bar.transform, "拉坯", new Vector2(-160f, 0f), EnterShape);
        CreateButton(bar.transform, "上釉", new Vector2(0f, 0f), EnterGlaze);
        CreateButton(bar.transform, "烧制", new Vector2(160f, 0f), EnterKiln);
        CreateButton(bar.transform, "剧情", new Vector2(320f, 0f), StartPhase10);

        GameObject panel = CreatePanel(canvasObject.transform, "MessagePanel", new Color(0.04f, 0.035f, 0.03f, 0.75f));
        messagePanel = panel.GetComponent<RectTransform>();
        messagePanel.anchorMin = new Vector2(0.5f, 1f);
        messagePanel.anchorMax = new Vector2(0.5f, 1f);
        messagePanel.pivot = new Vector2(0.5f, 1f);
        messagePanel.anchoredPosition = new Vector2(0f, -22f);
        messagePanel.sizeDelta = new Vector2(520f, 48f);
        messageText = CreateText(panel.transform, "Text", "点击地图移动，按钮进入 Phase3/Phase10 功能", 22);
        ShowMessage("点击地图移动，按钮进入 Phase3/Phase10 功能", 5f);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private static void CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(128f, 46f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.64f, 0.36f, 0.16f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(action);
        CreateText(buttonObject.transform, "Text", label, 22);
    }

    private static Text CreateText(Transform parent, string name, string value, int fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = new Color(1f, 0.9f, 0.72f, 1f);
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    private void ShowMessage(string message, float duration = 2.4f)
    {
        if (messageText == null || messagePanel == null)
        {
            return;
        }

        messageText.text = message;
        messagePanel.gameObject.SetActive(true);
        hideMessageAt = Time.realtimeSinceStartup + duration;
    }

    private void TickMessage()
    {
        if (messagePanel != null && messagePanel.gameObject.activeSelf && Time.realtimeSinceStartup > hideMessageAt)
        {
            messagePanel.gameObject.SetActive(false);
        }
    }
}
