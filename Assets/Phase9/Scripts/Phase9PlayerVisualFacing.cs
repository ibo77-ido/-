using UnityEngine;

[DisallowMultipleComponent]
public sealed class Phase9PlayerVisualFacing : MonoBehaviour
{
    private enum FacingDirection
    {
        Front,
        Back,
        Left,
        Right
    }

    [Header("Static Facing Fix")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Vector3 desiredLocalEuler;
    [SerializeField] private Vector3 desiredRootEuler = new Vector3(180f, 0f, 0f);
    [SerializeField] private bool desiredFlipY = true;

    [Header("Move Facing")]
    [SerializeField] private bool useMoveFacing = true;
    [SerializeField] private Transform movementRoot;
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private MovementController movementController;
    [SerializeField] private bool useTransformYAsForwardAxis = true;
    [SerializeField] private bool preferPathForwardDirection = true;
    [SerializeField, Min(0.0001f)] private float moveThreshold = 0.001f;
    [SerializeField, Min(0.01f)] private float frameRate = 8f;
    [SerializeField] private FacingDirection initialDirection = FacingDirection.Front;
    [SerializeField, Min(0f)] private float referenceSpriteHeight;

    [Header("Idle Sprites")]
    [SerializeField] private Sprite idleFront;
    [SerializeField] private Sprite idleBack;

    [Header("Walk Sprites")]
    [SerializeField] private Sprite[] walkFront = new Sprite[0];
    [SerializeField] private Sprite[] walkBack = new Sprite[0];

    private Vector3 previousRootPosition;
    private FacingDirection currentDirection;
    private int currentFrame;
    private float frameTimer;
    private bool wasMoving;
    private Vector3 baseVisualScale = Vector3.one;
    private float resolvedReferenceSpriteHeight;
    private System.Reflection.FieldInfo pathCornersField;
    private System.Reflection.FieldInfo currentCornerIndexField;
    private System.Reflection.FieldInfo hasDestinationField;
    private System.Reflection.FieldInfo mapNavMeshZToTransformYField;

    private void Awake()
    {
        ResolveVisualRoot();
        ResolveRuntimeReferences();
        previousRootPosition = movementRoot != null ? movementRoot.position : transform.position;
        currentDirection = initialDirection;
        baseVisualScale = visualRoot != null ? visualRoot.localScale : Vector3.one;
        ResolveReferenceSpriteHeight();
        ApplyRootFacing();
        ApplyFacing();
        ApplySpriteFacing();
        ApplyIdleSprite();
    }

    private void LateUpdate()
    {
        ApplyRootFacing();
        ApplyFacing();
        ApplySpriteFacing();
        UpdateMoveFacing();
    }

    private void OnValidate()
    {
        ResolveVisualRoot();
        ApplyRootFacing();
        ApplyFacing();
    }

    private void ApplyRootFacing()
    {
        transform.localRotation = Quaternion.Euler(desiredRootEuler);
    }

    private void ResolveVisualRoot()
    {
        if (visualRoot != null)
        {
            return;
        }

        Transform child = transform.Find("VisualRoot");
        visualRoot = child != null ? child : transform;
    }

    private void ResolveRuntimeReferences()
    {
        if (movementRoot == null)
        {
            movementRoot = transform.root;
        }

        if (targetRenderer == null)
        {
            if (visualRoot != null)
            {
                targetRenderer = visualRoot.GetComponent<SpriteRenderer>();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }
        }

        if (movementController == null && movementRoot != null)
        {
            movementController = movementRoot.GetComponent<MovementController>();
        }
    }

    private void ApplyFacing()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localRotation = Quaternion.Euler(desiredLocalEuler);
    }

