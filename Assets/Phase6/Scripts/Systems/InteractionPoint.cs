using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private AreaType areaType = AreaType.Order;
    [SerializeField] private float interactionDistance = 1.5f;

    public AreaType AreaType => areaType;
    public float InteractionDistance => interactionDistance;

    private IInteractionEntryHandler handler;

    public void Initialize(IInteractionEntryHandler entryHandler)
    {
        handler = entryHandler;
    }

    public IInteractionEntryHandler GetHandler()
    {
        return handler;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}