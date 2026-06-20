using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public sealed class CharacterIdleState : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField] private bool enterIdleOnStart = true;
    [SerializeField] private bool lockPosition = true;
    [SerializeField] private bool keepCurrentSpriteAsIdle = true;
    [SerializeField] private Sprite idleSprite;

    [Header("Breathing Motion")]
    [SerializeField] private bool useBreathingMotion = true;
    [SerializeField, Min(0.01f)] private float breathingSpeed = 0.85f;
    [SerializeField, Range(0f, 0.08f)] private float breathingAmount = 0.018f;
    [SerializeField, Range(0f, 0.6f)] private float footLock = 0.24f;
    [SerializeField, Range(0f, 1f)] private float upperBodyStart = 0.38f;
    [SerializeField, Range(0f, 1f)] private float chestCenterX = 0.5f;
    [SerializeField, Range(0f, 1f)] private float verticalStretch = 0.55f;
    [SerializeField, Range(0f, 1f)] private float horizontalStretch = 0.75f;
    [SerializeField] private bool randomizeBreathingPhase = true;

    [Header("Optional Animator Parameters")]
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string idleBoolName = "IsIdle";
    [SerializeField] private string movingBoolName = "IsMoving";
    [SerializeField] private string speedFloatName = "Speed";

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Rigidbody rigidbody3D;
    private Rigidbody2D rigidbody2D;
    private Vector3 idlePosition;
    private Quaternion idleRotation;
    private Vector3 idleScale;
    private float breathingPhase;
    private MaterialPropertyBlock materialPropertyBlock;
    private bool isIdle;

    private static readonly int BreathAmountId = Shader.PropertyToID("_BreathAmount");
    private static readonly int BreathValueId = Shader.PropertyToID("_BreathValue");
    private static readonly int FootLockId = Shader.PropertyToID("_FootLock");
    private static readonly int UpperBodyStartId = Shader.PropertyToID("_UpperBodyStart");
    private static readonly int ChestCenterXId = Shader.PropertyToID("_ChestCenterX");
    private static readonly int VerticalStretchId = Shader.PropertyToID("_VerticalStretch");
    private static readonly int HorizontalStretchId = Shader.PropertyToID("_HorizontalStretch");

    private void Awake()
    {
        ResolveComponents();
        CaptureIdlePose();
        breathingPhase = randomizeBreathingPhase
            ? Random.Range(0f, Mathf.PI * 2f)
            : 0f;

        if (keepCurrentSpriteAsIdle && idleSprite == null && spriteRenderer != null)
        {
            idleSprite = spriteRenderer.sprite;
        }
    }

    private void OnEnable()
    {
        if (enterIdleOnStart)
        {
            EnterIdle();
        }
    }

    private void LateUpdate()
    {
        if (!isIdle)
        {
            return;
        }

        ApplyIdleSprite();
        ApplyUpperBodyBreath();

        if (lockPosition)
        {
            ApplyIdlePose();
        }
    }

    public void EnterIdle()
    {
        ResolveComponents();
        isIdle = true;
        StopMotion();
        ApplyIdleSprite();
        ApplyAnimatorIdle();
    }

    public void ExitIdle()
    {
        isIdle = false;
        transform.localScale = idleScale;
        ResetUpperBodyBreath();

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = false;
        }

        SetAnimatorBool(idleBoolName, false);
    }

    public void CaptureIdlePose()
    {
        idlePosition = transform.position;
        idleRotation = transform.rotation;
        idleScale = transform.localScale;
    }

    public void SetIdleSprite(Sprite sprite)
    {
        idleSprite = sprite;
        ApplyIdleSprite();
    }

    private void ResolveComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (rigidbody3D == null)
        {
            rigidbody3D = GetComponent<Rigidbody>();
        }

        if (rigidbody2D == null)
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
        }

        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }
    }

    private void StopMotion()
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }

            navMeshAgent.velocity = Vector3.zero;
        }

        if (rigidbody3D != null)
        {
            rigidbody3D.velocity = Vector3.zero;
            rigidbody3D.angularVelocity = Vector3.zero;
        }

        if (rigidbody2D != null)
        {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
        }
    }

    private void ApplyIdleSprite()
    {
        if (spriteRenderer != null && idleSprite != null && spriteRenderer.sprite != idleSprite)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }

    private void ApplyIdlePose()
    {
        transform.SetPositionAndRotation(idlePosition, idleRotation);
        transform.localScale = idleScale;
    }

    private void ApplyUpperBodyBreath()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.GetPropertyBlock(materialPropertyBlock);

        float breath = useBreathingMotion
            ? Mathf.Sin((Time.time * breathingSpeed * Mathf.PI * 2f) + breathingPhase)
            : 0f;

        materialPropertyBlock.SetFloat(BreathAmountId, breathingAmount);
        materialPropertyBlock.SetFloat(BreathValueId, breath);
        materialPropertyBlock.SetFloat(FootLockId, footLock);
        materialPropertyBlock.SetFloat(UpperBodyStartId, Mathf.Max(footLock + 0.001f, upperBodyStart));
        materialPropertyBlock.SetFloat(ChestCenterXId, chestCenterX);
        materialPropertyBlock.SetFloat(VerticalStretchId, verticalStretch);
        materialPropertyBlock.SetFloat(HorizontalStretchId, horizontalStretch);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void ResetUpperBodyBreath()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetFloat(BreathValueId, 0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void ApplyAnimatorIdle()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        SetAnimatorBool(idleBoolName, true);
        SetAnimatorBool(movingBoolName, false);
        SetAnimatorFloat(speedFloatName, 0f);
        PlayAnimatorStateIfAvailable(idleStateName);
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (TryFindAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(parameterName, value);
        }
    }

    private void SetAnimatorFloat(string parameterName, float value)
    {
        if (TryFindAnimatorParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parameterName, value);
        }
    }

    private bool TryFindAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        AnimatorControllerParameter[] parameters = animator.parameters;
        int parameterHash = Animator.StringToHash(parameterName);
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter.nameHash == parameterHash && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }

    private void PlayAnimatorStateIfAvailable(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            return;
        }

        int stateHash = Animator.StringToHash(stateName);
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            if (!animator.HasState(layer, stateHash))
            {
                continue;
            }

            animator.Play(stateHash, layer, 0f);
            animator.Update(0f);
            return;
        }
    }
}
