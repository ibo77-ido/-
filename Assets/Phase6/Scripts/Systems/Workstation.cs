using UnityEngine;

public class Workstation : MonoBehaviour, IInteractable
{
    [Header("Config")]
    [SerializeField] private WorkstationConfigSO config;

    [Header("Structure")]
    [SerializeField] private Transform logicRoot;
    [SerializeField] private Transform artRoot;
    [SerializeField] private InteractionPoint interactionPoint;
    [SerializeField] private WorkstationVisualController visualController;

    public AreaType AreaType => config != null ? config.areaType : AreaType.Order;
    public WorkstationConfigSO Config => config;
    public InteractionPoint InteractionPoint => interactionPoint;
    public WorkstationVisualController VisualController => visualController;

    private TestUIRouter uiRouter;

    public void Initialize(TestUIRouter router)
    {
        uiRouter = router;
        if (interactionPoint != null)
        {
            interactionPoint.Initialize(router);
        }
    }

    public void Interact(ICharacter actor)
    {
        Debug.Log($"[Workstation] Interact: {config?.stationName ?? name} ({AreaType})");
        if (uiRouter != null)
        {
            uiRouter.OpenUI(AreaType);
        }
    }

    public void RefreshVisual()
    {
        if (visualController != null)
        {
            visualController.RefreshVisual();
        }
    }

    public void ApplyScale()
    {
        if (visualController != null)
        {
            visualController.ApplyScale();
        }
    }
}