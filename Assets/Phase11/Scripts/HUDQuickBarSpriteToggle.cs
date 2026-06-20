using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class HUDQuickBarSpriteToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject[] additionalTargets = new GameObject[0];
    [SerializeField] private bool hideOnStart;
    [SerializeField] private Camera eventCamera;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (hideOnStart)
        {
            SetTargetsActive(false, false);
        }
    }

    private void OnMouseDown()
    {
        Toggle();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || !IsPointerOverSprite())
        {
            return;
        }

        Toggle();
    }

    public void SetTarget(GameObject nextTarget)
    {
        target = nextTarget;
    }

    public void SetAdditionalTargets(GameObject[] nextTargets)
    {
        additionalTargets = nextTargets ?? new GameObject[0];
    }

    public void Toggle()
    {
        if (target == null)
        {
            return;
        }

        SetTargetsActive(!target.activeSelf);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    private void SetTargetsActive(bool active)
    {
        SetTargetsActive(active, true);
    }

    private void SetTargetsActive(bool active, bool playSfx)
    {
        if (playSfx)
        {
            SfxPlayer.Play(active ? SfxId.PanelOpen : SfxId.PanelClose);
        }

        if (target != null)
        {
            target.SetActive(active);
        }

        if (additionalTargets == null)
        {
            return;
        }

        for (int i = 0; i < additionalTargets.Length; i++)
        {
            if (additionalTargets[i] != null)
            {
                additionalTargets[i].SetActive(active);
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
