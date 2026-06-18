using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class DepthSortPointGuide : MonoBehaviour
{
    [SerializeField] private Transform owner;
    [SerializeField] private float halfWidth = 1f;
    [SerializeField] private Color lineColor = new Color(1f, 0.72f, 0.12f, 1f);
    [SerializeField] private Color pointColor = new Color(0.1f, 0.8f, 1f, 1f);

    public void Configure(Transform targetOwner, float targetHalfWidth)
    {
        owner = targetOwner;
        halfWidth = Mathf.Max(0.25f, targetHalfWidth);
    }

    private void OnDrawGizmos()
    {
        DrawGuide(false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGuide(true);
    }

    private void DrawGuide(bool selected)
    {
        float width = Mathf.Max(0.25f, halfWidth);
        Vector3 center = transform.position;
        Vector3 left = center + Vector3.left * width;
        Vector3 right = center + Vector3.right * width;

        Gizmos.color = selected ? Color.yellow : lineColor;
        Gizmos.DrawLine(left, right);

        Gizmos.color = selected ? Color.cyan : pointColor;
        Gizmos.DrawSphere(center, selected ? 0.08f : 0.045f);

        if (owner == null)
        {
            return;
        }

        Gizmos.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0.35f);
        Gizmos.DrawLine(center, owner.position);
    }
}
