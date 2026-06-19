using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sole runtime coordinator for Phase6 ↔ Phase3 bridge.
/// Owns session lifecycle, runtime mode switching, and gameplay entry/exit.
/// Implements IInteractionEntryHandler so Workstation can delegate without
/// knowing about Gameplay or Runtime concepts.
/// 
/// Phase3 is loaded additively at runtime. All Phase3 references are
/// discovered after scene load — not via Inspector SerializeField.
/// </summary>
public class GameplayBridgeManager : MonoBehaviour, IInteractionEntryHandler, IGameplayProgressionAuthority
{
    // ── Phase6 References (bound in Inspector, exist in base scene) ──
    [Header("Phase6 References")]
    [SerializeField] private Phase6GameManager phase6GameManager;
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private TestUIRouter testUIRouter;

    // ── Phase3 Scene Config (bound in Inspector) ──
    [Header("Phase3 Scene")]
    [SerializeField] private string phase3SceneName = "Phase3_Prototype";

    // ── Runtime Host (bound in Inspector, exists in base scene) ──
    [Header("Runtime Host")]
    [SerializeField] private GameplayRuntimeHost runtimeHost;

    // ── Phase3 References (discovered at runtime after additive load) ──
    private GameManager phase3GameManager;
    private ResultPanelController resultPanelController;

    // ── Runtime Mode ───────────────────────────────────────────
    [Header("Runtime")]
    [SerializeField] private RuntimeMode currentRuntimeMode = RuntimeMode.WorldMode;

    public RuntimeMode CurrentRuntimeMode => currentRuntimeMode;
    public bool IsPhase3Loaded => isPhase3Loaded;

    // ── Session ────────────────────────────────────────────────
    private GameplayModuleSession currentSession;
    private bool isPhase3Loaded;

    public GameplayModuleSession CurrentSession => currentSession;
    public bool HasActiveSession => currentSession != null && currentSession.IsActive;

    // ────────────────────────────────────────────────────────────
    // IGameplayProgressionAuthority
    // ────────────────────────────────────────────────────────────

    /// <summary>
    /// Phase6 bridge mode blocks Phase3's automatic GoToXxx() chain.
    /// Phase3 button actions should complete the current bridge session instead.
    /// </summary>
    public bool CanAutoAdvanceGameplay()
    {
        return false;
    }

    public void NotifyGameplayModuleCompleted(GameState completedState)
    {
        if (currentSession == null || !currentSession.IsActive)
        {
            Debug.LogWarning($"[GameplayBridgeManager] Ignoring completed {completedState}; no active session");
            return;
        }

        Debug.Log($"[GameplayBridgeManager] Module completed: {completedState} from {currentSession.SourceArea}");
        ExitGameplay();
    }

    // ────────────────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Phase6-side subscriptions (objects exist in base scene).
        if (testUIRouter != null)
        {
            testUIRouter.OnUIClosed.AddListener(OnUIClosedFromRouter);
        }

        // Hide gameplay UI on start (world mode is active).
        if (runtimeHost != null && runtimeHost.IsVisible)
        {
            runtimeHost.HideGameplayUI();
        }

        // Start additive load of Phase3 scene.
        // Phase3 references will be bound in OnPhase3SceneLoaded callback.
        SceneManager.sceneLoaded += OnPhase3SceneLoaded;
        SceneManager.LoadSceneAsync(phase3SceneName, LoadSceneMode.Additive);
        Debug.Log($"[GameplayBridgeManager] Loading Phase3 scene: {phase3SceneName}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnPhase3SceneLoaded;

        if (testUIRouter != null)
        {
            testUIRouter.OnUIClosed.RemoveListener(OnUIClosedFromRouter);
        }

        UnbindPhase3References();

        // Unload Phase3 scene if still loaded.
        if (isPhase3Loaded)
        {
            Scene phase3Scene = SceneManager.GetSceneByName(phase3SceneName);
            if (phase3Scene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(phase3SceneName);
            }
        }
    }

