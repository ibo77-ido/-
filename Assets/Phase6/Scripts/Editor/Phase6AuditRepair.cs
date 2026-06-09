using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class Phase6AuditRepair
{
    public static string Apply()
    {
        StringBuilder report = new StringBuilder();

        Material mapMaterial = EnsureMaterial("Assets/Phase6/Materials/Mat_Map_Background_Beige.mat", new Color(0.73f, 0.65f, 0.48f, 1f));
        Material roadMaterial = EnsureMaterial("Assets/Phase6/Materials/Mat_Map_Road_Brown.mat", new Color(0.38f, 0.30f, 0.20f, 1f));
        Material areaMaterial = EnsureMaterial("Assets/Phase6/Materials/Mat_Map_Area_Light.mat", new Color(0.58f, 0.72f, 0.62f, 1f));
        Material stationMaterial = EnsureMaterial("Assets/Phase6/Materials/Mat_Station_Fallback.mat", new Color(0.82f, 0.52f, 0.25f, 1f));
        Material playerMaterial = EnsureMaterial("Assets/Phase6/Materials/Mat_Player_Fallback.mat", new Color(0.18f, 0.28f, 0.85f, 1f));

        EnsureMapArt(mapMaterial, roadMaterial, areaMaterial);
        FixCamera(report);
        FixPlayer(playerMaterial, report);
        FixWorkstations(stationMaterial, report);
        FixAreaManager();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        return report.ToString();
    }

    private static void EnsureMapArt(Material mapMaterial, Material roadMaterial, Material areaMaterial)
    {
        FindOrCreatePath("_MapRoot/ArtRoot/Map_Background_2D");
        FindOrCreatePath("_MapRoot/ArtRoot/Map_Buildings_2D");
        FindOrCreatePath("_MapRoot/ArtRoot/Map_Foreground_2D");
        FindOrCreatePath("_MapRoot/ArtRoot/Map_Overlay_2D");

        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Background_2D/Visual_Map_Background", new Vector3(40f, -0.02f, 30f), new Vector3(80f, 0.03f, 60f), mapMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_OrderArea", new Vector3(11f, 0.01f, 14.5f), new Vector3(20f, 0.02f, 26f), areaMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_WheelArea", new Vector3(11f, 0.01f, 45.5f), new Vector3(20f, 0.02f, 26f), areaMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_GlazeArea", new Vector3(39f, 0.01f, 48f), new Vector3(24f, 0.02f, 22f), areaMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_StorageArea", new Vector3(39f, 0.01f, 10f), new Vector3(24f, 0.02f, 18f), areaMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_KilnArea", new Vector3(67f, 0.01f, 45.5f), new Vector3(24f, 0.02f, 26f), areaMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_MaterialArea", new Vector3(67f, 0.01f, 14.5f), new Vector3(24f, 0.02f, 26f), areaMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_MainRoad", new Vector3(24f, 0.02f, 30f), new Vector3(5.5f, 0.03f, 60f), roadMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_SecondaryRoad", new Vector3(40f, 0.03f, 30f), new Vector3(80f, 0.03f, 4.5f), roadMaterial);
        EnsureCubeVisual("_MapRoot/ArtRoot/Map_Overlay_2D/Visual_KilnBranch", new Vector3(53.5f, 0.04f, 28f), new Vector3(4f, 0.03f, 18f), roadMaterial);
    }

    private static void FixCamera(StringBuilder report)
    {
        Camera camera = GameObject.Find("Camera_2D_Oblique")?.GetComponent<Camera>();
        if (camera == null) return;

        camera.transform.position = new Vector3(40f, 45f, 4.02f);
        camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        camera.orthographic = true;
        camera.orthographicSize = 46f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.78f, 0.74f, 0.66f, 1f);
        camera.tag = "MainCamera";

        CameraFollow2D follow = camera.GetComponent<CameraFollow2D>();
        GameObject playerObject = GameObject.Find("PlayerCharacter");
        if (follow != null && playerObject != null)
        {
            SetSerializedReference(follow, "target", playerObject.transform);
            SetSerializedVector3(follow, "offset", new Vector3(0f, 45f, -26f));
        }

        report.AppendLine("Camera fixed");
    }

    private static void FixPlayer(Material playerMaterial, StringBuilder report)
    {
        GameObject player = GameObject.Find("PlayerCharacter");
        if (player == null) return;

        GameObject logicRoot = FindOrCreatePath("_PlayerRoot/PlayerCharacter/LogicRoot");
        GameObject artRoot = FindOrCreatePath("_PlayerRoot/PlayerCharacter/ArtRoot");
        GameObject visual = EnsureCubeVisual("_PlayerRoot/PlayerCharacter/ArtRoot/Visual_Player_Fallback", Vector3.zero, new Vector3(0.8f, 1.4f, 0.8f), playerMaterial);
        visual.transform.localPosition = new Vector3(0f, 0.7f, 0f);

        PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
        if (playerCharacter != null)
        {
            SetSerializedReference(playerCharacter, "logicRoot", logicRoot.transform);
            SetSerializedReference(playerCharacter, "artRoot", artRoot.transform);

            CharacterConfigSO config = AssetDatabase.LoadAssetAtPath<CharacterConfigSO>("Assets/Phase6/Data/PlayerCharacterConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CharacterConfigSO>();
                config.moveSpeed = 5f;
                config.stoppingDistance = 0.5f;
                config.acceleration = 8f;
                config.interactionRange = 2f;
                AssetDatabase.CreateAsset(config, "Assets/Phase6/Data/PlayerCharacterConfig.asset");
            }

            SetSerializedReference(playerCharacter, "config", config);
        }

        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
        NavMeshHit hit;
        if (agent != null && NavMesh.SamplePosition(player.transform.position, out hit, 3f, NavMesh.AllAreas))
        {
            player.transform.position = hit.position;
            agent.Warp(hit.position);
            report.AppendLine("Player snapped to NavMesh sample");
        }
    }

    private static void FixWorkstations(Material stationMaterial, StringBuilder report)
    {
        Workstation[] workstations = Object.FindObjectsOfType<Workstation>(true);
        foreach (Workstation workstation in workstations)
        {
            Transform artRoot = workstation.transform.Find("ArtRoot");
            if (artRoot == null) continue;

            Transform existingVisual = artRoot.Find("Visual_Fallback");
            GameObject visual = existingVisual != null ? existingVisual.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual_Fallback";
            visual.transform.SetParent(artRoot, false);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

            Collider collider = visual.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);

            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
                renderer.sharedMaterial = stationMaterial;
            }
        }

        report.AppendLine($"Workstation visuals ensured: {workstations.Length}");
    }

    private static void FixAreaManager()
    {
        AreaManager areaManager = Object.FindObjectOfType<AreaManager>(true);
        if (areaManager == null) return;

        SerializedObject serializedObject = new SerializedObject(areaManager);
        SerializedProperty currentArea = serializedObject.FindProperty("currentArea");
        if (currentArea != null)
        {
            currentArea.enumValueIndex = 0;
        }
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(areaManager);
    }

    private static Material EnsureMaterial(string path, Color color)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        material.name = Path.GetFileNameWithoutExtension(path);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject FindOrCreatePath(string path)
    {
        string[] parts = path.Split('/');
        GameObject current = GameObject.Find(parts[0]);
        if (current == null)
        {
            current = new GameObject(parts[0]);
        }

        for (int index = 1; index < parts.Length; index++)
        {
            Transform child = current.transform.Find(parts[index]);
            if (child == null)
            {
                GameObject childObject = new GameObject(parts[index]);
                childObject.transform.SetParent(current.transform, false);
                current = childObject;
            }
            else
            {
                current = child.gameObject;
            }
        }

        return current;
    }

    private static GameObject EnsureCubeVisual(string path, Vector3 position, Vector3 scale, Material material)
    {
        GameObject visual = GameObject.Find(path);
        if (visual == null)
        {
            int splitIndex = path.LastIndexOf('/');
            string parentPath = path.Substring(0, splitIndex);
            string objectName = path.Substring(splitIndex + 1);
            GameObject parent = FindOrCreatePath(parentPath);
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = objectName;
            visual.transform.SetParent(parent.transform, true);
        }

        visual.transform.position = position;
        visual.transform.rotation = Quaternion.identity;
        visual.transform.localScale = scale;

        Collider collider = visual.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);

        MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            renderer.sharedMaterial = material;
        }

        return visual;
    }

    private static void SetSerializedReference(Object target, string propertyName, Object value)
    {
        if (target == null) return;

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }

    private static void SetSerializedVector3(Object target, string propertyName, Vector3 value)
    {
        if (target == null) return;

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector3Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
