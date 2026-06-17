using UnityEngine;

public class HUDQuickBar : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private HUDQuickBarConfigSO config;

    [Header("Button Container")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    // Runtime references (auto-resolve via FindObjectOfType)
    private Phase6GameManager gameManager;
    private PlayerCharacter playerCharacter;
    private MovementController movementController;
    private InteractionController interactionController;

    // Pending auto-interact state
    private AreaType? pendingAutoInteract;
    private float pendingStartTime;
    private float autoInteractTimeout;

    private QuickBarButton[] runtimeButtons;
    private HUDButtonData[] buttonDataCache;

    private void Start()
    {
        gameManager = FindObjectOfType<Phase6GameManager>();
        var pc = FindObjectOfType<PlayerCharacter>();
        if (pc != null)
        {
            playerCharacter = pc;
            movementController = pc.GetComponent<MovementController>();
            interactionController = pc.GetComponent<InteractionController>();
        }

        autoInteractTimeout = config != null ? config.autoInteractTimeout : 15f;
        BuildButtons();
    }

    private void BuildButtons()
    {
        if (config == null || config.buttons == null) return;

        buttonDataCache = config.buttons;
        runtimeButtons = new QuickBarButton[buttonDataCache.Length];
        for (int i = 0; i < buttonDataCache.Length; i++)
        {
            var go = Instantiate(buttonPrefab, buttonContainer);
            var qbb = go.GetComponent<QuickBarButton>();
            if (qbb == null)
            {
                qbb = go.AddComponent<QuickBarButton>();
            }
            qbb.Initialize(buttonDataCache[i], OnButtonClicked);
            runtimeButtons[i] = qbb;
        }
    }

    private void Update()
    {
        if (pendingAutoInteract == null) return;

        // Timeout protection
        if (Time.time - pendingStartTime > autoInteractTimeout)
        {
            CancelPending("timeout: cannot reach target workstation");
            return;
        }

        if (movementController == null) return;

        // Interrupted: player clicked ground mid-navigation
        if (!movementController.IsMoving() && !movementController.HasReachedDestination())
        {
            CancelPending(null);
            return;
        }

        // Arrived
        if (movementController.HasReachedDestination())
        {
            AreaType arrived = pendingAutoInteract.Value;
            pendingAutoInteract = null;

            if (ShouldAutoInteract(arrived) && interactionController != null)
            {
                interactionController.TryInteract();
            }
        }
    }

    private bool ShouldAutoInteract(AreaType areaType)
    {
        if (buttonDataCache == null) return false;
        foreach (var data in buttonDataCache)
        {
            if (data.areaType == areaType)
                return data.autoInteract;
        }
        return false;
    }

    private void OnButtonClicked(AreaType target)
    {
        // State guard
        if (gameManager != null && !gameManager.CanMove())
            return;

        if (playerCharacter == null) return;

        // Find target workstation
        var workstations = FindObjectsOfType<Workstation>();
        Workstation targetWS = null;
        foreach (var ws in workstations)
        {
            if (ws.AreaType == target)
            {
                targetWS = ws;
                break;
            }
        }

        if (targetWS == null) return;

        bool shouldInteract = ShouldAutoInteract(target);

        // Already in range + autoInteract → interact directly
        if (shouldInteract &&
            interactionController != null &&
            interactionController.IsInRange &&
            interactionController.NearestWorkstation?.AreaType == target)
        {
            interactionController.TryInteract();
            return;
        }

        // Cancel previous pending
        if (pendingAutoInteract != null)
            playerCharacter.StopMoving();

        // Mark pending (only for autoInteract targets)
        if (shouldInteract)
        {
            pendingAutoInteract = target;
            pendingStartTime = Time.time;
        }

        // Navigation target: prefer InteractionPoint, fallback to workstation position
        Vector3 dest;
        if (targetWS.InteractionPoint != null)
            dest = targetWS.InteractionPoint.transform.position;
        else
            dest = targetWS.transform.position;

        playerCharacter.SetDestination(dest);
    }

    private void CancelPending(string reason)
    {
        if (playerCharacter != null)
            playerCharacter.StopMoving();
        pendingAutoInteract = null;

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.LogWarning($"[HUDQuickBar] {reason}");
        }
    }
}
