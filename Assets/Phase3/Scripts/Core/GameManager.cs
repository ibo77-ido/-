using UnityEngine;

public enum GameState
{
    Order,
    Shape,
    Glaze,
    Firing,
    Result
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameState currentState = GameState.Order;

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

    public GameState CurrentState => currentState;

    private void Start()
    {
        SetState(GameState.Order);
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
        SetState(GameState.Result);
    }

    public void GoToNextOrder()
    {
        if (orderManager != null) orderManager.NextOrder();
        ResetAllSystems();
        SetState(GameState.Order);
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
