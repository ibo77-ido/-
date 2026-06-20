using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GlazeTriangleMixer : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform triangleArea;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float maxElementValue = 0.02f;
    [SerializeField] private Vector2 defaultNormalizedPosition = new Vector2(0.5f, 0.35f);

    public event Action<GlazeInput> MixChanged;

    public float CopperWeight { get; private set; }
    public float IronWeight { get; private set; }
    public float CobaltWeight { get; private set; }
    public GlazeInput CurrentInput { get; private set; }

    private void Awake()
    {
        AutoBindReferences();
    }

    private void OnEnable()
    {
        AutoBindReferences();
        SetNormalizedPosition(defaultNormalizedPosition, true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ApplyPointerPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ApplyPointerPosition(eventData);
    }

    public void ResetMixer()
    {
        SetNormalizedPosition(defaultNormalizedPosition, true);
    }

    public void SetMix(float copper, float iron, float cobalt)
    {
        float sum = Mathf.Max(0.0001f, copper + iron + cobalt);
        float copperWeight = Mathf.Clamp01(copper / sum);
        float ironWeight = Mathf.Clamp01(iron / sum);
        float cobaltWeight = Mathf.Clamp01(cobalt / sum);
        float normalizedSum = Mathf.Max(0.0001f, copperWeight + ironWeight + cobaltWeight);
        copperWeight /= normalizedSum;
        ironWeight /= normalizedSum;
        cobaltWeight /= normalizedSum;

        if (triangleArea == null)
        {
            AutoBindReferences();
            if (triangleArea == null)
            {
                return;
            }
        }

        Rect rect = triangleArea.rect;
        Vector2 copperVertex = new Vector2(rect.center.x, rect.yMax);
        Vector2 ironVertex = new Vector2(rect.xMin, rect.yMin);
        Vector2 cobaltVertex = new Vector2(rect.xMax, rect.yMin);
        Vector2 localPoint = copperVertex * copperWeight + ironVertex * ironWeight + cobaltVertex * cobaltWeight;
        ApplyLocalPoint(localPoint, true);
    }

    private void AutoBindReferences()
    {
        if (handle == null)
        {
            handle = transform as RectTransform;
        }

        if (triangleArea == null && transform.parent != null)
        {
            triangleArea = transform.parent as RectTransform;
        }
    }

    private void ApplyPointerPosition(PointerEventData eventData)
    {
        if (triangleArea == null)
        {
            AutoBindReferences();
            if (triangleArea == null)
            {
                return;
            }
        }

        Camera eventCamera = eventData.pressEventCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(triangleArea, eventData.position, eventCamera, out Vector2 localPoint))
        {
            return;
        }

        ApplyLocalPoint(localPoint, true);
    }

    private void SetNormalizedPosition(Vector2 normalizedPosition, bool notify)
    {
        if (triangleArea == null)
        {
            AutoBindReferences();
            if (triangleArea == null)
            {
                return;
            }
        }

        Rect rect = triangleArea.rect;
        Vector2 localPoint = new Vector2(
            Mathf.Lerp(rect.xMin, rect.xMax, Mathf.Clamp01(normalizedPosition.x)),
            Mathf.Lerp(rect.yMin, rect.yMax, Mathf.Clamp01(normalizedPosition.y))
        );

        ApplyLocalPoint(localPoint, notify);
    }

    private void ApplyLocalPoint(Vector2 localPoint, bool notify)
    {
        Rect rect = triangleArea.rect;
        Vector2 copperVertex = new Vector2(rect.center.x, rect.yMax);
        Vector2 ironVertex = new Vector2(rect.xMin, rect.yMin);
        Vector2 cobaltVertex = new Vector2(rect.xMax, rect.yMin);
        Vector2 clampedPoint = ClampPointToTriangle(localPoint, copperVertex, ironVertex, cobaltVertex);

        if (handle != null)
        {
            handle.anchoredPosition = clampedPoint;
        }

        CalculateBarycentric(clampedPoint, copperVertex, ironVertex, cobaltVertex, out float copper, out float iron, out float cobalt);
        CopperWeight = copper;
        IronWeight = iron;
        CobaltWeight = cobalt;
        CurrentInput = new GlazeInput
        {
            copper = CopperWeight * maxElementValue,
            iron = IronWeight * maxElementValue,
            cobalt = CobaltWeight * maxElementValue
        };

        if (notify)
        {
            MixChanged?.Invoke(CurrentInput);
        }
    }

    private static Vector2 ClampPointToTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        CalculateBarycentric(point, a, b, c, out float wa, out float wb, out float wc);
        if (wa >= 0f && wb >= 0f && wc >= 0f)
        {
            return point;
        }

        Vector2 closestAB = ClosestPointOnSegment(point, a, b);
        Vector2 closestBC = ClosestPointOnSegment(point, b, c);
        Vector2 closestCA = ClosestPointOnSegment(point, c, a);

        float distAB = (point - closestAB).sqrMagnitude;
        float distBC = (point - closestBC).sqrMagnitude;
        float distCA = (point - closestCA).sqrMagnitude;

        if (distAB <= distBC && distAB <= distCA) return closestAB;
        if (distBC <= distAB && distBC <= distCA) return closestBC;
        return closestCA;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        return a + Mathf.Clamp01(t) * ab;
    }

    private static void CalculateBarycentric(Vector2 point, Vector2 a, Vector2 b, Vector2 c, out float wa, out float wb, out float wc)
    {
        Vector2 v0 = b - a;
        Vector2 v1 = c - a;
        Vector2 v2 = point - a;

        float d00 = Vector2.Dot(v0, v0);
        float d01 = Vector2.Dot(v0, v1);
        float d11 = Vector2.Dot(v1, v1);
        float d20 = Vector2.Dot(v2, v0);
        float d21 = Vector2.Dot(v2, v1);
        float denominator = d00 * d11 - d01 * d01;

        if (Mathf.Approximately(denominator, 0f))
        {
            wa = 1f;
            wb = 0f;
            wc = 0f;
            return;
        }

        wb = (d11 * d20 - d01 * d21) / denominator;
        wc = (d00 * d21 - d01 * d20) / denominator;
        wa = 1f - wb - wc;

        wa = Mathf.Clamp01(wa);
        wb = Mathf.Clamp01(wb);
        wc = Mathf.Clamp01(wc);
        float sum = wa + wb + wc;
        if (sum <= 0.0001f)
        {
            wa = 1f;
            wb = 0f;
            wc = 0f;
            return;
        }

        wa /= sum;
        wb /= sum;
        wc /= sum;
    }
}
