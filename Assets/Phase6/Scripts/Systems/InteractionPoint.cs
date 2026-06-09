using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private AreaType areaType = AreaType.Order;
    [SerializeField] private float interactionDistance = 1.5f;

    public AreaType AreaType => areaType;
    public float InteractionDistance => interactionDistance;

    private TestUIRouter uiRouter;

    public void Initialize(TestUIRouter router)
    {
        uiRouter = router;
    }

    public TestUIRouter GetUIRouter()
    {
        return uiRouter;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}