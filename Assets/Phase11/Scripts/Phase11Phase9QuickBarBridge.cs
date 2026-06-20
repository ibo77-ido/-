using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase11Phase9QuickBarBridge : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private HUDQuickBarConfigSO config;

    [Header("Button Container")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Phase9 Targets")]
    [SerializeField] private string interactionRootName = "Interaction";
    [SerializeField] private float arrivalDistance = 0.18f;
    [SerializeField] private float targetSampleDistance = 1.2f;
    [SerializeField] private float navigableArrivalDistance = 0.2f;
    [SerializeField] private float autoInteractTimeout = 15f;
    [SerializeField] private bool autoInteractAfterArrival;

    private readonly Dictionary<AreaType, string> targetNames = new Dictionary<AreaType, string>
    {
        { AreaType.Order, "Order-interact" },
        { AreaType.Wheel, "Shape-interact" },
        { AreaType.Glaze, "Glaze-interact" },
        { AreaType.Kiln, "Kiln-interact" },
        { AreaType.Storage, "Storage-interact" },
        { AreaType.Material, "Material-interact" }
    };

    private PlayerCharacter playerCharacter;
    private MovementController movementController;
    private Phase9InteractionBridge phase9Bridge;
    private AreaType? pendingArea;
    private Vector3 pendingMoveDestination;
    private bool hasPendingMoveDestination;
    private float pendingStartTime;

    private void Awake()
    {
        DisableLegacyQuickBar();
    }

    private void Start()
    {
        ResolveRuntimeReferences();
        BuildButtons();
    }

    private void Update()
    {
        if (pendingArea == null)
        {
            return;
        }

        ResolveRuntimeReferences();

        if (Time.time - pendingStartTime > autoInteractTimeout)
        {
            pendingArea = null;
            hasPendingMoveDestination = false;
            return;
        }

        if (playerCharacter == null)
        {
            pendingArea = null;
            hasPendingMoveDestination = false;
            return;
        }

        Transform target = FindTarget(pendingArea.Value);
        if (target == null)
        {
            pendingArea = null;
            hasPendingMoveDestination = false;
            return;
        }

        Vector3 arrivalTarget = hasPendingMoveDestination ? pendingMoveDestination : target.position;
        float requiredDistance = hasPendingMoveDestination
            ? Mathf.Max(arrivalDistance, navigableArrivalDistance)
            : arrivalDistance;

        if (Vector3.Distance(playerCharacter.transform.position, arrivalTarget) > requiredDistance)
        {
            return;
        }

        AreaType arrivedArea = pendingArea.Value;
        pendingArea = null;
        hasPendingMoveDestination = false;

        if (autoInteractAfterArrival && ShouldAutoInteract(arrivedArea))
        {
            TryInteractAtTarget();
        }
    }

    private void DisableLegacyQuickBar()
    {
        HUDQuickBar legacyQuickBar = GetComponent<HUDQuickBar>();
        if (legacyQuickBar != null)
        {
            legacyQuickBar.enabled = false;
        }
    }

    private void ResolveRuntimeReferences()
    {
        if (playerCharacter == null)
        {
            playerCharacter = FindObjectOfType<PlayerCharacter>();
        }

        if (movementController == null && playerCharacter != null)
        {
            movementController = playerCharacter.GetComponent<MovementController>();
        }

        if (phase9Bridge == null)
        {
            phase9Bridge = FindObjectOfType<Phase9InteractionBridge>();
        }
    }

    private void BuildButtons()
    {
        if (config == null || config.buttons == null || buttonContainer == null || buttonPrefab == null)
        {
            return;
        }

        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < config.buttons.Length; i++)
        {
            HUDButtonData data = config.buttons[i];
            GameObject go = Instantiate(buttonPrefab, buttonContainer);
            QuickBarButton button = go.GetComponent<QuickBarButton>();
            if (button == null)
            {
                button = go.AddComponent<QuickBarButton>();
            }

            button.Initialize(data, HandleButtonClicked);
        }
    }

    private void HandleButtonClicked(AreaType area)
    {
        ResolveRuntimeReferences();

        if (playerCharacter == null)
        {
            return;
        }

        Transform target = FindTarget(area);
        if (target == null)
        {
            Debug.LogWarning("[Phase11Phase9QuickBarBridge] Missing Phase9 interaction target for " + area);
            return;
        }

        Vector3 navigationTarget;
        if (!TryResolveNavigationTarget(target, out navigationTarget))
        {
            Debug.LogWarning("[Phase11Phase9QuickBarBridge] Could not resolve a NavMesh position near " + target.name);
            return;
        }

        pendingArea = area;
        pendingMoveDestination = navigationTarget;
        hasPendingMoveDestination = true;
        pendingStartTime = Time.time;
        playerCharacter.SetDestination(navigationTarget);
    }

    private Transform FindTarget(AreaType area)
    {
        string targetName;
        if (!targetNames.TryGetValue(area, out targetName))
        {
            return null;
        }

        GameObject rootObject = GameObject.Find(interactionRootName);
        if (rootObject == null)
        {
            return null;
        }

        Transform target = rootObject.transform.Find(targetName);
        return target;
    }

    private bool ShouldAutoInteract(AreaType area)
    {
        if (config == null || config.buttons == null)
        {
            return false;
        }

        for (int i = 0; i < config.buttons.Length; i++)
        {
            HUDButtonData data = config.buttons[i];
            if (data.areaType == area)
            {
                return data.autoInteract;
            }
        }

        return false;
    }

    private bool TryResolveNavigationTarget(Transform target, out Vector3 navigationTarget)
    {
        navigationTarget = Vector3.zero;
        if (target == null)
        {
            return false;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target.position, out hit, targetSampleDistance, NavMesh.AllAreas))
        {
            navigationTarget = hit.position;
            return true;
        }

        return false;
    }

    private void TryInteractAtTarget()
    {
        if (phase9Bridge != null)
        {
            phase9Bridge.TryEnterNearestGameplayModule();
        }
    }
}
