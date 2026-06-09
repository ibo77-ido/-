using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private NavMeshPath manualPath;
    private Vector3[] pathCorners = new Vector3[0];
    private int currentCornerIndex;
    private bool hasDestination;
    private Vector3 manualVelocity;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        ConfigureManualMovement();
    }

    private void OnEnable()
    {
        ConfigureManualMovement();
    }

    private void Update()
    {
        TickManualMovement(Time.deltaTime);
    }

    public void TickManualMovement(float deltaTime)
    {
        if (!hasDestination || navMeshAgent == null) return;
        if (deltaTime <= 0f) return;
        if (!EnsureOnNavMesh()) return;

        if (pathCorners == null || pathCorners.Length == 0 || currentCornerIndex >= pathCorners.Length)
        {
            Stop();
            return;
        }

        if (HasReachedDestination())
        {
            Stop();
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 steeringTarget = pathCorners[currentCornerIndex];
        steeringTarget.y = currentPosition.y;

        Vector3 toSteeringTarget = steeringTarget - currentPosition;
        if (toSteeringTarget.magnitude <= Mathf.Max(0.05f, navMeshAgent.stoppingDistance))
        {
            currentCornerIndex++;
            return;
        }

        float moveDistance = navMeshAgent.speed * deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(currentPosition, steeringTarget, moveDistance);
        manualVelocity = (nextPosition - currentPosition) / deltaTime;

        transform.position = nextPosition;
        navMeshAgent.nextPosition = nextPosition;
        navMeshAgent.velocity = manualVelocity;
    }

    private void ConfigureManualMovement()
    {
        if (navMeshAgent == null) return;

        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        if (manualPath == null)
        {
            manualPath = new NavMeshPath();
        }
    }

    public bool EnsureOnNavMesh(float searchDistance = 2f)
    {
        if (navMeshAgent == null) return false;
        if (navMeshAgent.isOnNavMesh) return true;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, searchDistance, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
            transform.position = hit.position;
            navMeshAgent.nextPosition = hit.position;
            return navMeshAgent.isOnNavMesh;
        }

        return false;
    }

    public void SetDestination(Vector3 target)
    {
        if (!EnsureOnNavMesh()) return;
        ConfigureManualMovement();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 3f, NavMesh.AllAreas))
        {
            if (manualPath == null)
            {
                manualPath = new NavMeshPath();
            }

            bool pathFound = navMeshAgent.CalculatePath(hit.position, manualPath);
            if (pathFound && manualPath.status != NavMeshPathStatus.PathInvalid && manualPath.corners.Length > 1)
            {
                navMeshAgent.ResetPath();
                pathCorners = manualPath.corners;
                currentCornerIndex = 1;
                hasDestination = true;
            }
        }
    }

    public void Stop()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
        }
        manualVelocity = Vector3.zero;
        pathCorners = new Vector3[0];
        currentCornerIndex = 0;
        hasDestination = false;
    }

    public bool IsMoving()
    {
        return hasDestination && navMeshAgent != null && navMeshAgent.isOnNavMesh
            && !HasReachedDestination();
    }

    public bool HasReachedDestination()
    {
        if (!hasDestination || navMeshAgent == null || !navMeshAgent.isOnNavMesh) return false;
        if (pathCorners == null || pathCorners.Length == 0) return false;

        Vector3 currentPosition = transform.position;
        Vector3 destination = pathCorners[pathCorners.Length - 1];
        currentPosition.y = 0f;
        destination.y = 0f;

        return Vector3.Distance(currentPosition, destination) <= navMeshAgent.stoppingDistance;
    }
}
