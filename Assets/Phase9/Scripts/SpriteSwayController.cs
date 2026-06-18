using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class SpriteSwayController : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxChildren = 8;
    [SerializeField, Range(0f, 10f)] private float swayAngle = 2.5f;
    [SerializeField, Range(0f, 5f)] private float swaySpeed = 1.25f;
    [SerializeField, Range(0f, 1f)] private float horizontalDrift = 0f;
    [SerializeField, Range(0f, 0.6f)] private float baseMotionFloor = 0.28f;
    [SerializeField, Range(0.5f, 8f)] private float heightExponent = 4.2f;
    [SerializeField, Range(0f, 1f)] private float driftScale = 0.8f;
    [SerializeField, Range(0f, 1f)] private float rotationInfluence = 0f;
    [SerializeField, Min(0)] private int lockBottomSegments = 2;

    private static readonly int PhaseOffsetId = Shader.PropertyToID("_PhaseOffset");

    private SpriteRenderer[] renderers = new SpriteRenderer[0];
    private Transform[] targets = new Transform[0];
    private Vector3[] basePositions = new Vector3[0];
    private Quaternion[] baseRotations = new Quaternion[0];
    private float[] motionWeights = new float[0];
    private float[] rootLocalHeights = new float[0];
    private float[] rootLocalBottoms = new float[0];
    private float[] rootLocalTops = new float[0];
    private MaterialPropertyBlock materialBlock;

    private void OnEnable()
    {
        CacheTargets();
    }

    private void OnValidate()
    {
        CacheTargets();
        if (Application.isPlaying)
        {
            Apply(Time.time);
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Apply(Time.time);
    }

    private void OnDisable()
    {
        RestoreBaseTransforms();
    }

    public void CacheTargets()
    {
        RestoreBaseTransforms();

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        int eligibleCount = 0;
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] && allRenderers[i].transform != transform)
            {
                eligibleCount++;
            }
        }

        int count = Mathf.Min(maxChildren, eligibleCount);
        renderers = new SpriteRenderer[count];
        targets = new Transform[count];
        basePositions = new Vector3[count];
        baseRotations = new Quaternion[count];
        motionWeights = new float[count];
        rootLocalHeights = new float[count];
        rootLocalBottoms = new float[count];
        rootLocalTops = new float[count];

        int applied = 0;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < allRenderers.Length && applied < count; i++)
        {
            SpriteRenderer spriteRenderer = allRenderers[i];
            if (!spriteRenderer)
                continue;

            Transform target = spriteRenderer.transform;
            if (!target || target == transform)
                continue;

            GetRendererRootYBounds(spriteRenderer, out float bottomY, out float topY);
            float localY = (bottomY + topY) * 0.5f;
            rootLocalHeights[applied] = localY;
            rootLocalBottoms[applied] = bottomY;
            rootLocalTops[applied] = topY;
            minY = Mathf.Min(minY, bottomY);
            maxY = Mathf.Max(maxY, topY);

            renderers[applied] = spriteRenderer;
            targets[applied] = target;
            basePositions[applied] = target.localPosition;
            baseRotations[applied] = target.localRotation;
            applied++;
        }

        if (applied == 0)
            return;

        SortTargetsByHeight(applied);
        ApplyShaderPhaseOffsets(applied);

        float heightRange = Mathf.Max(0.0001f, maxY - minY);
        for (int i = 0; i < applied; i++)
        {
            if (i < lockBottomSegments)
            {
                motionWeights[i] = 0f;
                continue;
            }

            float bottom01 = Mathf.Clamp01((rootLocalBottoms[i] - minY) / heightRange);
            float center01 = Mathf.Clamp01((rootLocalHeights[i] - minY) / heightRange);
            if (bottom01 <= baseMotionFloor)
            {
                motionWeights[i] = 0f;
                continue;
            }

            float normalizedHeight = Mathf.InverseLerp(baseMotionFloor, 1f, center01);
            float smoothHeight = Mathf.SmoothStep(0f, 1f, normalizedHeight);
            motionWeights[i] = Mathf.Pow(smoothHeight, heightExponent);
        }
    }

    private void SortTargetsByHeight(int length)
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = i + 1; j < length; j++)
            {
                if (rootLocalHeights[i] > rootLocalHeights[j])
                {
                    SpriteRenderer tempRenderer = renderers[i];
                    renderers[i] = renderers[j];
                    renderers[j] = tempRenderer;

                    float tempH = rootLocalHeights[i];
                    rootLocalHeights[i] = rootLocalHeights[j];
                    rootLocalHeights[j] = tempH;

                    float tempBottom = rootLocalBottoms[i];
                    rootLocalBottoms[i] = rootLocalBottoms[j];
                    rootLocalBottoms[j] = tempBottom;

                    float tempTop = rootLocalTops[i];
                    rootLocalTops[i] = rootLocalTops[j];
                    rootLocalTops[j] = tempTop;

                    Transform tempT = targets[i];
                    targets[i] = targets[j];
                    targets[j] = tempT;

                    Vector3 tempP = basePositions[i];
                    basePositions[i] = basePositions[j];
                    basePositions[j] = tempP;

                    Quaternion tempR = baseRotations[i];
                    baseRotations[i] = baseRotations[j];
                    baseRotations[j] = tempR;
                }
            }
        }
    }

    private void Apply(float time)
    {
        if (targets == null || targets.Length == 0)
        {
            CacheTargets();
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            Transform target = targets[i];
            if (!target)
                continue;

            float weight = motionWeights[i];
            if (weight <= 0.0001f)
            {
                target.localRotation = baseRotations[i];
                target.localPosition = basePositions[i];
                continue;
            }

            float phase = rootLocalHeights[i] * 3.13f + i * 0.41f;
            float wind = Mathf.Sin(time * swaySpeed + phase) + Mathf.Sin(time * swaySpeed * 1.61f + phase * 0.7f) * 0.35f;

            target.localRotation = baseRotations[i] * Quaternion.Euler(0f, 0f, wind * swayAngle * weight * rotationInfluence);
            target.localPosition = basePositions[i] + new Vector3(wind * horizontalDrift * weight * driftScale, 0f, 0f);
        }
    }

    private void GetRendererRootYBounds(SpriteRenderer spriteRenderer, out float bottomY, out float topY)
    {
        Bounds bounds = spriteRenderer.bounds;
        bottomY = float.PositiveInfinity;
        topY = float.NegativeInfinity;

        AddRootY(bounds.min.x, bounds.min.y, bounds.min.z, ref bottomY, ref topY);
        AddRootY(bounds.min.x, bounds.max.y, bounds.min.z, ref bottomY, ref topY);
        AddRootY(bounds.max.x, bounds.min.y, bounds.min.z, ref bottomY, ref topY);
        AddRootY(bounds.max.x, bounds.max.y, bounds.min.z, ref bottomY, ref topY);
    }

    private void AddRootY(float x, float y, float z, ref float bottomY, ref float topY)
    {
        float localY = transform.InverseTransformPoint(new Vector3(x, y, z)).y;
        bottomY = Mathf.Min(bottomY, localY);
        topY = Mathf.Max(topY, localY);
    }

    private void ApplyShaderPhaseOffsets(int length)
    {
        if (materialBlock == null)
            materialBlock = new MaterialPropertyBlock();

        for (int i = 0; i < length; i++)
        {
            SpriteRenderer spriteRenderer = renderers[i];
            if (!spriteRenderer)
                continue;

            spriteRenderer.GetPropertyBlock(materialBlock);
            materialBlock.SetFloat(PhaseOffsetId, rootLocalHeights[i] * 2.17f + i * 1.31f);
            spriteRenderer.SetPropertyBlock(materialBlock);
        }
    }

    private void RestoreBaseTransforms()
    {
        if (targets == null || basePositions == null || baseRotations == null)
            return;

        int count = targets.Length;
        if (basePositions.Length < count) count = basePositions.Length;
        if (baseRotations.Length < count) count = baseRotations.Length;
        for (int i = 0; i < count; i++)
        {
            if (!targets[i])
                continue;

            targets[i].localPosition = basePositions[i];
            targets[i].localRotation = baseRotations[i];
        }
    }
}
