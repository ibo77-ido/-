using UnityEngine;

public class WorkstationVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform artRoot;
    [SerializeField] private WorkstationConfigSO config;

    private GameObject currentVisual;
    private ScaleManager scaleManager;

    private void Start()
    {
        scaleManager = FindObjectOfType<ScaleManager>();
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (artRoot == null) return;

        // Destroy only ArtRoot children, never LogicRoot
        for (int i = artRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child = artRoot.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        if (config != null && config.defaultVisualPrefab != null)
        {
            currentVisual = Instantiate(config.defaultVisualPrefab, artRoot, false);
            currentVisual.transform.localPosition = Vector3.zero;
            currentVisual.transform.localRotation = Quaternion.identity;
            currentVisual.transform.localScale = Vector3.one;
        }
        else
        {
            currentVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentVisual.name = "Visual_Fallback";
            currentVisual.transform.SetParent(artRoot, false);
            currentVisual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            currentVisual.transform.localRotation = Quaternion.identity;
            currentVisual.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

            Collider visualCollider = currentVisual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(visualCollider);
                }
                else
                {
                    DestroyImmediate(visualCollider);
                }
            }
        }

        ApplyScale();

        Debug.Log($"[WorkstationVisualController] RefreshVisual on {gameObject.name}");
    }

    public void ApplyScale()
    {
        if (artRoot == null) return;

        float s = 1f;

        // Prefer ScaleManager's unified scale over config.scale
        if (scaleManager != null)
        {
            var scaleConfig = scaleManager.GetScaleConfig();
            if (scaleConfig != null)
            {
                s = scaleConfig.workstationScale;
            }
        }
        else if (config != null)
        {
            s = config.scale;
        }

        artRoot.localScale = new Vector3(s, s, s);
    }
}