    // ────────────────────────────────────────────────────────────
    // Phase3 Additive Scene Loading
    // ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called when any scene is loaded additively. Filters for Phase3 scene
    /// and binds all Phase3 references, injects progression authority,
    /// and reparents Phase3 Canvas under GameplayCanvasRoot.
    /// </summary>
    private void OnPhase3SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != phase3SceneName || mode != LoadSceneMode.Additive)
        {
            return;
        }

        Debug.Log($"[GameplayBridgeManager] Phase3 scene loaded: {scene.name}");

        // Remove duplicate EventSystem FIRST (before Unity detects the conflict).
        // Phase6 base scene already has an EventSystem; Phase3's must be destroyed
        // immediately to avoid the "only one EventSystem" error.
        RemoveDuplicateEventSystems(scene);

        // Discover Phase3 components in the loaded scene.
        BindPhase3References(scene);

        // Inject progression authority — GameManager never references
        // Phase6 or Phase8 types, only the IGameplayProgressionAuthority interface.
        if (phase3GameManager != null)
        {
            phase3GameManager.SetProgressionAuthority(this);
            Debug.Log("[GameplayBridgeManager] Progression authority injected");
        }

        // Subscribe to Phase3's exit gameplay event.
        if (resultPanelController != null)
        {
            resultPanelController.OnExitGameplayEvent.AddListener(OnGameplayExitRequested);
            Debug.Log("[GameplayBridgeManager] Exit event subscribed");
        }

        // Reparent Phase3 Canvas under GameplayCanvasRoot so RuntimeHost
        // can control visibility of the entire gameplay UI layer.
        ReparentPhase3Canvas(scene);

        // Phase6 owns the world camera. Any camera brought by Phase3 is only
        // valid for standalone Phase3 and must not render in bridge mode.
        DisablePhase3Cameras(scene);

        // Bind GameplayCanvasGroup panel references from GameManager's Inspector bindings.
        BindCanvasGroupReferences();

        isPhase3Loaded = true;
        Debug.Log("[GameplayBridgeManager] Phase3 integration complete");
    }

    /// <summary>
    /// Finds Phase3 GameManager and ResultPanelController in the loaded scene.
    /// </summary>
    private void BindPhase3References(Scene scene)
    {
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            var gm = rootObj.GetComponentInChildren<GameManager>();
            if (gm != null)
            {
                phase3GameManager = gm;
            }

            var rpc = rootObj.GetComponentInChildren<ResultPanelController>();
            if (rpc != null)
            {
                resultPanelController = rpc;
            }
        }

        if (phase3GameManager == null)
        {
            Debug.LogError("[GameplayBridgeManager] GameManager not found in Phase3 scene");
        }

        if (resultPanelController == null)
        {
            Debug.LogError("[GameplayBridgeManager] ResultPanelController not found in Phase3 scene");
        }
    }

    /// <summary>
    /// Unsubscribes Phase3 events and clears progression authority.
    /// </summary>
    private void UnbindPhase3References()
    {
        if (resultPanelController != null)
        {
            resultPanelController.OnExitGameplayEvent.RemoveListener(OnGameplayExitRequested);
        }

        if (phase3GameManager != null)
        {
            phase3GameManager.SetProgressionAuthority(null);
        }

        phase3GameManager = null;
        resultPanelController = null;
        isPhase3Loaded = false;
    }

    /// <summary>
    /// Moves Phase3's Canvas GameObject under GameplayCanvasRoot
    /// so that RuntimeHost.ShowGameplayUI()/HideGameplayUI() controls
    /// the entire gameplay UI layer as a single root.
    /// Searches recursively since Canvas may not be a root object
    /// in the Phase3 scene (e.g. it could be a child of GameManager).
    /// </summary>
    private void ReparentPhase3Canvas(Scene scene)
    {
        if (runtimeHost == null) return;

        GameObject canvasRoot = runtimeHost.CanvasRoot;
        if (canvasRoot == null) return;

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            // Canvas might be a root object OR a descendant — search both.
            var canvas = rootObj.GetComponent<UnityEngine.Canvas>();
            if (canvas == null)
            {
                canvas = rootObj.GetComponentInChildren<UnityEngine.Canvas>();
            }

            if (canvas != null)
            {
                canvas.transform.SetParent(canvasRoot.transform, false);
                ConfigurePhase3Canvas(canvas);
                ConfigurePhase3BridgeLayout(canvas);
                Debug.Log($"[GameplayBridgeManager] Phase3 Canvas '{canvas.gameObject.name}' reparented under GameplayCanvasRoot");
                return;
            }
        }

        Debug.LogWarning("[GameplayBridgeManager] No Canvas found in Phase3 scene to reparent");
    }

    private void ConfigurePhase3Canvas(UnityEngine.Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 200;

        var canvasScaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        Debug.Log($"[GameplayBridgeManager] Phase3 Canvas configured: renderMode={canvas.renderMode}, sortingOrder={canvas.sortingOrder}");
    }

    private void ConfigurePhase3BridgeLayout(UnityEngine.Canvas canvas)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.pivot = new Vector2(0.5f, 0.5f);
            canvasRect.anchoredPosition = Vector2.zero;
            canvasRect.sizeDelta = Vector2.zero;
            canvasRect.localScale = Vector3.one;
        }

        RectTransform[] rects = canvas.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rect in rects)
        {
            GameObject target = rect.gameObject;
            if (target.name == "Text_Title" || target.name == "Panel_Debug")
            {
                target.SetActive(false);
                Debug.Log($"[GameplayBridgeManager] Hidden bridge-only UI object '{target.name}'");
                continue;
            }

            if (IsGameplayPanel(target.name))
            {
                CenterGameplayPanel(rect);
            }
        }
    }

    private static bool IsGameplayPanel(string objectName)
    {
        return objectName == "Panel_Order"
            || objectName == "Panel_Shape"
            || objectName == "Panel_Glaze"
            || objectName == "Panel_Firing"
            || objectName == "Panel_Result";
    }

    private static void CenterGameplayPanel(RectTransform panelRect)
    {
        if (panelRect.name == "Panel_Firing")
        {
            FiringPanelController firingPanel = panelRect.GetComponent<FiringPanelController>();
            if (firingPanel != null)
            {
                firingPanel.CenterBridgeWindowOnly();
                return;
            }
        }

        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.localScale = Vector3.one;
	}

    private void DisablePhase3Cameras(Scene scene)
    {
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            var cameras = rootObj.GetComponentsInChildren<Camera>(true);
            foreach (var phase3Camera in cameras)
            {
                phase3Camera.enabled = false;
                Debug.Log($"[GameplayBridgeManager] Disabled Phase3 camera '{phase3Camera.gameObject.name}'");
            }
        }
    }

    /// <summary>
    /// Removes duplicate EventSystems from Phase3 scene.
    /// Phase6's base scene already has one; the additive-loaded Phase3 scene
    /// may bring its own, causing "only one EventSystem" errors.
    /// </summary>
    private void RemoveDuplicateEventSystems(Scene scene)
    {
        // Find any EventSystem in Phase3 scene and destroy it.
        // The base scene (Workshop_TestScene) already has one.
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            var eventSystems = rootObj.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>();
            foreach (var es in eventSystems)
            {
                Debug.Log($"[GameplayBridgeManager] Removing duplicate EventSystem from '{es.gameObject.name}'");
                DestroyImmediate(es.gameObject);
            }
        }
    }

    /// <summary>
    /// Reads the 5 panel GameObject references from GameManager's private SerializeFields
    /// and assigns them to GameplayCanvasGroup's corresponding fields.
    /// GameManager already has these bound in the Phase3 scene Inspector,
    /// so we reuse those bindings rather than searching by name.
    /// Uses reflection for both reads and writes since both sets of fields are private.
    /// </summary>
    private void BindCanvasGroupReferences()
    {
        if (phase3GameManager == null || runtimeHost == null || runtimeHost.CanvasGroup == null) return;

        GameplayCanvasGroup group = runtimeHost.CanvasGroup;
        var gmType = phase3GameManager.GetType();
        var cgType = group.GetType();

        string[] panelFieldNames = { "panelOrder", "panelShape", "panelGlaze", "panelFiring", "panelResult" };

        foreach (string fieldName in panelFieldNames)
        {
            var gmField = gmType.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cgField = cgType.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (gmField != null && cgField != null)
            {
                GameObject panel = gmField.GetValue(phase3GameManager) as GameObject;
                if (panel != null)
                {
                    cgField.SetValue(group, panel);
                    Debug.Log($"[GameplayBridgeManager] Bound {fieldName} = {panel.name}");
                }
                else
                {
                    Debug.LogWarning($"[GameplayBridgeManager] GameManager.{fieldName} is null — panel not bound in Phase3 Inspector");
                }
            }
            else
            {
                Debug.LogWarning($"[GameplayBridgeManager] Field '{fieldName}' not found on GameManager or GameplayCanvasGroup");
            }
        }
    }

    // ────────────────────────────────────────────────────────────
    // IInteractionEntryHandler — called by Workstation.Interact()
    // ────────────────────────────────────────────────────────────

    public void OnWorkstationInteracted(AreaType areaType)
    {
        if (currentRuntimeMode != RuntimeMode.WorldMode)
        {
            Debug.LogWarning("[GameplayBridgeManager] Cannot interact while in GameplayMode");
            return;
        }

        if (currentSession != null && !currentSession.IsClosed)
        {
            Debug.LogWarning("[GameplayBridgeManager] Session already active, ignoring interaction");
            return;
        }

        if (!isPhase3Loaded)
        {
            Debug.LogWarning("[GameplayBridgeManager] Phase3 scene not loaded yet, ignoring interaction");
            return;
        }

        // Validate area type maps to a gameplay module
        if (!IsAreaTypeValid(areaType))
        {
            Debug.LogWarning($"[GameplayBridgeManager] AreaType {areaType} does not map to a gameplay module");
            return;
        }

        EnterGameplay(areaType);
    }

    /// <summary>
    /// Checks if the given AreaType maps to a Phase3 gameplay module.
    /// Valid: Order, Wheel, Glaze, Kiln.
    /// Invalid: None, Storage, Material (world-only areas, no gameplay module).
    /// </summary>
    private static bool IsAreaTypeValid(AreaType areaType)
    {
        switch (areaType)
        {
            case AreaType.Order:
            case AreaType.Wheel:
            case AreaType.Glaze:
            case AreaType.Kiln:
                return true;
            default:
                return false;
        }
    }

    // ────────────────────────────────────────────────────────────
    // Enter Gameplay
    // ────────────────────────────────────────────────────────────

    private void EnterGameplay(AreaType areaType)
    {
        // Step 1 — Validate all preconditions
        if (!ValidateEnter(areaType, out string failReason))
        {
            Debug.LogWarning($"[GameplayBridgeManager] Enter blocked: {failReason}");
            return;
        }

        // Step 2 — Create pending session
        currentSession = new GameplayModuleSession(areaType);
        Debug.Log($"[GameplayBridgeManager] Session {currentSession.SessionId} created for {areaType}");

        // Step 3 — Show gameplay UI via RuntimeHost
        if (runtimeHost != null)
        {
            runtimeHost.ShowGameplayUI();
        }

        // Step 4 — Start the workstation-specific Phase3 module
        if (phase3GameManager != null)
        {
            EnsurePhase3AuthorityBound();
            EnterPhase3Module(areaType);
        }

        // Step 5 — Enter GameplayMode + Lock runtime (last, after everything succeeded)
        currentRuntimeMode = RuntimeMode.GameplayMode;
        phase6GameManager.SetState(Phase6GameState.UIOpen);
        if (playerCharacter != null)
        {
            playerCharacter.StopMoving();
        }

        // Step 6 — Commit session active
        currentSession.CommitActive();

        // Step 7 — Validate consistency
        Debug.Assert(GameplayModuleSession.IsConsistent(currentRuntimeMode, currentSession),
            $"[GameplayBridgeManager] Inconsistent state after enter: {currentRuntimeMode} + {currentSession.State}");

        Debug.Log($"[GameplayBridgeManager] Session {currentSession.SessionId} active, mode={currentRuntimeMode}");
    }

    private void EnsurePhase3AuthorityBound()
    {
        if (phase3GameManager == null)
        {
            return;
        }

        phase3GameManager.SetProgressionAuthority(this);
    }

    private void EnterPhase3Module(AreaType areaType)
    {
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
            default:
                Debug.LogError($"[GameplayBridgeManager] Unsupported gameplay area: {areaType}");
                break;
        }
    }

    // ────────────────────────────────────────────────────────────
    // Exit Gameplay
    // ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called when Phase3 ResultPanelController.onExitGameplay fires.
    /// </summary>
    public void OnGameplayExitRequested()
    {
        if (currentSession == null || !currentSession.IsActive)
        {
            Debug.LogWarning("[GameplayBridgeManager] No active session to exit");
            return;
        }

        ExitGameplay();
    }

    /// <summary>
    /// Called when TestUIPanel close button triggers TestUIRouter.OnUIClosed.
    /// Only acts if there is an active session (bridge-managed flow).
    /// </summary>
    private void OnUIClosedFromRouter()
    {
        if (currentSession == null || !currentSession.IsActive)
        {
            return;
        }

        ExitGameplay();
    }

    private void ExitGameplay()
    {
        // Step 0 — Validate exit preconditions
        if (!ValidateExit())
        {
            Debug.LogWarning("[GameplayBridgeManager] Exit validation failed, aborting exit");
            return;
        }

        // Step 1 — Begin exit
        currentSession.BeginExit();
        Debug.Log($"[GameplayBridgeManager] Session {currentSession.SessionId} exiting");

        // Step 2 — Stop gameplay
        if (phase3GameManager != null)
        {
            phase3GameManager.StopGameplayLoop();
        }

        // Step 3 — Hide gameplay UI via RuntimeHost
        if (runtimeHost != null)
        {
            runtimeHost.HideGameplayUI();
        }

        // Step 4 — Exit GameplayMode + Unlock runtime
        currentRuntimeMode = RuntimeMode.WorldMode;
        phase6GameManager.SetState(Phase6GameState.Playing);

        // Step 5 — Close and clear session
        currentSession.Close();
        currentSession = null;

        // Step 6 — Validate consistency
        Debug.Assert(GameplayModuleSession.IsConsistent(currentRuntimeMode, currentSession),
            $"[GameplayBridgeManager] Inconsistent state after exit: {currentRuntimeMode} + {(currentSession != null ? currentSession.State.ToString() : "null")}");

        Debug.Log($"[GameplayBridgeManager] Session closed, mode={currentRuntimeMode}, world control restored");
    }

    // ────────────────────────────────────────────────────────────
    // Validation
    // ────────────────────────────────────────────────────────────

    private bool ValidateEnter(AreaType areaType, out string failReason)
    {
        failReason = "";

        // Runtime mode must be WorldMode
        if (currentRuntimeMode != RuntimeMode.WorldMode)
        {
            failReason = $"Runtime mode is {currentRuntimeMode}, expected WorldMode";
            return false;
        }

        if (phase6GameManager == null)
        {
            failReason = "Phase6GameManager is null";
            return false;
        }

        if (!phase6GameManager.CanTransitionTo(Phase6GameState.UIOpen))
        {
            failReason = $"Cannot transition to UIOpen from {phase6GameManager.CurrentState}";
            return false;
        }

        if (runtimeHost == null)
        {
            failReason = "GameplayRuntimeHost is null";
            return false;
        }

        if (!runtimeHost.ValidateSetup())
        {
            failReason = "GameplayRuntimeHost setup incomplete";
            return false;
        }

        // Area type must map to a valid gameplay module
        if (!IsAreaTypeValid(areaType))
        {
            failReason = $"AreaType {areaType} does not map to a gameplay module";
            return false;
        }

        if (playerCharacter == null)
        {
            failReason = "PlayerCharacter is null";
            return false;
        }

        // Session must be free (no active or pending session)
        if (currentSession != null && !currentSession.IsClosed)
        {
            failReason = $"Session {currentSession.SessionId} still active";
            return false;
        }

        // Validate current consistency
        if (!GameplayModuleSession.IsConsistent(currentRuntimeMode, currentSession))
        {
            failReason = $"Inconsistent state: {currentRuntimeMode} + {(currentSession != null ? currentSession.State.ToString() : "null")}";
            return false;
        }

        return true;
    }

    private bool ValidateExit()
    {
        // Must be in GameplayMode to exit
        if (currentRuntimeMode != RuntimeMode.GameplayMode)
        {
            Debug.LogWarning($"[GameplayBridgeManager] Cannot exit from {currentRuntimeMode}");
            return false;
        }

        // Must have an active or pending session
        if (currentSession == null || currentSession.IsClosed)
        {
            Debug.LogWarning($"[GameplayBridgeManager] No session to exit (state={currentSession?.State.ToString() ?? "null"})");
            return false;
        }

        // Block duplicate exit if already exiting
        if (currentSession.IsExiting)
        {
            Debug.LogWarning("[GameplayBridgeManager] Exit already in progress");
            return false;
        }

        return true;
    }

    // ────────────────────────────────────────────────────────────
    // Abort (emergency cleanup)
    // ────────────────────────────────────────────────────────────

    /// <summary>
    /// Emergency abort — restores world control unconditionally.
    /// Only use for edge cases (e.g. scene unload while gameplay active).
    /// </summary>
    public void AbortGameplay()
    {
        if (currentSession == null || currentSession.IsClosed)
        {
            return;
        }

        Debug.LogWarning($"[GameplayBridgeManager] Aborting session {currentSession.SessionId}");

        if (phase3GameManager != null && currentSession.IsActive)
        {
            phase3GameManager.StopGameplayLoop();
        }

        if (runtimeHost != null && runtimeHost.IsVisible)
        {
            runtimeHost.HideGameplayUI();
        }

        currentRuntimeMode = RuntimeMode.WorldMode;
        phase6GameManager.SetState(Phase6GameState.Playing);
        currentSession.Close();
        currentSession = null;

        Debug.Log($"[GameplayBridgeManager] Aborted, mode={currentRuntimeMode}");
    }
}
