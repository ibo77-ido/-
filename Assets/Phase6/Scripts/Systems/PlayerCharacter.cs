using UnityEngine;
using UnityEngine.AI;

public class PlayerCharacter : MonoBehaviour, ICharacter
{
    [Header("References")]
    [SerializeField] private CharacterConfigSO config;
    [SerializeField] private Phase6GameManager gameManager;
    [SerializeField] private MovementController movementController;
    [SerializeField] private CharacterStateMachine stateMachine;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform logicRoot;
    [SerializeField] private Transform artRoot;
    [SerializeField] private InteractionController interactionController;

    public Transform Transform => transform;
    public CharacterState CurrentState => stateMachine != null ? stateMachine.CurrentState : CharacterState.Idle;

    private void Awake()
    {
        if (movementController == null) movementController = GetComponent<MovementController>();
        if (stateMachine == null) stateMachine = GetComponent<CharacterStateMachine>();
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
        if (logicRoot == null) logicRoot = transform.Find("LogicRoot");
        if (artRoot == null) artRoot = transform.Find("ArtRoot");
        if (interactionController == null) interactionController = GetComponent<InteractionController>();
    }

    private void Start()
    {
        if (config != null && navMeshAgent != null)
        {
            navMeshAgent.speed = config.moveSpeed;
            navMeshAgent.stoppingDistance = config.stoppingDistance;
            navMeshAgent.acceleration = config.acceleration;
        }

        if (movementController != null)
        {
            movementController.EnsureOnNavMesh();
        }
    }

    public void SetDestination(Vector3 target)
    {
        TrySetDestination(target);
    }

    public bool TrySetDestination(Vector3 target)
    {
        if (gameManager != null && !gameManager.CanMove()) return false;
        if (movementController != null)
        {
            return movementController.TrySetDestination(target);
        }

        return false;
    }

    public void StopMoving()
    {
        if (movementController != null)
        {
            movementController.Stop();
        }
    }
}
