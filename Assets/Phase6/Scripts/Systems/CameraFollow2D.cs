using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform boundsRoot;

    [Header("Settings")]
    [SerializeField, Min(0.1f)] private float smoothSpeed = 8f;
    [SerializeField] private Vector2 framingOffset = new Vector2(0f, 0.45f);
    [SerializeField, Min(0.1f)] private float orthographicSize = 2.2f;
    [SerializeField] private bool clampToBounds = true;
    [SerializeField] private Vector2 boundsPadding = new Vector2(0.25f, 0.25f);

    private Camera cam;
    private Quaternion fixedRotation;
    private Bounds mapBounds;
    private bool hasMapBounds;
    private float fixedZ;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        fixedRotation = transform.rotation;
        fixedZ = transform.position.z;
        ResolveReferences();
        ConfigureCamera();
        RebuildBounds();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            ResolveReferences();
            if (target == null)
            {
                return;
            }
        }

        if (!hasMapBounds && boundsRoot != null)
        {
            RebuildBounds();
        }

        Vector3 targetPosition = GetDesiredPosition();
        transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        transform.rotation = fixedRotation;
        ConfigureCamera();
    }

    private void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = GetDesiredPosition();
        }
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 desiredPosition = new Vector3(
            target.position.x + framingOffset.x,
            target.position.y + framingOffset.y,
            fixedZ);

        return ClampToMapBounds(desiredPosition);
    }

    private void ConfigureCamera()
    {
        if (cam == null)
        {
            return;
        }

        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.allowDynamicResolution = false;
    }

    private void ResolveReferences()
    {
        if (target == null)
        {
            GameObject heroine = GameObject.Find("\u5973\u4E3B");
            if (heroine != null)
            {
                target = heroine.transform;
            }
        }

        if (target == null)
        {
            PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
            if (playerCharacter != null)
            {
                target = playerCharacter.transform;
            }
        }

        if (boundsRoot == null)
        {
            GameObject root = GameObject.Find("\u9759\u6001\u5C42");
            if (root == null)
            {
                root = GameObject.Find("_MapRoot");
            }

            if (root == null)
            {
                root = GameObject.Find("MapRoot");
            }

            if (root != null)
            {
                boundsRoot = root.transform;
            }
        }
    }

    private void RebuildBounds()
    {
        hasMapBounds = false;
        if (boundsRoot == null)
        {
            return;
        }

        SpriteRenderer[] renderers = boundsRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (!renderer || renderer.transform == target || renderer.GetComponentInParent<CloudFogDriftController>() != null)
            {
                continue;
            }

            if (!hasMapBounds)
            {
                mapBounds = renderer.bounds;
                hasMapBounds = true;
            }
            else
            {
                mapBounds.Encapsulate(renderer.bounds);
            }
        }
    }

    private Vector3 ClampToMapBounds(Vector3 desiredPosition)
    {
        if (!clampToBounds || !hasMapBounds || cam == null)
        {
            return desiredPosition;
        }

        float halfHeight = orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float minX = mapBounds.min.x + halfWidth + boundsPadding.x;
        float maxX = mapBounds.max.x - halfWidth - boundsPadding.x;
        float minY = mapBounds.min.y + halfHeight + boundsPadding.y;
        float maxY = mapBounds.max.y - halfHeight - boundsPadding.y;

        desiredPosition.x = minX > maxX ? mapBounds.center.x : Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = minY > maxY ? mapBounds.center.y : Mathf.Clamp(desiredPosition.y, minY, maxY);
        return desiredPosition;
    }
}
