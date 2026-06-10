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

    public void StopGameplayLoop()
    {
        Debug.Log("[GameManager] StopGameplayLoop");
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
        if (orderManager != null) orderManager.GetCurrentOrder();
        if (shapeSystem != null) shapeSystem.LoadTargetFromCurrentOrder();
        SetState(GameState.Shape);
    }

    public void GoToGlaze()
    {
        if (glazeSystem != null) glazeSystem.LoadTargetFromCurrentOrder();
        SetState(GameState.Glaze);
    }

    public void GoToFiring()
    {
        if (firingSystem != null) firingSystem.StartFiring();
        SetState(GameState.Firing);
    }

    public void GoToResult()
    {
        if (resultPanelController != null) resultPanelController.ShowResult();
        advanceOrderOnNextStart = true;
        SetState(GameState.Result);
    }

    public void GoToNextOrder()
    {
        BeginOrder(true);
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
