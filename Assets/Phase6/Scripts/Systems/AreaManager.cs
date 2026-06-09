using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AreaManager : MonoBehaviour
{
    [Header("Current Area")]
    [SerializeField] private AreaType currentArea = AreaType.None;

    private AreaTrigger currentAreaTrigger;
    private List<AreaTrigger> allAreas;

    public AreaType CurrentArea => currentArea;
    public AreaTrigger CurrentAreaTrigger => currentAreaTrigger;

    public System.Action<AreaType> OnAreaChanged;

    private void Start()
    {
        allAreas = FindObjectsOfType<AreaTrigger>().ToList();
        Debug.Log($"[AreaManager] Found {allAreas.Count} areas");
    }

    public void OnPlayerEnterArea(AreaTrigger trigger)
    {
        if (trigger == null) return;

        // Debounce: ignore rapid re-entry to same area
        if (currentAreaTrigger == trigger) return;

        currentAreaTrigger = trigger;
        currentArea = trigger.AreaType;

        Debug.Log($"[AreaManager] Entered: {trigger.AreaName} ({trigger.AreaType})");
        OnAreaChanged?.Invoke(currentArea);
    }

    public void OnPlayerExitArea(AreaTrigger trigger)
    {
        if (trigger == null || currentAreaTrigger != trigger) return;

        currentAreaTrigger = null;
        currentArea = AreaType.None;

        Debug.Log($"[AreaManager] Exited: {trigger.AreaName}");
        OnAreaChanged?.Invoke(currentArea);
    }

    public string GetCurrentAreaName()
    {
        return currentAreaTrigger != null ? currentAreaTrigger.AreaName : "None";
    }
}
