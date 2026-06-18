using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class DepthSortable : MonoBehaviour
{
    private static readonly List<DepthSortable> ActiveSortables = new List<DepthSortable>();

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Transform sortPoint;
    [SerializeField] private float manualZOffset;
    [SerializeField] private int manualOrderOffset;
    [SerializeField] private bool staticOrder;
    [SerializeField] private int staticSortingOrder = 1000;

    public static IReadOnlyList<DepthSortable> RegisteredSortables
    {
        get { return ActiveSortables; }
    }

    public Renderer[] Renderers
    {
        get { return renderers; }
    }

    public Transform SortPoint
    {
        get { return sortPoint != null ? sortPoint : transform; }
    }

    public float SortZ
    {
        get { return SortPoint.position.z + manualZOffset; }
    }

    public int ManualOrderOffset
    {
        get { return manualOrderOffset; }
    }

    public bool StaticOrder
    {
        get { return staticOrder; }
    }

    public int StaticSortingOrder
    {
        get { return staticSortingOrder; }
    }

    private void OnEnable()
    {
        if (!ActiveSortables.Contains(this))
        {
            ActiveSortables.Add(this);
        }

        EnsureRenderers();
    }

    private void OnDisable()
    {
        ActiveSortables.Remove(this);
    }

    private void Reset()
    {
        EnsureRenderers();
    }

    private void OnValidate()
    {
        EnsureRenderers();
    }

    public void Configure(
        Renderer[] targetRenderers,
        Transform targetSortPoint,
        float targetManualZOffset,
        int targetManualOrderOffset,
        bool targetStaticOrder,
        int targetStaticSortingOrder)
    {
        renderers = targetRenderers;
        sortPoint = targetSortPoint;
        manualZOffset = targetManualZOffset;
        manualOrderOffset = targetManualOrderOffset;
        staticOrder = targetStaticOrder;
        staticSortingOrder = targetStaticSortingOrder;
        EnsureRenderers();
    }

    public void ApplySortingOrder(int sortingOrder)
    {
        EnsureRenderers();

        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer target = renderers[i];
            if (target == null)
            {
                continue;
            }

            if (target.sortingOrder != sortingOrder)
            {
                target.sortingOrder = sortingOrder;
            }
        }
    }

    private void EnsureRenderers()
    {
        if (HasAnyRenderer())
        {
            return;
        }

        renderers = GetComponentsInChildren<Renderer>(true);
    }

    private bool HasAnyRenderer()
    {
        if (renderers == null || renderers.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                return true;
            }
        }

        return false;
    }
}