    private void ApplySpriteFacing()
    {
        if (visualRoot == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = visualRoot.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.flipY = desiredFlipY;
    }

    private void UpdateMoveFacing()
    {
        if (!useMoveFacing)
        {
            return;
        }

        ResolveRuntimeReferences();
        if (movementRoot == null || targetRenderer == null)
        {
            return;
        }

        Vector3 currentPosition = movementRoot.position;
        Vector3 delta = currentPosition - previousRootPosition;
        previousRootPosition = currentPosition;

        Vector2 planarDelta;
        if (!TryGetPathForwardDelta(out planarDelta))
        {
            planarDelta = GetMovementPlaneDelta(delta);
        }

        bool isMoving = planarDelta.sqrMagnitude > moveThreshold * moveThreshold;
        if (isMoving)
        {
            SetDirectionFromDelta(planarDelta);
            AdvanceWalkFrame();
            ApplyWalkSprite();
            wasMoving = true;
            return;
        }

        frameTimer = 0f;
        currentFrame = 0;
        if (wasMoving)
        {
            ApplyIdleSprite();
            wasMoving = false;
        }
    }

    private void SetDirectionFromDelta(Vector2 planarDelta)
    {
        if (Mathf.Abs(planarDelta.y) > moveThreshold)
        {
            currentDirection = planarDelta.y >= 0f ? FacingDirection.Back : FacingDirection.Front;
        }
    }

    private Vector2 GetMovementPlaneDelta(Vector3 worldDelta)
    {
        if (ShouldUseTransformYAsForwardAxis())
        {
            return new Vector2(worldDelta.x, worldDelta.y);
        }

        return new Vector2(worldDelta.x, worldDelta.z);
    }

    private bool TryGetPathForwardDelta(out Vector2 planarDelta)
    {
        planarDelta = Vector2.zero;

        if (!preferPathForwardDirection || movementController == null || movementRoot == null)
        {
            return false;
        }

        EnsureMovementReflection();
        if (pathCornersField == null || currentCornerIndexField == null || hasDestinationField == null)
        {
            return false;
        }

        bool hasDestination = (bool)hasDestinationField.GetValue(movementController);
        if (!hasDestination)
        {
            return false;
        }

        Vector3[] pathCorners = pathCornersField.GetValue(movementController) as Vector3[];
        if (pathCorners == null || pathCorners.Length == 0)
        {
            return false;
        }

        int currentCornerIndex = (int)currentCornerIndexField.GetValue(movementController);
        if (currentCornerIndex < 0 || currentCornerIndex >= pathCorners.Length)
        {
            return false;
        }

        Vector2 currentPoint = GetMovementPlanePoint(movementRoot.position);
        for (int i = currentCornerIndex; i < pathCorners.Length; i++)
        {
            Vector2 candidateDelta = GetPathPlanePoint(pathCorners[i]) - currentPoint;
            if (candidateDelta.sqrMagnitude <= moveThreshold * moveThreshold)
            {
                continue;
            }

            if (TryUseFrontBackDelta(candidateDelta, out planarDelta))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryUseFrontBackDelta(Vector2 candidateDelta, out Vector2 planarDelta)
    {
        planarDelta = Vector2.zero;
        if (Mathf.Abs(candidateDelta.y) <= moveThreshold)
        {
            return false;
        }

        planarDelta = candidateDelta;
        return true;
    }

    private Vector2 GetMovementPlanePoint(Vector3 worldPoint)
    {
        if (ShouldUseTransformYAsForwardAxis())
        {
            return new Vector2(worldPoint.x, worldPoint.y);
        }

        return new Vector2(worldPoint.x, worldPoint.z);
    }

    private Vector2 GetPathPlanePoint(Vector3 pathPoint)
    {
        if (ShouldUseTransformYAsForwardAxis())
        {
            return new Vector2(pathPoint.x, pathPoint.z);
        }

        return new Vector2(pathPoint.x, pathPoint.z);
    }

    private bool ShouldUseTransformYAsForwardAxis()
    {
        EnsureMovementReflection();
        if (movementController != null && mapNavMeshZToTransformYField != null)
        {
            object value = mapNavMeshZToTransformYField.GetValue(movementController);
            if (value is bool)
            {
                return (bool)value;
            }
        }

        return useTransformYAsForwardAxis;
    }

    private void EnsureMovementReflection()
    {
        if (pathCornersField != null)
        {
            return;
        }

        System.Type type = typeof(MovementController);
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic;
        pathCornersField = type.GetField("pathCorners", flags);
        currentCornerIndexField = type.GetField("currentCornerIndex", flags);
        hasDestinationField = type.GetField("hasDestination", flags);
        mapNavMeshZToTransformYField = type.GetField("mapNavMeshZToTransformY", flags);
    }

    private void AdvanceWalkFrame()
    {
        Sprite[] frames = GetWalkFrames(currentDirection);
        if (frames == null || frames.Length == 0)
        {
            currentFrame = 0;
            frameTimer = 0f;
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / frameRate;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame = (currentFrame + 1) % frames.Length;
        }
    }

    private void ApplyWalkSprite()
    {
        Sprite[] frames = GetWalkFrames(currentDirection);
        if (frames == null || frames.Length == 0)
        {
            ApplyIdleSprite();
            return;
        }

        int frame = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
        if (frames[frame] != null)
        {
            SetRendererSprite(frames[frame]);
        }
    }

    private void ApplyIdleSprite()
    {
        if (targetRenderer == null)
        {
            return;
        }

        Sprite sprite = GetIdleSprite(currentDirection);
        if (sprite != null)
        {
            SetRendererSprite(sprite);
        }
    }

    private void SetRendererSprite(Sprite sprite)
    {
        if (targetRenderer == null || sprite == null)
        {
            return;
        }

        targetRenderer.sprite = sprite;
        ApplySpriteHeightNormalization(sprite);
    }

    private void ApplySpriteHeightNormalization(Sprite sprite)
    {
        if (visualRoot == null || sprite == null)
        {
            return;
        }

        if (resolvedReferenceSpriteHeight <= 0f)
        {
            ResolveReferenceSpriteHeight();
        }

        if (resolvedReferenceSpriteHeight <= 0f || sprite.bounds.size.y <= 0f)
        {
            visualRoot.localScale = baseVisualScale;
            return;
        }

        float scaleMultiplier = resolvedReferenceSpriteHeight / sprite.bounds.size.y;
        visualRoot.localScale = new Vector3(
            baseVisualScale.x * scaleMultiplier,
            baseVisualScale.y * scaleMultiplier,
            baseVisualScale.z * scaleMultiplier);
    }

    private void ResolveReferenceSpriteHeight()
    {
        resolvedReferenceSpriteHeight = referenceSpriteHeight > 0f
            ? referenceSpriteHeight
            : GetFirstSpriteHeight(idleFront, walkFront, walkBack);
    }

    private static float GetFirstSpriteHeight(Sprite preferredSprite, params Sprite[][] frameSets)
    {
        if (preferredSprite != null && preferredSprite.bounds.size.y > 0f)
        {
            return preferredSprite.bounds.size.y;
        }

        if (frameSets == null)
        {
            return 0f;
        }

        for (int i = 0; i < frameSets.Length; i++)
        {
            Sprite[] frames = frameSets[i];
            if (frames == null)
            {
                continue;
            }

            for (int frame = 0; frame < frames.Length; frame++)
            {
                Sprite sprite = frames[frame];
                if (sprite != null && sprite.bounds.size.y > 0f)
                {
                    return sprite.bounds.size.y;
                }
            }
        }

        return 0f;
    }

    private Sprite GetIdleSprite(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Back:
                return idleBack != null ? idleBack : idleFront;
            default:
                return idleFront;
        }
    }

    private Sprite[] GetWalkFrames(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Back:
                return walkBack;
            default:
                return walkFront;
        }
    }
}
