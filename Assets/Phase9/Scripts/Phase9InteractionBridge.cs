using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class Phase9InteractionBridge : MonoBehaviour, IGameplayProgressionAuthority
{
    [Header("Scene Names")]
    [SerializeField] private string phase3SceneName = "Phase3_Prototype";
    [SerializeField] private string interactionRootName = "Interaction";
    [SerializeField] private string playerName = "\u5973\u4E3B";

    [Header("Interaction")]
    [SerializeField, Min(0.1f)] private float interactionDistance = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Runtime UI")]
    [SerializeField] private GameObject gameplayCanvasRoot;
    [SerializeField] private GameplayCanvasGroup gameplayCanvasGroup;

    private readonly List<EntryPoint> entryPoints = new List<EntryPoint>();

    private Transform player;
    private GameManager phase3GameManager;
    private ResultPanelController resultPanelController;
    private GraphicRaycaster phase3GraphicRaycaster;
    private RuntimeMode currentRuntimeMode = RuntimeMode.WorldMode;
    private bool isPhase3Loaded;
    private bool allowPhase3Progression;
    private bool orderDone;
    private bool shapeDone;
    private bool glazeDone;
    private bool kilnDone;

    private sealed class EntryPoint
    {
        public string Name;
        public AreaType AreaType;
        public Transform Transform;
    }

    private void Awake()
    {
        EnsureCanvasRoot();
        RegisterEntryPoints();

        SceneManager.sceneLoaded += OnSceneLoaded;
        if (!SceneManager.GetSceneByName(phase3SceneName).isLoaded)
        {
            SceneManager.LoadSceneAsync(phase3SceneName, LoadSceneMode.Additive);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (resultPanelController != null)
        {
            resultPanelController.OnExitGameplayEvent.RemoveListener(OnResultExitRequested);
        }

        if (phase3GameManager != null)
        {
            phase3GameManager.SetProgressionAuthority(null);
        }
    }

    private void Update()
    {
        if (currentRuntimeMode == RuntimeMode.GameplayMode && Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsRoundComplete() && phase3GameManager != null && phase3GameManager.CurrentState == GameState.Result)
            {
                CloseResultAndResetRound();
            }
            else
            {
                ExitGameplayModule();
            }

            return;
        }

        if (currentRuntimeMode != RuntimeMode.WorldMode || !isPhase3Loaded)
        {
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.Find(playerName);
            player = playerObject != null ? playerObject.transform : null;
        }

        if (player == null || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        EntryPoint nearest = FindNearestEntryPoint();
        if (nearest != null)
        {
            EnterGameplay(nearest.AreaType);
        }
    }

    public bool CanAutoAdvanceGameplay()
    {
        return allowPhase3Progression;
    }

    public void NotifyGameplayModuleCompleted(GameState completedState)
    {
        switch (completedState)
        {
            case GameState.Order:
                orderDone = true;
                ExitGameplayModule();
                break;
            case GameState.Shape:
                shapeDone = true;
                ExitGameplayModule();
                break;
            case GameState.Glaze:
                glazeDone = true;
                ExitGameplayModule();
                break;
            case GameState.Firing:
                kilnDone = true;
                ExitGameplayModule();
                break;
            case GameState.Result:
                CloseResultAndResetRound();
                break;
        }

        if (currentRuntimeMode == RuntimeMode.WorldMode && IsRoundComplete())
        {
            ShowRoundResult();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != phase3SceneName || mode != LoadSceneMode.Additive)
        {
            return;
        }

        RemoveDuplicateEventSystems(scene);
        BindPhase3References(scene);
        ReparentPhase3Canvas(scene);
        DisablePhase3Cameras(scene);
        EnsureEventSystem();
        BindCanvasGroupReferences();
        HideBridgeOnlyObjects();
        CenterGameplayPanels();
        HideGameplayUI();

        if (phase3GameManager != null)
        {
            phase3GameManager.SetProgressionAuthority(this);
        }

        if (resultPanelController != null)
        {
            resultPanelController.OnExitGameplayEvent.AddListener(OnResultExitRequested);
        }

        isPhase3Loaded = phase3GameManager != null;
    }

    private void RegisterEntryPoints()
    {
        entryPoints.Clear();

        Transform root = FindTransform(interactionRootName);
        if (root == null)
        {
            Debug.LogWarning("[Phase9InteractionBridge] Interaction root not found.");
            return;
        }

        AddEntry(root, "Order-interact", AreaType.Order);
        AddEntry(root, "Shape-interact", AreaType.Wheel);
        AddEntry(root, "Glaze-interact", AreaType.Glaze);
        AddEntry(root, "Kiln-interact", AreaType.Kiln);
    }

    private void AddEntry(Transform root, string childName, AreaType areaType)
    {
        Transform entry = root.Find(childName);
        if (entry == null)
        {
            Debug.LogWarning("[Phase9InteractionBridge] Missing interaction point: " + childName);
            return;
        }

        entryPoints.Add(new EntryPoint
        {
            Name = childName,
            AreaType = areaType,
            Transform = entry
        });
    }

    private EntryPoint FindNearestEntryPoint()
    {
        EntryPoint nearest = null;
        float nearestDistance = float.MaxValue;
        Vector3 playerPosition = player.position;

        for (int i = 0; i < entryPoints.Count; i++)
        {
            EntryPoint entry = entryPoints[i];
            if (entry.Transform == null)
            {
                continue;
            }

            float distance = Vector3.Distance(playerPosition, entry.Transform.position);
            if (distance <= interactionDistance && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = entry;
            }
        }

        return nearest;
    }

    private void EnterGameplay(AreaType areaType)
    {
        if (phase3GameManager == null || currentRuntimeMode != RuntimeMode.WorldMode)
        {
            return;
        }

        Debug.Log("[Phase9InteractionBridge] Enter " + areaType);
        currentRuntimeMode = RuntimeMode.GameplayMode;
        ShowGameplayUI();
        HideBridgeOnlyObjects();
        CenterGameplayPanels();
        StopPlayerMotion();

        switch (areaType)
        {
            case AreaType.Order:
                phase3GameManager.EnterOrderModule();
                break;
            case AreaType.Wheel:
                phase3GameManager.EnterShapeModule();
                break;
            case AreaType.Glaze:
                phase3GameManager.EnterGlazeModule();
                break;
            case AreaType.Kiln:
                phase3GameManager.EnterFiringModule();
                break;
        }
    }

    private void ExitGameplayModule()
    {
        Debug.Log("[Phase9InteractionBridge] Exit module. Progress: "
            + "Order=" + orderDone
            + ", Shape=" + shapeDone
            + ", Glaze=" + glazeDone
            + ", Kiln=" + kilnDone);

        if (phase3GameManager != null)
        {
            phase3GameManager.StopGameplayLoop();
        }

        HideGameplayUI();
        currentRuntimeMode = RuntimeMode.WorldMode;
    }

    private bool IsRoundComplete()
    {
        return orderDone && shapeDone && glazeDone && kilnDone;
    }

    private void ShowRoundResult()
    {
        if (phase3GameManager == null)
        {
            return;
        }

        Debug.Log("[Phase9InteractionBridge] Round complete. Showing result panel.");
        currentRuntimeMode = RuntimeMode.GameplayMode;
        ShowGameplayUI();
        allowPhase3Progression = true;
        phase3GameManager.GoToResult();
        allowPhase3Progression = false;
        HideBridgeOnlyObjects();
        CenterGameplayPanels();
    }

    private void OnResultExitRequested()
    {
        CloseResultAndResetRound();
    }

    private void CloseResultAndResetRound()
    {
        if (phase3GameManager != null)
        {
            phase3GameManager.AdvanceOrderForBridge();
        }

        orderDone = false;
        shapeDone = false;
        glazeDone = false;
        kilnDone = false;
        HideGameplayUI();
        currentRuntimeMode = RuntimeMode.WorldMode;
    }

    private void EnsureCanvasRoot()
    {
        if (gameplayCanvasRoot == null)
        {
            gameplayCanvasRoot = GameObject.Find("GameplayCanvasRoot");
        }

        if (gameplayCanvasRoot == null)
        {
            gameplayCanvasRoot = new GameObject("GameplayCanvasRoot");
            gameplayCanvasRoot.transform.SetParent(transform, false);
        }

        if (gameplayCanvasGroup == null)
        {
            gameplayCanvasGroup = gameplayCanvasRoot.GetComponent<GameplayCanvasGroup>();
        }

        if (gameplayCanvasGroup == null)
        {
            gameplayCanvasGroup = gameplayCanvasRoot.AddComponent<GameplayCanvasGroup>();
        }
    }

    private void BindPhase3References(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (phase3GameManager == null)
            {
                phase3GameManager = rootObject.GetComponentInChildren<GameManager>(true);
            }

            if (resultPanelController == null)
            {
                resultPanelController = rootObject.GetComponentInChildren<ResultPanelController>(true);
            }
        }
    }

    private void ReparentPhase3Canvas(Scene scene)
    {
        if (gameplayCanvasRoot == null)
        {
            return;
        }

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Canvas canvas = rootObject.GetComponentInChildren<Canvas>(true);
            if (canvas == null)
            {
                continue;
            }

            canvas.transform.SetParent(gameplayCanvasRoot.transform, false);
            ConfigurePhase3Canvas(canvas);
            phase3GraphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (phase3GraphicRaycaster == null)
            {
                phase3GraphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            return;
        }
    }

    private static void ConfigurePhase3Canvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform rect = canvas.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }

    private void BindCanvasGroupReferences()
    {
        if (phase3GameManager == null || gameplayCanvasGroup == null)
        {
            return;
        }

        FieldInfo[] fields =
        {
            typeof(GameManager).GetField("panelOrder", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(GameManager).GetField("panelShape", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(GameManager).GetField("panelGlaze", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(GameManager).GetField("panelFiring", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(GameManager).GetField("panelResult", BindingFlags.NonPublic | BindingFlags.Instance)
        };

        string[] groupFields = { "panelOrder", "panelShape", "panelGlaze", "panelFiring", "panelResult" };
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i] == null)
            {
                continue;
            }

            GameObject panel = fields[i].GetValue(phase3GameManager) as GameObject;
            FieldInfo groupField = typeof(GameplayCanvasGroup).GetField(groupFields[i], BindingFlags.NonPublic | BindingFlags.Instance);
            if (groupField != null)
            {
                groupField.SetValue(gameplayCanvasGroup, panel);
            }
        }
    }

    private void HideBridgeOnlyObjects()
    {
        if (gameplayCanvasRoot == null)
        {
            return;
        }

        Transform[] children = gameplayCanvasRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            GameObject target = children[i].gameObject;
            if (target.name == "Panel_Debug" || target.name == "Text_Title")
            {
                target.SetActive(false);
            }
        }
    }

    private void CenterGameplayPanels()
    {
        if (gameplayCanvasRoot == null)
        {
            return;
        }

        RectTransform[] rects = gameplayCanvasRoot.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            GameObject target = rects[i].gameObject;
            if (target.name == "Panel_Order"
                || target.name == "Panel_Shape"
                || target.name == "Panel_Glaze"
                || target.name == "Panel_Firing"
                || target.name == "Panel_Result")
            {
                RectTransform rect = rects[i];
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
            }
        }
    }

    private static void RemoveDuplicateEventSystems(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            EventSystem[] eventSystems = rootObject.GetComponentsInChildren<EventSystem>(true);
            for (int i = 0; i < eventSystems.Length; i++)
            {
                DestroyImmediate(eventSystems[i].gameObject);
            }
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static void DisablePhase3Cameras(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Camera[] cameras = rootObject.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].enabled = false;
            }

            AudioListener[] listeners = rootObject.GetComponentsInChildren<AudioListener>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
            }
        }
    }

    private void ShowGameplayUI()
    {
        if (gameplayCanvasRoot != null)
        {
            gameplayCanvasRoot.SetActive(true);
        }
    }

    private void HideGameplayUI()
    {
        if (gameplayCanvasRoot != null)
        {
            gameplayCanvasRoot.SetActive(false);
        }
    }

    private void StopPlayerMotion()
    {
        if (player != null)
        {
            player.SendMessage("StopMoving", SendMessageOptions.DontRequireReceiver);
        }
    }

    private static Transform FindTransform(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        return obj != null ? obj.transform : null;
    }
}
