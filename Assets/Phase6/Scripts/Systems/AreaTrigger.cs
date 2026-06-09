using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private AreaConfigSO config;

    private AreaManager manager;

    public AreaType AreaType => config != null ? config.areaType : AreaType.Order;
    public string AreaName => config != null ? config.areaName : "Unknown";

    private void Awake()
    {
        manager = FindObjectOfType<AreaManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ICharacter>() == null) return;
        if (manager != null)
        {
            manager.OnPlayerEnterArea(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ICharacter>() == null) return;
        if (manager != null)
        {
            manager.OnPlayerExitArea(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
        Gizmos.DrawWireCube(config.boundsCenter + transform.position, config.boundsSize);
    }
}