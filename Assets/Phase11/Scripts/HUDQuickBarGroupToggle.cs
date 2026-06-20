using UnityEngine;

[DisallowMultipleComponent]
public sealed class HUDQuickBarGroupToggle : MonoBehaviour
{
    [SerializeField] private GameObject[] targetsToToggle = new GameObject[0];
    [SerializeField] private Camera eventCamera;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        ToggleTargets();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || !IsPointerOverSprite())
        {
            return;
        }

        ToggleTargets();
    }

    public void SetTargets(GameObject[] targets)
    {
        targetsToToggle = targets ?? new GameObject[0];
    }

    public void ToggleTargets()
    {
        if (targetsToToggle == null || targetsToToggle.Length == 0)
        {
            return;
        }

        bool shouldShow = !targetsToToggle[0].activeSelf;
        for (int i = 0; i < targetsToToggle.Length; i++)
        {
            if (targetsToToggle[i] != null)
            {
                targetsToToggle[i].SetActive(shouldShow);
            }
        }
    }

    private bool IsPointerOverSprite()
    {
        if (spriteRenderer == null)
        {
            return false;
        }

        Camera cameraToUse = eventCamera != null ? eventCamera : Camera.main;
        if (cameraToUse == null)
        {
            return false;
        }

        Vector3 worldPoint = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        worldPoint.z = spriteRenderer.bounds.center.z;
        return spriteRenderer.bounds.Contains(worldPoint);
    }
}
