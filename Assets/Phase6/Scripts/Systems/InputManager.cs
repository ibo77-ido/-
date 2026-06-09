using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Phase6GameManager gameManager;
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private InteractionController interactionController;

    private void Update()
    {
        if (gameManager == null || playerCharacter == null) return;
        if (targetCamera == null) targetCamera = Camera.main;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (interactionController != null && gameManager.CanInteract())
            {
                interactionController.TryInteract();
            }
            return;
        }

        if (!gameManager.CanMove()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector3 targetPoint;
            if (TryGetPointerWorldPosition(Input.mousePosition, out targetPoint))
            {
                playerCharacter.SetDestination(targetPoint);
            }
        }
    }

    private bool TryGetPointerWorldPosition(Vector3 screenPosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (targetCamera == null) return false;

        Ray ray = targetCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 200f))
        {
            if (IsBlockedClick(hit.collider.transform))
            {
                return false;
            }

            worldPosition = hit.point;
            worldPosition.y = 0f;
            return true;
        }

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            worldPosition = ray.GetPoint(enter);
            worldPosition.y = 0f;
            return true;
        }

        return false;
    }

    private bool IsBlockedClick(Transform hitTransform)
    {
        Transform current = hitTransform;
        while (current != null)
        {
            if (current.name == "StaticBlockerRoot" || current.name == "WallRoot")
            {
                return true;
            }

            if (current.name.Contains("_Blocker") || current.name.Contains("Obstacle") || current.name.StartsWith("Wall_"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
