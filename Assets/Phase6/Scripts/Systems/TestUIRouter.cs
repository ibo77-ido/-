using UnityEngine;
using System.Collections.Generic;

public class TestUIRouter : MonoBehaviour
{
    [System.Serializable]
    public class UIMapping
    {
        public AreaType areaType;
        public GameObject uiPanel;
    }

    [Header("UI Mappings")]
    [SerializeField] private List<UIMapping> uiMappings = new List<UIMapping>();

    [Header("References")]
    [SerializeField] private Phase6GameManager gameManager;
    [SerializeField] private PlayerCharacter playerCharacter;

    private Dictionary<AreaType, GameObject> uiMap;
    private GameObject currentOpenUI;

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
        Workstation[] workstations = FindObjectsOfType<Workstation>();
        foreach (Workstation ws in workstations)
        {
            ws.Initialize(this);
        }
    }

    public void OpenUI(AreaType areaType)
    {
        if (currentOpenUI != null) return;

        if (gameManager == null)
        {
            Debug.LogError("[TestUIRouter] gameManager is null, cannot open UI");
            return;
        }

        if (playerCharacter == null)
        {
            Debug.LogError("[TestUIRouter] playerCharacter is null, cannot open UI");
            return;
        }

        gameManager.SetState(Phase6GameState.UIOpen);
        playerCharacter.StopMoving();

        if (!uiMap.TryGetValue(areaType, out GameObject panel) || panel == null)
        {
            Debug.LogError($"[TestUIRouter] No UI mapped for {areaType}");
            gameManager.SetState(Phase6GameState.Playing);
            return;
        }

        currentOpenUI = panel;
        panel.SetActive(true);

        Debug.Log($"[UIRoute] {areaType} -> {panel.name}");
    }

    public void CloseUI()
    {
        string closedName = currentOpenUI != null ? currentOpenUI.name : "none";

        if (currentOpenUI != null)
        {
            currentOpenUI.SetActive(false);
            currentOpenUI = null;
        }

        if (playerCharacter != null)
        {
            playerCharacter.StopMoving();
        }

        if (gameManager != null)
        {
            gameManager.SetState(Phase6GameState.Playing);
        }

        Debug.Log($"[UIRoute] Close -> {closedName}");
    }
}
