using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class SpriteSwayController : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxChildren = 8;
    [SerializeField, Range(0f, 10f)] private float swayAngle = 2.5f;
    [SerializeField, Range(0f, 5f)] private float swaySpeed = 1.25f;
    [SerializeField, Range(0f, 1f)] private float horizontalDrift = 0.015f;
    [SerializeField, Range(0f, 0.3f)] private float baseMotionFloor = 0.005f;
    [SerializeField, Range(0.5f, 4f)] private float heightExponent = 2.2f;
    [SerializeField, Range(0f, 1f)] private float driftScale = 0.22f;

    private Transform[] targets = new Transform[0];
    private Vector3[] basePositions = new Vector3[0];
    private Quaternion[] baseRotations = new Quaternion[0];
    private float[] motionWeights = new float[0];
    private float[] rootLocalHeights = new float[0];

    private void OnEnable()
    {
        CacheTargets();
    }

    private void OnValidate()
    {
        CacheTargets();
        Apply(Time.realtimeSinceStartup);
    }

    private void Update()
    {
        Apply(Application.isPlaying ? Time.time : Time.realtimeSinceStartup);
    }

    public void CacheTargets()
    {
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
        targets = new Transform[count];
        basePositions = new Vector3[count];
        baseRotations = new Quaternion[count];
        motionWeights = new float[count];
        rootLocalHeights = new float[count];

        int applied = 0;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < allRenderers.Length && applied < count; i++)
        {
            Transform target = allRenderers[i].transform;
            if (!target || target == transform)
            {
                continue;
            }

            Vector3 localPosition = target.localPosition;
            float rootLocalHeight = transform.InverseTransformPoint(target.position).y;
            minY = Mathf.Min(minY, rootLocalHeight);
            maxY = Mathf.Max(maxY, rootLocalHeight);
            targets[applied] = target;
            basePositions[applied] = localPosition;
            baseRotations[applied] = target.localRotation;
            rootLocalHeights[applied] = rootLocalHeight;
            applied++;
        }

        if (applied == 0)
        {
            return;
        }

        for (int i = 0; i < applied; i++)
        {
            float normalizedHeight = Mathf.InverseLerp(minY, maxY, rootLocalHeights[i]);
            float easedHeight = Mathf.Pow(Mathf.SmoothStep(0f, 1f, normalizedHeight), heightExponent);
            motionWeights[i] = Mathf.Lerp(baseMotionFloor, 1f, easedHeight);
        }
    }

    private void Apply(float time)
    {
        if (targets == null || targets.Length == 0 || motionWeights == null || motionWeights.Length != targets.Length || rootLocalHeights == null || rootLocalHeights.Length != targets.Length)
        {
            CacheTargets();
        }

        for (int i = 0; i < targets.Length; i++)
        {
            Transform target = targets[i];
            if (!target)
            {
                continue;
            }

            float heightWeight = motionWeights[i];
            float phase = rootLocalHeights[i] * 3.13f + i * 0.41f;
            float wind = Mathf.Sin(time * swaySpeed + phase) + Mathf.Sin(time * swaySpeed * 1.61f + phase * 0.7f) * 0.35f;
            target.localRotation = baseRotations[i] * Quaternion.Euler(0f, 0f, wind * swayAngle * heightWeight);
            target.localPosition = basePositions[i] + new Vector3(wind * horizontalDrift * heightWeight * driftScale, 0f, 0f);
        }
    }
}
