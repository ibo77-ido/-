using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class BoatWaterDriftController : MonoBehaviour
{
    [Header("Water Link")]
    [SerializeField] private Material waterFlowMaterial;
    [SerializeField, Range(0f, 1f)] private float waterMotionIntensity = 0.55f;
    [SerializeField] private bool readWaterMaterial = true;

    [Header("Motion")]
    [SerializeField, Range(0f, 0.2f)] private float driftDistance = 0.035f;
    [SerializeField, Range(0f, 0.2f)] private float bobDistance = 0.022f;
    [SerializeField, Range(0f, 8f)] private float rollAngle = 1.6f;
    [SerializeField, Range(0.05f, 5f)] private float motionSpeed = 0.42f;
    [SerializeField, Range(0f, 1f)] private float irregularity = 0.35f;
    [SerializeField] private float phaseOffset = 0.73f;

    [Header("Local Axes")]
    [SerializeField] private Vector3 driftAxis = Vector3.right;
    [SerializeField] private Vector3 bobAxis = Vector3.up;
    [SerializeField] private Vector3 rollAxis = Vector3.forward;

    [Header("Editor Preview")]
    [SerializeField] private bool previewInEditMode;

    private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");
    private static readonly int RippleStrengthId = Shader.PropertyToID("_RippleStrength");
    private static readonly int FoamIntensityId = Shader.PropertyToID("_FoamIntensity");

    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private bool hasBaseTransform;

    private void OnEnable()
    {
        CacheBaseTransform();
        TryAutoBindWaterMaterial();
    }

    private void OnValidate()
    {
        if (!hasBaseTransform)
        {
            CacheBaseTransform();
        }

        driftAxis = NormalizeOrFallback(driftAxis, Vector3.right);
        bobAxis = NormalizeOrFallback(bobAxis, Vector3.up);
        rollAxis = NormalizeOrFallback(rollAxis, Vector3.forward);
        TryAutoBindWaterMaterial();

        if (!Application.isPlaying && !previewInEditMode)
        {
            RestoreBaseTransform();
        }
    }

    private void Update()
    {
        if (!Application.isPlaying && !previewInEditMode)
        {
            RestoreBaseTransform();
            return;
        }

        Apply(Application.isPlaying ? Time.time : GetEditorPreviewTime());
    }

    private void OnDisable()
    {
        RestoreBaseTransform();
    }

    public void CacheBaseTransform()
    {
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;
        hasBaseTransform = true;
    }

    public void RestoreBaseTransform()
    {
        if (!hasBaseTransform)
        {
            return;
        }

        transform.localPosition = baseLocalPosition;
        transform.localRotation = baseLocalRotation;
    }

    public float GetEffectiveWaterIntensity()
    {
        float materialIntensity = 0f;
        if (readWaterMaterial && waterFlowMaterial)
        {
            float flowSpeed = waterFlowMaterial.HasProperty(FlowSpeedId) ? waterFlowMaterial.GetFloat(FlowSpeedId) : 0f;
            float rippleStrength = waterFlowMaterial.HasProperty(RippleStrengthId) ? waterFlowMaterial.GetFloat(RippleStrengthId) : 0f;
            float foamIntensity = waterFlowMaterial.HasProperty(FoamIntensityId) ? waterFlowMaterial.GetFloat(FoamIntensityId) : 0f;

            materialIntensity = Mathf.Clamp01(flowSpeed / 0.12f * 0.45f + rippleStrength / 0.08f * 0.4f + foamIntensity * 0.15f);
        }

        return Mathf.Clamp01(waterMotionIntensity + materialIntensity);
    }

    private void Apply(float time)
    {
        if (!hasBaseTransform)
        {
            CacheBaseTransform();
        }

        float intensity = GetEffectiveWaterIntensity();
        float t = time * motionSpeed + phaseOffset;
        float mainWave = Mathf.Sin(t);
        float secondaryWave = Mathf.Sin(t * 1.71f + 1.37f) * irregularity;
        float slowWave = Mathf.Sin(t * 0.53f + 2.11f) * 0.35f;
        float waterWave = mainWave + secondaryWave;

        Vector3 offset =
            driftAxis * (waterWave * driftDistance * intensity) +
            bobAxis * ((Mathf.Sin(t * 1.23f + 0.41f) + slowWave) * bobDistance * intensity);

        float roll = (mainWave * 0.75f + secondaryWave + slowWave * 0.35f) * rollAngle * intensity;

        transform.localPosition = baseLocalPosition + offset;
        transform.localRotation = baseLocalRotation * Quaternion.AngleAxis(roll, rollAxis);
    }

    private void TryAutoBindWaterMaterial()
    {
        if (waterFlowMaterial)
        {
            return;
        }

        GameObject water = GameObject.Find("水体外部流动");
        if (!water)
        {
            return;
        }

        SpriteRenderer renderer = water.GetComponent<SpriteRenderer>();
        if (renderer)
        {
            waterFlowMaterial = renderer.sharedMaterial;
        }
    }

    private static Vector3 NormalizeOrFallback(Vector3 value, Vector3 fallback)
    {
        return value.sqrMagnitude > 0.0001f ? value.normalized : fallback;
    }

    private static float GetEditorPreviewTime()
    {
#if UNITY_EDITOR
        return (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
        return Time.time;
#endif
    }
}
