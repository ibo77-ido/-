using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Phase9DepthSortingBinder
{
    private const string ScenePath = "Assets/Phase9/Scenes/SampleScene.unity";
    private const string RequestPath = "Library/Phase9DepthSortingBinder.request";
    private const string ManagerName = "DepthSortManager";
    private const string SortPointName = "SortPoint";
    private const int BaseOrder = 10000;
    private const float SortScale = 1000f;
    private const int StaticBaseOrder = 1000;

    private static readonly string[] RegionNames =
    {
        "\u8ba2\u5355\u533a",
        "\u91c9\u6599\u533a",
        "\u539f\u6599\u533a",
        "\u4ed3\u5e93\u533a",
        "\u5668\u578b\u533a",
        "\u70e7\u5236\u533a"
    };

    private static readonly string[] DynamicSpriteNames =
    {
        "V_Willow_01",
        "V_Willow_02"
    };

    static Phase9DepthSortingBinder()
    {
        EditorApplication.delayCall += RunIfRequested;
    }

    [MenuItem("Phase9/Depth Sorting/Bind SampleScene Sortables")]
    public static void BindSampleSceneSortables()
    {
        Scene scene = OpenTargetScene();

        DepthSortManager manager = EnsureManager(scene);
        ConfigureManager(manager);

        List<string> missing = new List<string>();
        BindPlayer(scene, missing);
        BindRegions(scene, missing);
        BindDynamicSprites(scene, missing);

        manager.RefreshAll();

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        if (missing.Count > 0)
        {
            Debug.LogWarning("[Phase9DepthSortingBinder] Bound depth sorting with missing objects: " + string.Join(", ", missing));
        }
        else
        {
            Debug.Log("[Phase9DepthSortingBinder] Bound depth sorting for player, regions, and willows.");
        }
    }

    public static void RunFromCommandLine()
    {
        BindSampleSceneSortables();
        EditorApplication.Exit(0);
    }

    [MenuItem("Phase9/Depth Sorting/Request Bind On Reload")]
    public static void RequestBindOnReload()
    {
        System.IO.File.WriteAllText(RequestPath, "bind");
        AssetDatabase.Refresh();
        Debug.Log("[Phase9DepthSortingBinder] Bind request written. It will run after the next editor reload.");
    }

    private static void RunIfRequested()
    {
        if (!System.IO.File.Exists(RequestPath))
        {
            return;
        }

        try
        {
            System.IO.File.Delete(RequestPath);
        }
        catch (System.IO.IOException)
        {
            Debug.LogWarning("[Phase9DepthSortingBinder] Request marker could not be deleted; continuing.");
        }

        BindSampleSceneSortables();
    }

    private static Scene OpenTargetScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.IsValid() && active.path == ScenePath)
        {
            return active;
        }

        return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static DepthSortManager EnsureManager(Scene scene)
    {
        GameObject managerObject = FindSceneObject(scene, ManagerName);
        if (managerObject == null)
        {
            managerObject = new GameObject(ManagerName);
            SceneManager.MoveGameObjectToScene(managerObject, scene);
            Undo.RegisterCreatedObjectUndo(managerObject, "Create DepthSortManager");
        }

        DepthSortManager manager = managerObject.GetComponent<DepthSortManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<DepthSortManager>(managerObject);
        }

        return manager;
    }

    private static void ConfigureManager(DepthSortManager manager)
    {
        manager.BaseOrder = BaseOrder;
        manager.SortScale = SortScale;
    }

    private static void BindPlayer(Scene scene, List<string> missing)
    {
        GameObject player = FindSceneObject(scene, "\u5973\u4e3b");
        if (player == null)
        {
            missing.Add("\u5973\u4e3b");
            return;
        }

        Transform visualRoot = player.transform.Find("VisualRoot");
        Renderer[] renderers = visualRoot != null
            ? visualRoot.GetComponentsInChildren<Renderer>(true)
            : player.GetComponentsInChildren<Renderer>(true);

        DepthSortable sortable = EnsureSortable(player);
        sortable.Configure(renderers, player.transform, 0f, 0, false, StaticBaseOrder);
        EditorUtility.SetDirty(sortable);
    }

    private static void BindRegions(Scene scene, List<string> missing)
    {
        for (int i = 0; i < RegionNames.Length; i++)
        {
            GameObject region = FindSceneObject(scene, RegionNames[i]);
            if (region == null)
            {
                missing.Add(RegionNames[i]);
                continue;
            }

            string baseName = RegionNames[i] + "_Base";
            string occluderName = RegionNames[i] + "_Occluder";
            Transform baseLayer = region.transform.Find(baseName);
            Transform occluderLayer = region.transform.Find(occluderName);

            Renderer[] renderers;
            if (occluderLayer != null)
            {
                SetFixedSortingOrder(baseLayer, StaticBaseOrder);
                DisableRootRenderer(region);
                renderers = occluderLayer.GetComponentsInChildren<Renderer>(true);
            }
            else
            {
                renderers = region.GetComponentsInChildren<Renderer>(true);
            }

            Transform sortPoint = EnsureBoundsSortPoint(
                occluderLayer != null ? occluderLayer : region.transform,
                renderers,
                occluderLayer == null);

            DepthSortable sortable = EnsureSortable(region);
            sortable.Configure(renderers, sortPoint, 0f, 0, false, StaticBaseOrder);
            EditorUtility.SetDirty(sortable);
        }
    }

    private static void BindDynamicSprites(Scene scene, List<string> missing)
    {
        for (int i = 0; i < DynamicSpriteNames.Length; i++)
        {
            GameObject obj = FindSceneObject(scene, DynamicSpriteNames[i]);
            if (obj == null)
            {
                missing.Add(DynamicSpriteNames[i]);
                continue;
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            Transform sortPoint = EnsureBoundsSortPoint(obj.transform, renderers, true);

            DepthSortable sortable = EnsureSortable(obj);
            sortable.Configure(renderers, sortPoint, 0f, 0, false, StaticBaseOrder);
            EditorUtility.SetDirty(sortable);
        }
    }

    private static Transform EnsureBoundsSortPoint(Transform owner, Renderer[] renderers, bool useFrontEdge)
    {
        Transform existing = owner.Find(SortPointName);
        GameObject sortPointObject;
        if (existing != null)
        {
            EnsureSortPointGuide(existing.gameObject, owner, renderers);
            return existing;
        }

        sortPointObject = new GameObject(SortPointName);
        Undo.RegisterCreatedObjectUndo(sortPointObject, "Create SortPoint");
        sortPointObject.transform.SetParent(owner, false);

        Bounds bounds;
        if (TryGetBounds(renderers, out bounds))
        {
            Vector3 worldPosition = owner.position;
            worldPosition.z = useFrontEdge ? bounds.min.z : bounds.center.z;
            sortPointObject.transform.position = worldPosition;
        }
        else
        {
            sortPointObject.transform.localPosition = Vector3.zero;
        }

        EditorUtility.SetDirty(sortPointObject);
        EnsureSortPointGuide(sortPointObject, owner, renderers);
        return sortPointObject.transform;
    }

    private static void EnsureSortPointGuide(GameObject sortPointObject, Transform owner, Renderer[] renderers)
    {
        float halfWidth = 0.5f;
        Bounds bounds;
        if (TryGetBounds(renderers, out bounds))
        {
            halfWidth = Mathf.Max(0.5f, bounds.extents.x);
        }

        System.Type guideType = System.Type.GetType("DepthSortPointGuide, Assembly-CSharp");
        if (guideType == null || !typeof(Component).IsAssignableFrom(guideType))
        {
            return;
        }

        Component guide = sortPointObject.GetComponent(guideType);
        if (guide == null)
        {
            guide = Undo.AddComponent(sortPointObject, guideType);
        }

        System.Reflection.MethodInfo configure = guideType.GetMethod("Configure");
        if (configure != null)
        {
            configure.Invoke(guide, new object[] { owner, halfWidth });
        }

        EditorUtility.SetDirty(guide);
    }

    private static void SetFixedSortingOrder(Transform root, int sortingOrder)
    {
        if (root == null)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            renderers[i].sortingOrder = sortingOrder;
            EditorUtility.SetDirty(renderers[i]);
        }
    }

    private static void DisableRootRenderer(GameObject root)
    {
        Renderer renderer = root.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.enabled = false;
        EditorUtility.SetDirty(renderer);
    }

    private static bool TryGetBounds(Renderer[] renderers, out Bounds bounds)
    {
        bounds = new Bounds();
        bool initialized = false;

        if (renderers == null)
        {
            return false;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (!initialized)
            {
                bounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return initialized;
    }

    private static DepthSortable EnsureSortable(GameObject obj)
    {
        DepthSortable sortable = obj.GetComponent<DepthSortable>();
        if (sortable == null)
        {
            sortable = Undo.AddComponent<DepthSortable>(obj);
        }

        return sortable;
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindChildRecursive(roots[i].transform, objectName);
            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform current, string objectName)
    {
        if (current.name == objectName)
        {
            return current;
        }

        for (int i = 0; i < current.childCount; i++)
        {
            Transform found = FindChildRecursive(current.GetChild(i), objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
