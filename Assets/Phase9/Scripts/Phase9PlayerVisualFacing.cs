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

    private enum FacingMode
    {
        FrontBackOnly,
        FourDirection
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
    [SerializeField] private FacingMode facingMode = FacingMode.FrontBackOnly;
    [SerializeField, Min(0.0001f)] private float moveThreshold = 0.001f;
    [SerializeField, Min(0.01f)] private float frameRate = 8f;
    [SerializeField] private FacingDirection initialDirection = FacingDirection.Front;

    [Header("Idle Sprites")]
    [SerializeField] private Sprite idleFront;
    [SerializeField] private Sprite idleBack;
    [SerializeField] private Sprite idleLeft;
    [SerializeField] private Sprite idleRight;

    [Header("Walk Sprites")]
    [SerializeField] private Sprite[] walkFront = new Sprite[0];
    [SerializeField] private Sprite[] walkBack = new Sprite[0];
    [SerializeField] private Sprite[] walkLeft = new Sprite[0];
    [SerializeField] private Sprite[] walkRight = new Sprite[0];

    private Vector3 previousRootPosition;
    private FacingDirection currentDirection;
    private int currentFrame;
    private float frameTimer;
    private bool wasMoving;

    private void Awake()
    {
        ResolveVisualRoot();
        ResolveRuntimeReferences();
        previousRootPosition = movementRoot != null ? movementRoot.position : transform.position;
        currentDirection = initialDirection;
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

        Vector2 planarDelta = new Vector2(delta.x, delta.z);
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
        if (facingMode == FacingMode.FrontBackOnly)
        {
            if (Mathf.Abs(planarDelta.y) > moveThreshold)
            {
                currentDirection = planarDelta.y >= 0f ? FacingDirection.Back : FacingDirection.Front;
            }

            return;
        }

        if (Mathf.Abs(planarDelta.x) >= Mathf.Abs(planarDelta.y))
        {
            currentDirection = planarDelta.x >= 0f ? FacingDirection.Right : FacingDirection.Left;
            return;
        }

        currentDirection = planarDelta.y >= 0f ? FacingDirection.Back : FacingDirection.Front;
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
            targetRenderer.sprite = frames[frame];
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
            targetRenderer.sprite = sprite;
        }
    }

    private Sprite GetIdleSprite(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Back:
                return idleBack != null ? idleBack : idleFront;
            case FacingDirection.Left:
                return idleLeft != null ? idleLeft : idleFront;
            case FacingDirection.Right:
                return idleRight != null ? idleRight : idleLeft != null ? idleLeft : idleFront;
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
            case FacingDirection.Left:
                return walkLeft;
            case FacingDirection.Right:
                return walkRight;
            default:
                return walkFront;
        }
    }
}
