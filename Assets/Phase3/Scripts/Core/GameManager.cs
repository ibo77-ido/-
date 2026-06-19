using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    None,
    Order,
    Shape,
    Glaze,
    Firing,
    Result
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameState currentState = GameState.None;
    [SerializeField] private bool autoStartInPhase3Scene = true;

    [SerializeField] private GameObject panelOrder;
    [SerializeField] private GameObject panelShape;
    [SerializeField] private GameObject panelGlaze;
    [SerializeField] private GameObject panelFiring;
    [SerializeField] private GameObject panelResult;

    [SerializeField] private OrderManager orderManager;
    [SerializeField] private ShapeSystem shapeSystem;
    [SerializeField] private GlazeSystem glazeSystem;
    [SerializeField] private FiringSystem firingSystem;
    [SerializeField] private ResultSystem resultSystem;

    [SerializeField] private ShapePanelController shapePanelController;
    [SerializeField] private GlazePanelController glazePanelController;
    [SerializeField] private FiringPanelController firingPanelController;
    [SerializeField] private ResultPanelController resultPanelController;
    [SerializeField] private OrderPanelController orderPanelController;

    public GameState CurrentState => currentState;

    private bool advanceOrderOnNextStart;

    /// <summary>
    /// Queries Bridge Runtime Authority for auto-advance permission.
    /// Returns true when either:
    /// - No bridge is present (standalone Phase3 mode), or
    /// - Bridge confirms automatic gameplay progression is allowed.
    /// GameManager does NOT hold permission state — only queries it.
    /// </summary>
    private bool CanProgress()
    {
        if (progressionAuthority != null)
        {
            return progressionAuthority.CanAutoAdvanceGameplay();
        }

        // No authority bound — standalone Phase3, always allow
        return true;
    }

    private void CompleteModuleForBridge(GameState completedState)
    {
        if (progressionAuthority != null)
        {
            progressionAuthority.NotifyGameplayModuleCompleted(completedState);
        }
    }

    /// <summary>
    /// Binds the progression authority at runtime.
    /// Called by GameplayBridgeManager during setup.
    /// GameManager never references Bridge types directly.
    /// </summary>
    public void SetProgressionAuthority(IGameplayProgressionAuthority authority)
    {
        progressionAuthority = authority;
    }

    private IGameplayProgressionAuthority progressionAuthority;

    private void Start()
    {
        if (currentState == GameState.None && ShouldAutoStart())
        {
            BeginOrder(false);
            return;
        }

        UpdatePanels();
    }

    public void StartGameplayLoop()
    {
        Debug.Log("[GameManager] StartGameplayLoop");
        BeginOrder(advanceOrderOnNextStart);
    }

    public void EnterOrderModule()
    {
        Debug.Log("[GameManager] EnterOrderModule");
        BeginOrder(false);
    }

    public void EnterShapeModule()
    {
        Debug.Log("[GameManager] EnterShapeModule");
        if (orderManager != null) orderManager.GetCurrentOrder();
        if (shapeSystem != null) shapeSystem.LoadTargetFromCurrentOrder();
        if (shapePanelController != null) shapePanelController.ResetPanel();
        SetState(GameState.Shape);
    }

    public void EnterGlazeModule()
    {
        Debug.Log("[GameManager] EnterGlazeModule");
        if (glazeSystem != null) glazeSystem.LoadTargetFromCurrentOrder();
        if (glazePanelController != null) glazePanelController.ResetPanel();
        SetState(GameState.Glaze);
    }

    public void EnterFiringModule()
    {
        Debug.Log("[GameManager] EnterFiringModule");
        if (firingSystem != null) firingSystem.ResetFiring();
        SetState(GameState.Firing);
    }

    public void StopGameplayLoop()
    {
        Debug.Log("[GameManager] StopGameplayLoop");
        advanceOrderOnNextStart = false;
        firingSystem.StopFiring();
        SetState(GameState.None);
    }

    public void SetState(GameState newState)
    {
        Debug.Log($"[GameManager] State: {currentState} -> {newState}");
        currentState = newState;
        UpdatePanels();
    }

    public void GoToShape()
    {
        if (!CanProgress())
        {
            CompleteModuleForBridge(GameState.Order);
            return;
        }

        if (orderManager != null) orderManager.GetCurrentOrder();
        if (shapeSystem != null) shapeSystem.LoadTargetFromCurrentOrder();
        SetState(GameState.Shape);
    }

    public void GoToGlaze()
    {
        if (!CanProgress())
        {
            CompleteModuleForBridge(GameState.Shape);
            return;
        }

        if (glazeSystem != null) glazeSystem.LoadTargetFromCurrentOrder();
        SetState(GameState.Glaze);
    }

    public void GoToFiring()
    {
        if (!CanProgress())
        {
            CompleteModuleForBridge(GameState.Glaze);
            return;
        }

        if (firingSystem != null) firingSystem.ResetFiring();
        SetState(GameState.Firing);
    }

    public void GoToResult()
    {
        if (!CanProgress())
        {
            CompleteModuleForBridge(GameState.Firing);
            return;
        }

        if (resultPanelController != null) resultPanelController.ShowResult();
        advanceOrderOnNextStart = true;
        SetState(GameState.Result);
    }

    public void GoToNextOrder()
    {
        if (!CanProgress())
        {
            CompleteModuleForBridge(GameState.Result);
            return;
        }

        BeginOrder(true);
    }

    public void AdvanceOrderForBridge()
    {
        if (orderManager != null)
        {
            orderManager.NextOrder();
        }

        advanceOrderOnNextStart = false;
        StopGameplayLoop();
    }

    private void BeginOrder(bool advanceOrder)
    {
        OrderData currentOrder = null;

        if (orderManager != null)
        {
            if (advanceOrder)
            {
                orderManager.NextOrder();
            }
            else
            {
                orderManager.GetCurrentOrder();
            }

            currentOrder = orderManager.CurrentOrder;
        }

        advanceOrderOnNextStart = false;
        ResetAllSystems();
        if (orderPanelController != null)
        {
            orderPanelController.ShowOrder(currentOrder);
        }
        SetState(GameState.Order);
    }

    private bool ShouldAutoStart()
    {
        if (!autoStartInPhase3Scene)
        {
            return false;
        }

        string scenePath = SceneManager.GetActiveScene().path.Replace('\\', '/');
        return scenePath.Contains("/Phase3/");
    }

    private void ResetAllSystems()
    {
        if (firingSystem != null) firingSystem.ResetFiring();
        if (shapeSystem != null) shapeSystem.LoadTargetFromCurrentOrder();
        if (glazeSystem != null) glazeSystem.LoadTargetFromCurrentOrder();
        if (shapePanelController != null) shapePanelController.ResetPanel();
        if (glazePanelController != null) glazePanelController.ResetPanel();
        if (firingPanelController != null) firingPanelController.ResetPanel();
    }

    private void UpdatePanels()
    {
        SetActive(panelOrder, currentState == GameState.Order);
        SetActive(panelShape, currentState == GameState.Shape);
        SetActive(panelGlaze, currentState == GameState.Glaze);
        SetActive(panelFiring, currentState == GameState.Firing);
        SetActive(panelResult, currentState == GameState.Result);
    }

    private static void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }
}
