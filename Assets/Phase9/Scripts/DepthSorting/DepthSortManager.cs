using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class DepthSortManager : MonoBehaviour
{
    [SerializeField] private int baseOrder = 10000;
    [SerializeField] private float sortScale = 1000f;
    [SerializeField] private bool refreshInEditMode = true;
    [SerializeField] private bool autoDiscoverWhenEmpty = true;

    private readonly List<DepthSortable> discoveredSortables = new List<DepthSortable>();

    public int BaseOrder
    {
        get { return baseOrder; }
        set { baseOrder = value; }
    }

    public float SortScale
    {
        get { return sortScale; }
        set { sortScale = value; }
    }

    private void OnEnable()
    {
        RefreshAll();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying && !refreshInEditMode)
        {
            return;
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        IReadOnlyList<DepthSortable> sortables = DepthSortable.RegisteredSortables;
        if (sortables.Count == 0 && autoDiscoverWhenEmpty)
        {
            DiscoverSortables();
            sortables = discoveredSortables;
        }

        for (int i = 0; i < sortables.Count; i++)
        {
            DepthSortable sortable = sortables[i];
            if (sortable == null || !sortable.isActiveAndEnabled)
            {
                continue;
            }

            int order = sortable.StaticOrder
                ? sortable.StaticSortingOrder
                : baseOrder + Mathf.RoundToInt(-sortable.SortZ * sortScale) + sortable.ManualOrderOffset;

            sortable.ApplySortingOrder(order);
        }
    }

    private void DiscoverSortables()
    {
        discoveredSortables.Clear();

#if UNITY_2023_1_OR_NEWER
        DepthSortable[] sortables = FindObjectsByType<DepthSortable>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
#else
        DepthSortable[] sortables = FindObjectsOfType<DepthSortable>(true);
#endif

        for (int i = 0; i < sortables.Length; i++)
        {
            if (sortables[i] != null)
            {
                discoveredSortables.Add(sortables[i]);
            }
        }
    }
}
