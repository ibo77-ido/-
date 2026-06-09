using System.Text;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class Phase6NavMeshConnectivityRepair
{
    public static string Apply()
    {
        StringBuilder report = new StringBuilder();

        ResizeWalkables();
        ConfigureNavigationAreas();
        UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        report.AppendLine("Bake=Requested");
        report.Append(CheckRoutes());
        return report.ToString();
    }

    private static void ResizeWalkables()
    {
        RestoreDesignedWalkableSizes();
        RestoreDesignedVisualSizes();
        EnsureLogicConnectors();
    }

    private static void RestoreDesignedWalkableSizes()
    {
        SetScale("OrderArea_Walkable", new Vector3(20f, 0.1f, 26f));
        SetScale("WheelArea_Walkable", new Vector3(20f, 0.1f, 26f));
        SetScale("GlazeArea_Walkable", new Vector3(24f, 0.1f, 22f));
        SetScale("StorageArea_Walkable", new Vector3(24f, 0.1f, 18f));
        SetScale("KilnArea_Walkable", new Vector3(24f, 0.1f, 26f));
        SetScale("MaterialArea_Walkable", new Vector3(24f, 0.1f, 26f));
        SetScale("MainRoad_Walkable", new Vector3(5.5f, 0.1f, 60f));
        SetScale("SecondaryRoad_Walkable", new Vector3(80f, 0.1f, 4.5f));
        SetScale("KilnBranch_Walkable", new Vector3(4f, 0.1f, 18f));
    }

    private static void RestoreDesignedVisualSizes()
    {
        SetScale("Visual_OrderArea", new Vector3(20f, 0.02f, 26f));
        SetScale("Visual_WheelArea", new Vector3(20f, 0.02f, 26f));
        SetScale("Visual_GlazeArea", new Vector3(24f, 0.02f, 22f));
        SetScale("Visual_StorageArea", new Vector3(24f, 0.02f, 18f));
        SetScale("Visual_KilnArea", new Vector3(24f, 0.02f, 26f));
        SetScale("Visual_MaterialArea", new Vector3(24f, 0.02f, 26f));
        SetScale("Visual_MainRoad", new Vector3(5.5f, 0.03f, 60f));
        SetScale("Visual_SecondaryRoad", new Vector3(80f, 0.03f, 4.5f));
        SetScale("Visual_KilnBranch", new Vector3(4f, 0.03f, 18f));
    }

    private static void EnsureLogicConnectors()
    {
        GameObject root = GameObject.Find("WalkableRoot");
        if (root == null) return;

        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Phase6/Materials/Mat_Walkable_Green.mat");
        EnsureConnector(root.transform, "Connector_Order_To_MainRoad", new Vector3(22f, 0.5f, 14.5f), new Vector3(4f, 0.1f, 8f), material);
        EnsureConnector(root.transform, "Connector_Wheel_To_MainRoad", new Vector3(22f, 0.5f, 45.5f), new Vector3(4f, 0.1f, 8f), material);
        EnsureConnector(root.transform, "Connector_Storage_To_MainRoad", new Vector3(28f, 0.5f, 10f), new Vector3(8f, 0.1f, 6f), material);
        EnsureConnector(root.transform, "Connector_Glaze_To_MainRoad", new Vector3(28f, 0.5f, 48f), new Vector3(8f, 0.1f, 6f), material);
        EnsureConnector(root.transform, "Connector_Material_To_KilnBranch", new Vector3(55f, 0.5f, 14.5f), new Vector3(8f, 0.1f, 8f), material);
        EnsureConnector(root.transform, "Connector_Kiln_To_KilnBranch", new Vector3(55f, 0.5f, 45.5f), new Vector3(8f, 0.1f, 8f), material);
        EnsureConnector(root.transform, "Connector_KilnBranch_To_SecondaryRoad", new Vector3(53.5f, 0.5f, 30f), new Vector3(5f, 0.1f, 6f), material);
    }

    private static void ConfigureNavigationAreas()
    {
        ConfigureRoot("WalkableRoot", 0, true);
        ConfigureRoot("StaticBlockerRoot", 1, true);
        ConfigureRoot("WallRoot", 1, true);
        ConfigureRoot("Ground_Base", 1, true);
        ConfigureRoot("AreaTriggerRoot", 1, false);
        ConfigureRoot("RouteDebugRoot", 1, false);
        ConfigureRoot("ExpansionAnchorRoot", 1, false);
        ConfigureRoot("ArtRoot", 1, false);
        ConfigureRoot("_PlayerRoot", 1, false);
        ConfigureRoot("_ConfigRoot", 1, false);
    }

    private static string CheckRoutes()
    {
        StringBuilder report = new StringBuilder();
        Vector3 start = new Vector3(11f, 0f, 14.5f);
        Vector3[] targets =
        {
            new Vector3(24f, 0f, 14.5f),
            new Vector3(24f, 0f, 30f),
            new Vector3(11f, 0f, 43f),
            new Vector3(39f, 0f, 46f),
            new Vector3(69f, 0f, 43f),
            new Vector3(67f, 0f, 12f)
        };

        foreach (Vector3 target in targets)
        {
            NavMeshHit startHit;
            NavMeshHit targetHit;
            bool startOk = NavMesh.SamplePosition(start, out startHit, 3f, NavMesh.AllAreas);
            bool targetOk = NavMesh.SamplePosition(target, out targetHit, 3f, NavMesh.AllAreas);
            NavMeshPath path = new NavMeshPath();
            bool found = startOk && targetOk && NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, path);
            report.AppendLine($"{start} -> {target}: startOk={startOk} targetOk={targetOk} found={found} status={path.status} corners={path.corners.Length}");
        }

        return report.ToString();
    }

    private static void SetScale(string name, Vector3 scale)
    {
        GameObject gameObject = GameObject.Find(name);
        if (gameObject == null) return;

        gameObject.transform.localScale = scale;
        EditorUtility.SetDirty(gameObject);
    }

    private static void EnsureConnector(Transform root, string name, Vector3 position, Vector3 scale, Material material)
    {
        Transform existing = root.Find(name);
        GameObject connector = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        connector.name = name;
        connector.transform.SetParent(root, true);
        connector.transform.position = position;
        connector.transform.rotation = Quaternion.identity;
        connector.transform.localScale = scale;

        MeshRenderer renderer = connector.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            renderer.sharedMaterial = material;
        }

        Collider collider = connector.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        EditorUtility.SetDirty(connector);
    }

    private static void ConfigureRoot(string rootName, int navArea, bool navigationStatic)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null) return;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            GameObject gameObject = child.gameObject;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            if (navigationStatic)
            {
                flags |= StaticEditorFlags.NavigationStatic;
            }
            else
            {
                flags &= ~StaticEditorFlags.NavigationStatic;
            }

            GameObjectUtility.SetStaticEditorFlags(gameObject, flags);
            GameObjectUtility.SetNavMeshArea(gameObject, navArea);
            EditorUtility.SetDirty(gameObject);
        }
    }
}
