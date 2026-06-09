using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Phase6MapStructureMigrator
{
    private const string ScenePath = "Assets/Phase6/Scenes/Workshop_TestScene.unity";

    private static readonly string[] LogicChildren =
    {
        "Ground_Base",
        "WalkableRoot",
        "StaticBlockerRoot",
        "WallRoot",
        "AreaTriggerRoot",
        "WorkstationRoot",
        "RouteDebugRoot",
        "ExpansionAnchorRoot"
    };

    private static readonly string[] ArtChildren =
    {
        "Map_Background_2D",
        "Map_Buildings_2D",
        "Map_Foreground_2D",
        "Map_Overlay_2D"
    };

    [MenuItem("Phase6/Map/Apply 2.5D Art Logic Structure")]
    public static void ApplyToWorkshopTestScene()
    {
        OpenWorkshopSceneIfNeeded();

        GameObject mapRoot = FindOrCreateRoot("_MapRoot");
        Transform logicRoot = FindOrCreateChild(mapRoot.transform, "LogicRoot");
        Transform artRoot = FindOrCreateChild(mapRoot.transform, "ArtRoot");

        foreach (string childName in LogicChildren)
        {
            Transform existing = mapRoot.transform.Find(childName);
            Transform target = existing != null
                ? existing
                : FindOrCreateChild(logicRoot, childName);

            target.SetParent(logicRoot, true);
        }

        foreach (string childName in ArtChildren)
        {
            FindOrCreateChild(artRoot, childName);
        }

        EnsureObliqueCamera();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("Phase6 map structure migrated to LogicRoot + ArtRoot.");
    }

    private static void OpenWorkshopSceneIfNeeded()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path == ScenePath)
        {
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(ScenePath);
        }
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        GameObject found = GameObject.Find(name);
        if (found != null)
        {
            return found;
        }

        return new GameObject(name);
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform found = parent.Find(name);
        if (found != null)
        {
            return found;
        }

        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static void EnsureObliqueCamera()
    {
        GameObject cameraRoot = FindOrCreateRoot("_CameraRoot");
        Transform cameraTransform = cameraRoot.transform.Find("Camera_2D_Oblique");

        if (cameraTransform == null)
        {
            GameObject cameraObject = new GameObject("Camera_2D_Oblique");
            cameraTransform = cameraObject.transform;
            cameraTransform.SetParent(cameraRoot.transform, false);
        }

        cameraTransform.position = new Vector3(40f, 45f, -35f);
        cameraTransform.rotation = Quaternion.Euler(60f, 0f, 0f);

        Camera camera = cameraTransform.GetComponent<Camera>();
        if (camera == null)
        {
            camera = cameraTransform.gameObject.AddComponent<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 40f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.78f, 0.74f, 0.66f, 1f);
    }
}
