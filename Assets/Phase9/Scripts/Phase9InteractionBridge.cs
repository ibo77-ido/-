using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
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

    [Header("World Movement")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private MovementController movementController;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private string walkableAreaName = "NavMesh-walkable";
    [SerializeField] private string walkableBakeMeshName = "NavMeshWalkableBakeMesh_FromAlpha";
    [SerializeField, Min(0.01f)] private float clickSampleDistance = 0.25f;
    [SerializeField, Min(0.05f)] private float nearestWalkableSearchRadius = 8f;
    [SerializeField, Min(0.02f)] private float nearestWalkableSearchStep = 0.12f;
    [SerializeField, Min(0.02f)] private float pathCoverageSampleStep = 0.08f;
    [SerializeField] private float navMeshPlaneY = 0.52f;

    [Header("Runtime UI")]
    [SerializeField] private GameObject gameplayCanvasRoot;
    [SerializeField] private GameplayCanvasGroup gameplayCanvasGroup;
    [SerializeField] private Vector2 bridgePanelReferenceSize = new Vector2(1920f, 1080f);
    [SerializeField, Min(0.1f)] private float bridgePanelScaleMultiplier = 2f;

    private readonly List<EntryPoint> entryPoints = new List<EntryPoint>();
    private readonly Dictionary<RectTransform, BridgePanelLayout> bridgePanelLayouts = new Dictionary<RectTransform, BridgePanelLayout>();

    private Transform player;
    private Transform walkableArea;
    private SpriteRenderer walkableRenderer;
    private MeshFilter walkableBakeMeshFilter;
    private Mesh walkableBakeMesh;
    private GameManager phase3GameManager;
    private ResultPanelController resultPanelController;
    private GraphicRaycaster phase3GraphicRaycaster;
    private RuntimeMode currentRuntimeMode = RuntimeMode.WorldMode;
    private bool isPhase3Loaded;
    private bool phase3SceneCanBeLoaded;
    private bool allowPhase3Progression;
    private bool orderDone;
    private bool shapeDone;
    private bool glazeDone;
    private bool kilnDone;
    private bool playerResolveAttempted;
    private bool walkableResolveAttempted;
    private bool walkableBakeMeshResolveAttempted;
    private bool navMeshPlaneResolved;
    private MovementController subscribedMovementController;

    private struct BridgePanelLayout
    {
        public Vector2 AnchoredPosition;
        public Vector2 Size;
        public Vector3 Scale;
    }

    private sealed class EntryPoint
    {
        public string Name;
        public AreaType AreaType;
        public Transform Transform;
    }

    private void Awake()
    {
        Debug.Log("[Phase9InteractionBridge] Awake begin");
        EnsureCanvasRoot();
        RegisterEntryPoints();
        ResolvePlayerReferences();
        ResolveWalkableArea();
        ResolveTargetCamera();
        UpdateNavMeshPlaneY();

        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBindPhase3Scene(SceneManager.GetActiveScene());
        phase3SceneCanBeLoaded = CanLoadPhase3Scene();
        if (!phase3SceneCanBeLoaded && !isPhase3Loaded)
        {
            Debug.LogWarning("[Phase9InteractionBridge] Phase3 scene is not included in this build. Installing safe runtime module: " + phase3SceneName);
            SafePhase3RuntimeInstaller.EnsureInstalled(transform);
            TryBindPhase3Scene(SceneManager.GetActiveScene());
        }
        else if (!isPhase3Loaded && !SceneManager.GetSceneByName(phase3SceneName).isLoaded)
        {
            Debug.Log("[Phase9InteractionBridge] Loading additive scene: " + phase3SceneName);
            SceneManager.LoadSceneAsync(phase3SceneName, LoadSceneMode.Additive);
        }
        Debug.Log("[Phase9InteractionBridge] Awake end");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeMovementController();

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

        if (currentRuntimeMode != RuntimeMode.WorldMode)
        {
            return;
        }

        if (GetPlayerTransform() == null)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            TryEnterNearestGameplayModule();
            return;
        }

        HandleWorldClickMove();
    }

    public bool CanMoveInWorldMode()
    {
        return currentRuntimeMode == RuntimeMode.WorldMode && (isPhase3Loaded || !phase3SceneCanBeLoaded);
    }

    public bool TryEnterNearestGameplayModule()
    {
        if (currentRuntimeMode != RuntimeMode.WorldMode || !isPhase3Loaded)
        {
            SfxPlayer.Play(SfxId.InteractFail);
            return false;
        }

        if (GetPlayerTransform() == null)
        {
            SfxPlayer.Play(SfxId.InteractFail);
            return false;
        }

        EntryPoint nearest = FindNearestEntryPoint();
        if (nearest == null)
        {
            SfxPlayer.Play(SfxId.InteractFail);
            return false;
        }

        SfxPlayer.Play(SfxId.InteractSuccess);
        EnterGameplay(nearest.AreaType);
        return true;
    }

    public bool TryEnterGameplayModule(AreaType areaType)
    {
        if (areaType == AreaType.None || currentRuntimeMode != RuntimeMode.WorldMode || !isPhase3Loaded || phase3GameManager == null)
        {
            SfxPlayer.Play(SfxId.InteractFail);
            return false;
        }

        SfxPlayer.Play(SfxId.InteractSuccess);
        EnterGameplay(areaType);
        return true;
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

        TryBindPhase3Scene(scene);
    }

    private void TryBindPhase3Scene(Scene scene)
    {
        if (isPhase3Loaded || !scene.IsValid())
        {
            return;
        }

        RemoveDuplicateEventSystems(scene);
        BindPhase3References(scene);
        if (phase3GameManager == null)
        {
            return;
        }

        ReparentPhase3Canvas(scene);
        DisablePhase3Cameras(scene);
        EnsureEventSystem();
        BindCanvasGroupReferences();
        HideBridgeOnlyObjects();
        bridgePanelLayouts.Clear();
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
        Transform playerTransform = GetPlayerTransform();
        if (playerTransform == null)
        {
            return null;
        }

        Vector3 playerPosition = playerTransform.position;

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
        SfxPlayer.Play(SfxId.InteractCancel);
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

    private bool CanLoadPhase3Scene()
    {
        return !string.IsNullOrWhiteSpace(phase3SceneName)
            && Application.CanStreamedLevelBeLoaded(phase3SceneName);
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
                ApplyBridgePanelLayout(rect);
            }
        }
    }

    private void ApplyBridgePanelLayout(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        BridgePanelLayout layout;
        if (!bridgePanelLayouts.TryGetValue(rect, out layout))
        {
            layout = CaptureBridgePanelLayout(rect);
            bridgePanelLayouts.Add(rect, layout);
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = rect.name == "Panel_Shape" ? layout.AnchoredPosition : Vector2.zero;
        rect.sizeDelta = layout.Size;
        rect.localScale = new Vector3(
            layout.Scale.x * bridgePanelScaleMultiplier,
            layout.Scale.y * bridgePanelScaleMultiplier,
            layout.Scale.z);
    }

    private BridgePanelLayout CaptureBridgePanelLayout(RectTransform rect)
    {
        Vector2 size = rect.rect.size;
        if (size.x <= 0f || size.y <= 0f)
        {
            size = rect.sizeDelta;
        }
        if (size.x <= 0f || size.y <= 0f)
        {
            size = bridgePanelReferenceSize;
        }
        if (size.x <= 0f)
        {
            size.x = 1920f;
        }
        if (size.y <= 0f)
        {
            size.y = 1080f;
        }

        Vector3 scale = rect.localScale;
        if (scale.x <= 0f)
        {
            scale.x = 1f;
        }
        if (scale.y <= 0f)
        {
            scale.y = scale.x;
        }
        if (scale.z <= 0f)
        {
            scale.z = 1f;
        }

        return new BridgePanelLayout
        {
            AnchoredPosition = rect.anchoredPosition,
            Size = size,
            Scale = scale
        };
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
        ResolvePlayerReferences();

        if (playerCharacter != null)
        {
            playerCharacter.StopMoving();
            return;
        }

        if (movementController != null)
        {
            movementController.Stop();
            return;
        }

        if (player != null)
        {
            player.SendMessage("StopMoving", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void HandleWorldClickMove()
    {
        if (!Input.GetMouseButtonDown(0) || !CanMoveInWorldMode())
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        ResolvePlayerReferences(false);
        if (playerCharacter == null || movementController == null)
        {
            return;
        }

        Vector3 navTarget;
        if (TryResolveMoveTarget(Input.mousePosition, out navTarget))
        {
            if (playerCharacter.TrySetDestination(navTarget))
            {
                SfxPlayer.Play(SfxId.NavStart);
            }
            else
            {
                SfxPlayer.Play(SfxId.Denied);
            }
            return;
        }

        SfxPlayer.Play(SfxId.Denied);
    }

    private bool TryResolveMoveTarget(Vector3 screenPosition, out Vector3 navTarget)
    {
        navTarget = Vector3.zero;

        if (!ResolveTargetCamera())
        {
            return false;
        }

        if (!ResolveWalkableArea())
        {
            return TryResolveFallbackMoveTarget(screenPosition, out navTarget);
        }

        Vector3 worldPoint;
        if (!TryGetWorldPointOnMapPlane(screenPosition, out worldPoint))
        {
            return false;
        }

        EnsureNavMeshPlaneY();

        NavMeshHit targetHit;
        if (!TryResolveNearestWalkableNavMeshPoint(worldPoint, out targetHit))
        {
            return false;
        }

        Vector3 hitWorldPoint = new Vector3(targetHit.position.x, worldPoint.y, targetHit.position.z);
        if (!IsInsideWalkableArea(hitWorldPoint))
        {
            return false;
        }

        Transform navMeshReference = GetNavMeshReferenceTransform();
        if (navMeshReference == null)
        {
            return false;
        }

        Vector3 currentCandidate = new Vector3(navMeshReference.position.x, navMeshPlaneY, navMeshReference.position.z);
        NavMeshHit currentHit;
        if (!NavMesh.SamplePosition(currentCandidate, out currentHit, clickSampleDistance, NavMesh.AllAreas))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(currentHit.position, targetHit.position, NavMesh.AllAreas, path))
        {
            return false;
        }

        if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length <= 1)
        {
            return false;
        }

        if (!IsPathInsideWalkableArea(path))
        {
            return false;
        }

        navTarget = targetHit.position;
        return true;
    }

    private bool TryResolveFallbackMoveTarget(Vector3 screenPosition, out Vector3 navTarget)
    {
        navTarget = Vector3.zero;
        if (!ResolveTargetCamera())
        {
            return false;
        }

        Vector3 worldPoint;
        if (!TryGetWorldPointOnMapPlane(screenPosition, out worldPoint))
        {
            return false;
        }

        Bounds bounds;
        if (TryGetWorldBounds(out bounds))
        {
            worldPoint.x = Mathf.Clamp(worldPoint.x, bounds.min.x, bounds.max.x);
            worldPoint.z = Mathf.Clamp(worldPoint.z, bounds.min.z, bounds.max.z);
        }

        navTarget = worldPoint;
        return true;
    }

    private bool TryResolveNearestWalkableNavMeshPoint(Vector3 worldPoint, out NavMeshHit targetHit)
    {
        Vector3 candidate = new Vector3(worldPoint.x, navMeshPlaneY, worldPoint.z);
        if (TrySampleInsideWalkableArea(candidate, clickSampleDistance, out targetHit))
        {
            return true;
        }

        if (TryFindNearestPointOnWalkableBakeMesh(candidate, out targetHit))
        {
            return true;
        }

        return TryFindNearestWalkableNavMeshPoint(candidate, out targetHit);
    }

    private bool TrySampleInsideWalkableArea(Vector3 candidate, float sampleDistance, out NavMeshHit targetHit)
    {
        if (NavMesh.SamplePosition(candidate, out targetHit, sampleDistance, NavMesh.AllAreas))
        {
            Vector3 hitWorldPoint = new Vector3(targetHit.position.x, navMeshPlaneY, targetHit.position.z);
            return IsInsideWalkableArea(hitWorldPoint);
        }

        return false;
    }

    private bool TryFindNearestPointOnWalkableBakeMesh(Vector3 candidate, out NavMeshHit nearestHit)
    {
        nearestHit = new NavMeshHit();
        if (!ResolveWalkableBakeMesh())
        {
            return false;
        }

        Vector3 localPoint = walkableBakeMeshFilter.transform.worldToLocalMatrix.MultiplyPoint3x4(candidate);
        Vector2 localPointXZ = new Vector2(localPoint.x, localPoint.z);
        Vector3[] vertices = walkableBakeMesh.vertices;
        int[] triangles = walkableBakeMesh.triangles;
        float bestSqrDistance = float.MaxValue;
        Vector3 bestWorldPoint = Vector3.zero;
        bool found = false;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = vertices[triangles[i]];
            Vector3 b = vertices[triangles[i + 1]];
            Vector3 c = vertices[triangles[i + 2]];
            Vector2 closest = ClosestPointOnTriangleXZ(
                localPointXZ,
                new Vector2(a.x, a.z),
                new Vector2(b.x, b.z),
                new Vector2(c.x, c.z));
            float sqrDistance = (closest - localPointXZ).sqrMagnitude;
            if (sqrDistance >= bestSqrDistance)
            {
                continue;
            }

            Vector3 localClosest = new Vector3(closest.x, localPoint.y, closest.y);
            bestWorldPoint = walkableBakeMeshFilter.transform.localToWorldMatrix.MultiplyPoint3x4(localClosest);
            bestWorldPoint.y = navMeshPlaneY;
            bestSqrDistance = sqrDistance;
            found = true;
        }

        if (!found)
        {
            return false;
        }

        return TrySampleInsideWalkableArea(bestWorldPoint, Mathf.Max(clickSampleDistance, nearestWalkableSearchStep), out nearestHit);
    }

    private bool TryFindNearestWalkableNavMeshPoint(Vector3 candidate, out NavMeshHit nearestHit)
    {
        nearestHit = new NavMeshHit();
        bool found = false;
        float bestSqrDistance = float.MaxValue;
        float step = Mathf.Max(0.02f, nearestWalkableSearchStep);
        int maxRing = Mathf.CeilToInt(Mathf.Max(step, nearestWalkableSearchRadius) / step);

        for (int ring = 1; ring <= maxRing; ring++)
        {
            float radius = ring * step;
            int sampleCount = Mathf.Max(8, Mathf.CeilToInt(2f * Mathf.PI * radius / step));

            for (int i = 0; i < sampleCount; i++)
            {
                float angle = i * Mathf.PI * 2f / sampleCount;
                Vector3 sample = candidate + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                NavMeshHit hit;
                if (!TrySampleInsideWalkableArea(sample, step, out hit))
                {
                    continue;
                }

                float sqrDistance = (new Vector3(hit.position.x, 0f, hit.position.z)
                    - new Vector3(candidate.x, 0f, candidate.z)).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    nearestHit = hit;
                    found = true;
                }
            }

            if (found)
            {
                return true;
            }
        }

        return false;
    }

    private static Vector2 ClosestPointOnTriangleXZ(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        if (IsPointInsideTriangleXZ(point, a, b, c))
        {
            return point;
        }

        Vector2 closestAB = ClosestPointOnSegment(point, a, b);
        Vector2 closestBC = ClosestPointOnSegment(point, b, c);
        Vector2 closestCA = ClosestPointOnSegment(point, c, a);
        float distanceAB = (point - closestAB).sqrMagnitude;
        float distanceBC = (point - closestBC).sqrMagnitude;
        float distanceCA = (point - closestCA).sqrMagnitude;

        if (distanceAB <= distanceBC && distanceAB <= distanceCA)
        {
            return closestAB;
        }

        return distanceBC <= distanceCA ? closestBC : closestCA;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float denominator = Vector2.Dot(ab, ab);
        if (denominator <= 0.000001f)
        {
            return a;
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / denominator);
        return a + ab * t;
    }

    private static bool IsPointInsideTriangleXZ(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        const float epsilon = 0.0001f;
        float denominator = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
        if (Mathf.Abs(denominator) < epsilon)
        {
            return false;
        }

        float u = ((b.y - c.y) * (point.x - c.x) + (c.x - b.x) * (point.y - c.y)) / denominator;
        float v = ((c.y - a.y) * (point.x - c.x) + (a.x - c.x) * (point.y - c.y)) / denominator;
        float w = 1f - u - v;
        return u >= -epsilon && v >= -epsilon && w >= -epsilon;
    }

    private bool TryGetWorldPointOnMapPlane(Vector3 screenPosition, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);
        Plane mapPlane = new Plane(Vector3.up, new Vector3(0f, navMeshPlaneY, 0f));

        float enter;
        if (!mapPlane.Raycast(ray, out enter))
        {
            return false;
        }

        worldPoint = ray.GetPoint(enter);
        worldPoint.y = navMeshPlaneY;
        return true;
    }

    private bool IsInsideWalkableArea(Vector3 worldPoint)
    {
        if (ResolveWalkableBakeMesh())
        {
            return IsInsideWalkableBakeMesh(worldPoint);
        }

        return IsInsideWalkableVisualBounds(worldPoint);
    }

    private bool IsPathInsideWalkableArea(NavMeshPath path)
    {
        if (path == null || path.corners == null || path.corners.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < path.corners.Length; i++)
        {
            if (!IsInsideWalkableArea(path.corners[i]))
            {
                return false;
            }
        }

        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 from = path.corners[i - 1];
            Vector3 to = path.corners[i];
            float distance = Vector3.Distance(from, to);
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(distance / pathCoverageSampleStep));
            for (int sample = 1; sample < sampleCount; sample++)
            {
                Vector3 point = Vector3.Lerp(from, to, sample / (float)sampleCount);
                if (!IsInsideWalkableArea(point))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsInsideWalkableBakeMesh(Vector3 worldPoint)
    {
        if (walkableBakeMeshFilter == null || walkableBakeMesh == null)
        {
            return false;
        }

        Vector3 localPoint = walkableBakeMeshFilter.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
        Vector3[] vertices = walkableBakeMesh.vertices;
        int[] triangles = walkableBakeMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (IsPointInsideTriangleXZ(
                localPoint,
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPointInsideTriangleXZ(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        const float epsilon = 0.0001f;
        float denominator = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
        if (Mathf.Abs(denominator) < epsilon)
        {
            return false;
        }

        float u = ((b.z - c.z) * (point.x - c.x) + (c.x - b.x) * (point.z - c.z)) / denominator;
        float v = ((c.z - a.z) * (point.x - c.x) + (a.x - c.x) * (point.z - c.z)) / denominator;
        float w = 1f - u - v;
        return u >= -epsilon && v >= -epsilon && w >= -epsilon;
    }

    private bool IsInsideWalkableVisualCoverage(Vector3 worldPoint)
    {
        return IsInsideWalkableArea(worldPoint);
    }

    private bool IsInsideWalkableVisualBounds(Vector3 worldPoint)
    {
        if (!ResolveWalkableArea())
        {
            return false;
        }

        Bounds bounds = walkableRenderer != null ? walkableRenderer.bounds : new Bounds(walkableArea.position, Vector3.zero);
        return worldPoint.x >= bounds.min.x
            && worldPoint.x <= bounds.max.x
            && worldPoint.z >= bounds.min.z
            && worldPoint.z <= bounds.max.z;
    }

    private bool ResolveWalkableBakeMesh()
    {
        if (walkableBakeMeshFilter != null && walkableBakeMesh != null)
        {
            return true;
        }

        if (walkableBakeMeshResolveAttempted)
        {
            return false;
        }

        walkableBakeMeshResolveAttempted = true;

        Transform bakeMeshTransform = FindTransform(walkableBakeMeshName);
        if (bakeMeshTransform == null && walkableBakeMeshName != "NavMeshWalkableBakeMesh_FromAlpha")
        {
            bakeMeshTransform = FindTransform("NavMeshWalkableBakeMesh_FromAlpha");
        }

        if (bakeMeshTransform == null)
        {
            return false;
        }

        walkableBakeMeshFilter = bakeMeshTransform.GetComponent<MeshFilter>();
        if (walkableBakeMeshFilter == null)
        {
            return false;
        }

        walkableBakeMesh = walkableBakeMeshFilter.sharedMesh;
        return walkableBakeMesh != null;
    }

    private bool ResolveWalkableArea()
    {
        if (walkableRenderer != null)
        {
            return true;
        }

        if (walkableResolveAttempted)
        {
            return false;
        }

        walkableResolveAttempted = true;

        if (walkableArea == null)
        {
            walkableArea = FindTransform(walkableAreaName);
        }

        if (walkableArea == null && walkableAreaName != "NavMesh-walkable")
        {
            walkableArea = FindTransform("NavMesh-walkable");
        }

        if (walkableArea == null && walkableAreaName != "NavMesh-walkable (1)")
        {
            walkableArea = FindTransform("NavMesh-walkable (1)");
        }

        if (walkableArea == null)
        {
            return false;
        }

        if (walkableRenderer == null)
        {
            walkableRenderer = walkableArea.GetComponent<SpriteRenderer>();
        }

        return walkableRenderer != null;
    }

    private static bool TryGetWorldBounds(out Bounds bounds)
    {
        bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;
        Renderer[] renderers = FindObjectsOfType<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled || renderer.GetComponentInParent<Canvas>(true) != null)
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

        return hasBounds;
    }

    private bool ResolveTargetCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        return targetCamera != null;
    }

    private void EnsureNavMeshPlaneY()
    {
        if (!navMeshPlaneResolved)
        {
            UpdateNavMeshPlaneY();
        }
    }

    private void UpdateNavMeshPlaneY()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices != null && triangulation.vertices.Length > 0)
        {
            navMeshPlaneY = triangulation.vertices[0].y;
            navMeshPlaneResolved = true;
            if (movementController != null)
            {
                movementController.SetMappedNavMeshY(navMeshPlaneY);
            }
        }
    }

    private void ResolvePlayerReferences(bool allowFind = true)
    {
        if (playerCharacter != null && movementController != null && player != null)
        {
            SubscribeMovementController();
            return;
        }

        if (playerCharacter != null)
        {
            player = playerCharacter.Transform;
        }

        if (player != null)
        {
            if (playerCharacter == null)
            {
                playerCharacter = player.GetComponent<PlayerCharacter>();
                if (playerCharacter == null)
                {
                    playerCharacter = player.GetComponentInParent<PlayerCharacter>();
                }
            }

            if (movementController == null)
            {
                movementController = player.GetComponent<MovementController>();
                if (movementController == null)
                {
                    movementController = player.GetComponentInParent<MovementController>();
                }
            }

            if (playerCharacter != null)
            {
                player = playerCharacter.Transform;
            }

            if (movementController != null && navMeshPlaneResolved)
            {
                movementController.SetMappedNavMeshY(navMeshPlaneY);
            }

            SubscribeMovementController();

            return;
        }

        if (!allowFind || playerResolveAttempted)
        {
            return;
        }

        playerResolveAttempted = true;

        GameObject playerObject = FindGameObject("HeroineRoot");
        if (playerObject == null)
        {
            playerObject = FindGameObject(playerName);
        }
        if (playerObject == null)
        {
            playerObject = FindGameObject("GirlModel");
        }
        if (playerObject == null)
        {
            playerObject = FindGameObject("\u5973\u4E3B");
        }
        if (playerObject == null)
        {
            return;
        }

        player = playerObject.transform;

        if (playerCharacter == null)
        {
            playerCharacter = playerObject.GetComponent<PlayerCharacter>();
            if (playerCharacter == null)
            {
                playerCharacter = playerObject.GetComponentInParent<PlayerCharacter>();
            }
        }

        if (movementController == null)
        {
            movementController = playerObject.GetComponent<MovementController>();
            if (movementController == null)
            {
                movementController = playerObject.GetComponentInParent<MovementController>();
            }
        }

        if (playerCharacter != null)
        {
            player = playerCharacter.Transform;
        }

        if (movementController != null && navMeshPlaneResolved)
        {
            movementController.SetMappedNavMeshY(navMeshPlaneY);
        }

        SubscribeMovementController();
    }

    private Transform GetPlayerTransform()
    {
        ResolvePlayerReferences(false);
        if (playerCharacter != null)
        {
            return playerCharacter.Transform;
        }

        return player;
    }

    private Transform GetNavMeshReferenceTransform()
    {
        ResolvePlayerReferences(false);

        if (movementController != null)
        {
            return movementController.transform;
        }

        if (playerCharacter != null)
        {
            return playerCharacter.Transform;
        }

        GameObject heroineRoot = FindGameObject("HeroineRoot");
        if (heroineRoot != null)
        {
            return heroineRoot.transform;
        }

        return GetPlayerTransform();
    }

    private void SubscribeMovementController()
    {
        if (movementController == null || subscribedMovementController == movementController)
        {
            return;
        }

        UnsubscribeMovementController();
        subscribedMovementController = movementController;
        subscribedMovementController.DestinationReached.AddListener(OnMovementDestinationReached);
    }

    private void UnsubscribeMovementController()
    {
        if (subscribedMovementController != null)
        {
            subscribedMovementController.DestinationReached.RemoveListener(OnMovementDestinationReached);
            subscribedMovementController = null;
        }
    }

    private void OnMovementDestinationReached()
    {
        if (currentRuntimeMode == RuntimeMode.WorldMode)
        {
            SfxPlayer.Play(SfxId.NavArrive);
        }
    }

    private static Transform FindTransform(string objectName)
    {
        GameObject obj = FindGameObject(objectName);
        return obj != null ? obj.transform : null;
    }

    private static GameObject FindGameObject(string objectName)
    {
        GameObject activeObject = GameObject.Find(objectName);
        if (activeObject != null)
        {
            return activeObject;
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate == null || candidate.name != objectName)
            {
                continue;
            }

            if (!candidate.scene.IsValid())
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
}
