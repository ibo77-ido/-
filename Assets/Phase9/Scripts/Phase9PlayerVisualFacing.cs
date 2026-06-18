using UnityEngine;

[DisallowMultipleComponent]
public sealed class Phase9PlayerVisualFacing : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Vector3 desiredLocalEuler;
    [SerializeField] private Vector3 desiredRootEuler = new Vector3(180f, 0f, 0f);
    [SerializeField] private bool desiredFlipY = true;

    private void Awake()
    {
        ResolveVisualRoot();
        ApplyRootFacing();
        ApplyFacing();
        ApplySpriteFacing();
    }

    private void LateUpdate()
    {
        ApplyRootFacing();
        ApplyFacing();
        ApplySpriteFacing();
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
}
