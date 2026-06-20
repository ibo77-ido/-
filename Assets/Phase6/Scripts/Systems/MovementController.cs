using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    [Header("Plane Mapping")]
    public bool mapNavMeshZToTransformY;
    public float mappedNavMeshY;
    public float currentPositionSampleDistance = 2f;
    public float destinationSampleDistance = 3f;
    public float fallbackSpeed = 2.5f;
    public float fallbackStoppingDistance = 0.08f;

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

        Vector3 currentPosition = GetCurrentPathPosition();
        Vector3 steeringTarget = pathCorners[currentCornerIndex];
        if (!mapNavMeshZToTransformY)
        {
            steeringTarget.y = currentPosition.y;
        }

        Vector3 toSteeringTarget = steeringTarget - currentPosition;
        float cornerThreshold = Mathf.Max(0.05f, GetStoppingDistance());
        if (toSteeringTarget.sqrMagnitude <= cornerThreshold * cornerThreshold)
        {
            currentCornerIndex++;
            return;
        }

        float moveDistance = GetMoveSpeed() * deltaTime;
        Vector3 nextPathPosition = Vector3.MoveTowards(currentPosition, steeringTarget, moveDistance);
        Vector3 nextPosition = ToTransformPosition(nextPathPosition);
        manualVelocity = (nextPosition - transform.position) / deltaTime;

        transform.position = nextPosition;
        if (!mapNavMeshZToTransformY && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.nextPosition = nextPosition;
            navMeshAgent.velocity = manualVelocity;
        }
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

        if (mapNavMeshZToTransformY)
        {
            NavMeshHit mappedHit;
            if (NavMesh.SamplePosition(ToPathPosition(transform.position), out mappedHit, Mathf.Max(searchDistance, currentPositionSampleDistance), NavMesh.AllAreas))
            {
                Vector3 correctedPosition = ToTransformPosition(mappedHit.position);
                transform.position = correctedPosition;
                return true;
            }

            return false;
        }

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
        if (NavMesh.SamplePosition(target, out hit, destinationSampleDistance, NavMesh.AllAreas))
        {
            if (manualPath == null)
            {
                manualPath = new NavMeshPath();
            }

            bool pathFound = mapNavMeshZToTransformY
                ? NavMesh.CalculatePath(GetCurrentPathPosition(), hit.position, NavMesh.AllAreas, manualPath)
                : navMeshAgent.CalculatePath(hit.position, manualPath);

            if (pathFound && manualPath.status == NavMeshPathStatus.PathComplete && manualPath.corners.Length > 1)
            {
                if (!mapNavMeshZToTransformY && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.ResetPath();
                }
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
        return hasDestination && navMeshAgent != null && (mapNavMeshZToTransformY || navMeshAgent.isOnNavMesh)
            && !HasReachedDestination();
    }

    public bool HasReachedDestination()
    {
        if (!hasDestination || navMeshAgent == null) return false;
        if (!mapNavMeshZToTransformY && !navMeshAgent.isOnNavMesh) return false;
        if (pathCorners == null || pathCorners.Length == 0) return false;

        Vector3 currentPosition = GetCurrentPathPosition();
        Vector3 destination = pathCorners[pathCorners.Length - 1];
        if (!mapNavMeshZToTransformY)
        {
            currentPosition.y = 0f;
            destination.y = 0f;
        }

        float stoppingDistance = GetStoppingDistance();
        return (currentPosition - destination).sqrMagnitude <= stoppingDistance * stoppingDistance;
    }

    public void SetMapNavMeshZToTransformY(bool enabled)
    {
        mapNavMeshZToTransformY = enabled;
    }

    public void SetSampleDistances(float currentPositionDistance, float destinationDistance)
    {
        currentPositionSampleDistance = Mathf.Max(0.01f, currentPositionDistance);
        destinationSampleDistance = Mathf.Max(0.01f, destinationDistance);
    }

    public Vector3 GetNavMeshCenterPoint()
    {
        return transform.position;
    }

    public bool IsCenterOnNavMesh()
    {
        return IsCenterOnNavMesh(0.05f);
    }

    public bool IsCenterOnNavMesh(float sampleDistance)
    {
        NavMeshHit hit;
        Vector3 center = mapNavMeshZToTransformY ? ToPathPosition(GetNavMeshCenterPoint()) : GetNavMeshCenterPoint();
        return NavMesh.SamplePosition(center, out hit, Mathf.Max(0.001f, sampleDistance), NavMesh.AllAreas);
    }

    public void SetMappedNavMeshY(float navMeshY)
    {
        mappedNavMeshY = navMeshY;
    }

    public void SetFallbackMovementSettings(float speed, float stoppingDistance)
    {
        fallbackSpeed = Mathf.Max(0.01f, speed);
        fallbackStoppingDistance = Mathf.Max(0.01f, stoppingDistance);
    }

    private float GetMoveSpeed()
    {
        return navMeshAgent != null && navMeshAgent.enabled ? navMeshAgent.speed : fallbackSpeed;
    }

    private float GetStoppingDistance()
    {
        return navMeshAgent != null && navMeshAgent.enabled ? navMeshAgent.stoppingDistance : fallbackStoppingDistance;
    }

    private Vector3 GetCurrentPathPosition()
    {
        return mapNavMeshZToTransformY ? ToPathPosition(transform.position) : transform.position;
    }

    private Vector3 ToPathPosition(Vector3 transformPosition)
    {
        return new Vector3(transformPosition.x, mappedNavMeshY, transformPosition.y);
    }

    private Vector3 ToTransformPosition(Vector3 pathPosition)
    {
        return mapNavMeshZToTransformY
            ? new Vector3(pathPosition.x, pathPosition.z, transform.position.z)
            : pathPosition;
    }
}
