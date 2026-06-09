using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 45f, -26f);

    private Camera cam;
    private Quaternion fixedRotation;
    private float fixedOrthographicSize;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        fixedRotation = transform.rotation;
        if (target != null && offset == Vector3.zero)
        {
            offset = new Vector3(0f, transform.position.y - target.position.y, -26f);
        }

        if (cam != null)
        {
            fixedOrthographicSize = cam.orthographicSize;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = fixedRotation;

        if (cam != null)
        {
            cam.orthographicSize = fixedOrthographicSize;
        }
    }
}
