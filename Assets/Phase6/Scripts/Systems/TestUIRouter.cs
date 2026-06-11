using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TestUIRouter : MonoBehaviour, IInteractionEntryHandler
{
    [System.Serializable]
    public class UIMapping
    {
        public AreaType areaType;
        public GameObject uiPanel;
    }

    [Header("UI Mappings")]
    [SerializeField] private List<UIMapping> uiMappings = new List<UIMapping>();

    [Header("Events")]
    public UnityEvent OnUIClosed = new UnityEvent();

    private Dictionary<AreaType, GameObject> uiMap;
    private GameObject currentOpenUI;

    public bool IsUIOpen => currentOpenUI != null;

    private void Awake()
    {
        uiMap = new Dictionary<AreaType, GameObject>();
        foreach (var mapping in uiMappings)
        {
            uiMap[mapping.areaType] = mapping.uiPanel;
            if (mapping.uiPanel != null)
            {
                TestUIPanel panel = mapping.uiPanel.GetComponent<TestUIPanel>();
                if (panel != null)
                {
                    panel.Initialize(this, $"{mapping.areaType} UI");
                }

                mapping.uiPanel.SetActive(false);
            }
        }
    }

    private void Start()
    {
        // Delegate Workstation initialization to GameplayBridgeManager if present,
        // otherwise fall back to self-injection for standalone Phase6 testing.
        GameplayBridgeManager bridge = FindObjectOfType<GameplayBridgeManager>();
        IInteractionEntryHandler handler = bridge != null ? (IInteractionEntryHandler)bridge : null;

        Workstation[] workstations = FindObjectsOfType<Workstation>();
        foreach (Workstation ws in workstations)
        {
            ws.Initialize(handler ?? this);
        }
    }

    /// <summary>
    /// Pure UI operation — activates the mapped panel.
    /// Runtime state switching (SetState, StopMoving) is NOT done here;
    /// GameplayBridgeManager handles runtime transitions.
    /// </summary>
    public void OpenUI(AreaType areaType)
    {
        if (currentOpenUI != null) return;

        if (!uiMap.TryGetValue(areaType, out GameObject panel) || panel == null)
        {
            Debug.LogError($"[TestUIRouter] No UI mapped for {areaType}");
            return;
        }

        currentOpenUI = panel;
        panel.SetActive(true);

        Debug.Log($"[UIRoute] {areaType} -> {panel.name}");
    }

    /// <summary>
    /// Pure UI operation — deactivates the current panel.
    /// Runtime state restoration (SetState) is NOT done here;
    /// GameplayBridgeManager handles runtime transitions.
    /// Fires OnUIClosed event so Bridge can react.
    /// </summary>
    public void CloseUI()
    {
        string closedName = currentOpenUI != null ? currentOpenUI.name : "none";

        if (currentOpenUI != null)
        {
            currentOpenUI.SetActive(false);
            currentOpenUI = null;
        }

        OnUIClosed.Invoke();

        Debug.Log($"[UIRoute] Close -> {closedName}");
    }

    /// <summary>
    /// Check if a UI panel is mapped for the given area type.
    /// </summary>
    public bool HasMapping(AreaType areaType)
    {
        return uiMap != null && uiMap.ContainsKey(areaType) && uiMap[areaType] != null;
    }

    // ── IInteractionEntryHandler fallback for standalone Phase6 testing ──────
    // When no GameplayBridgeManager exists in the scene, TestUIRouter acts as
    // the interaction handler so Phase6 can still function independently.

    public void OnWorkstationInteracted(AreaType areaType)
    {
        // Standalone fallback: just open the UI panel.
        // No runtime state changes — in standalone mode there is no bridge.
        OpenUI(areaType);
    }
}
