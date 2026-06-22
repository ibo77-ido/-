using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class CloudFogDriftController : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxChildren = 5;
    [SerializeField, Range(0f, 2f)] private float horizontalDrift = 0.35f;
    [SerializeField, Range(0f, 1f)] private float verticalFloat = 0.08f;
    [SerializeField, Range(0f, 3f)] private float driftSpeed = 0.28f;
    [Range(0f, 1f)] public float cloudOpacity = 1f;
    [Range(0f, 0.5f)] public float alphaPulse = 0.08f;
    [SerializeField] private bool includeRootRenderer = true;

    private Transform[] targets = new Transform[0];
    private SpriteRenderer[] renderers = new SpriteRenderer[0];
    private Vector3[] basePositions = new Vector3[0];
    private Color[] baseColors = new Color[0];

    private void OnEnable()
    {
        CacheTargets(Application.isPlaying);
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (UnityEditor.BuildPipeline.isBuildingPlayer)
        {
            return;
        }
#endif

        CacheTargets(Application.isPlaying);
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

    public void CacheTargets()
    {
        CacheTargets(Application.isPlaying);
    }

    private void CacheTargets(bool createMissingChildren)
    {
        if (createMissingChildren)
        {
            EnsureCloudLayerChildren();
        }

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        int eligibleCount = 0;
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] && (includeRootRenderer || allRenderers[i].transform != transform))
            {
                eligibleCount++;
            }
        }

        int count = Mathf.Min(maxChildren, eligibleCount);
        targets = new Transform[count];
        renderers = new SpriteRenderer[count];
        basePositions = new Vector3[count];
        baseColors = new Color[count];

        int applied = 0;
        for (int i = 0; i < allRenderers.Length && applied < count; i++)
        {
            SpriteRenderer renderer = allRenderers[i];
            if (!renderer || (!includeRootRenderer && renderer.transform == transform))
            {
                continue;
            }

            targets[applied] = renderer.transform;
            renderers[applied] = renderer;
            basePositions[applied] = renderer.transform.localPosition;
            baseColors[applied] = renderer.color;
            applied++;
        }
    }

    private void EnsureCloudLayerChildren()
    {
#if UNITY_EDITOR
        if (GetComponentsInChildren<SpriteRenderer>(true).Length > 0)
        {
            return;
        }

        string[] spriteGuids =
        {
            "bd9d26868c0aecf48a59ed28dd946468",
            "623a0a82901c6e746846c769b5bb2de8",
            "0a1fd6d672314da4eb2840933603b4b0",
            "623a0a82901c6e746846c769b5bb2de8",
            "0a1fd6d672314da4eb2840933603b4b0"
        };

        Vector3[] positions =
        {
            new Vector3(-1.8f, 0.2f, 0f),
            new Vector3(-0.6f, 0.55f, 0f),
            new Vector3(0.8f, 0.1f, 0f),
            new Vector3(1.9f, 0.45f, 0f),
            new Vector3(-2.8f, 0.65f, 0f)
        };

        float[] scales = { 0.55f, 0.42f, 0.36f, 0.32f, 0.28f };
        float[] alphas = { 0.7f, 0.62f, 0.52f, 0.44f, 0.38f };
        Material fogMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Phase9/Materials/Mat_Cloud_Fog_Drift.mat");

        for (int i = 0; i < Mathf.Min(maxChildren, spriteGuids.Length); i++)
        {
            string spritePath = UnityEditor.AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
            Sprite sprite = string.IsNullOrEmpty(spritePath) ? null : UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (!sprite)
            {
                continue;
            }

            GameObject layer = new GameObject("Cloud_Fog_Layer_" + (i + 1).ToString("00"));
            layer.transform.SetParent(transform, false);
            layer.transform.localPosition = positions[i];
            layer.transform.localRotation = Quaternion.identity;
            layer.transform.localScale = Vector3.one * scales[i];

            SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(1f, 1f, 1f, alphas[i]);
            renderer.sortingOrder = 4 + i;
            if (fogMaterial)
            {
                renderer.sharedMaterial = fogMaterial;
            }

            UnityEditor.Undo.RegisterCreatedObjectUndo(layer, "Create cloud fog layer");
        }

        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void Apply(float time)
    {
        if (targets == null || targets.Length == 0)
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

            float layer = i / Mathf.Max(1f, targets.Length - 1f);
            float phase = i * 1.83f;
            float speed = driftSpeed * Mathf.Lerp(0.72f, 1.28f, layer);
            float x = Mathf.Sin(time * speed + phase) * horizontalDrift * Mathf.Lerp(0.7f, 1.2f, layer);
            float y = Mathf.Sin(time * speed * 1.37f + phase + 0.65f) * verticalFloat * Mathf.Lerp(0.6f, 1.1f, layer);
            target.localPosition = basePositions[i] + new Vector3(x, y, 0f);

            SpriteRenderer renderer = renderers[i];
            if (renderer)
            {
                Color color = baseColors[i];
                float alpha = 1f - alphaPulse + Mathf.Sin(time * speed * 0.9f + phase) * alphaPulse;
                color.a = Mathf.Clamp01(baseColors[i].a * cloudOpacity * alpha);
                renderer.color = color;
            }
        }
    }
}
