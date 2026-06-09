using UnityEngine;
using System.Collections.Generic;

public class InteractionController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float detectionRange = 5f;

    private ICharacter character;
    private Phase6GameManager gameManager;
    private List<Workstation> allWorkstations;
    private Workstation nearestWorkstation;
    private bool isInRange;

    public Workstation NearestWorkstation => nearestWorkstation;
    public bool IsInRange => isInRange;

    private void Awake()
    {
        character = GetComponent<ICharacter>();
    }

    private void Start()
    {
        gameManager = FindObjectOfType<Phase6GameManager>();
        RefreshWorkstations();
    }

    public void RefreshWorkstations()
    {
        allWorkstations = new List<Workstation>(FindObjectsOfType<Workstation>());
    }

    private void Update()
    {
        FindNearestWorkstation();
    }

    private void FindNearestWorkstation()
    {
        nearestWorkstation = null;
        isInRange = false;
        float minDist = float.MaxValue;

        Vector3 playerPos = transform.position;

        if (allWorkstations == null) return;

        foreach (Workstation ws in allWorkstations)
        {
            if (ws == null || ws.InteractionPoint == null) continue;
            float dist = Vector3.Distance(playerPos, ws.InteractionPoint.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestWorkstation = ws;
            }
        }

        if (nearestWorkstation != null && nearestWorkstation.InteractionPoint != null
            && minDist <= nearestWorkstation.InteractionPoint.InteractionDistance)
        {
            isInRange = true;
        }
    }

    public bool TryInteract()
    {
        if (gameManager != null && gameManager.CurrentState != Phase6GameState.Playing)
        {
            return false;
        }

        if (nearestWorkstation == null || !isInRange)
        {
            return false;
        }

        nearestWorkstation.Interact(character);
        return true;
    }
}