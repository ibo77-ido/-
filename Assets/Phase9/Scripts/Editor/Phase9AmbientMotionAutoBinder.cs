using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class Phase9AmbientMotionAutoBinder
{
    static Phase9AmbientMotionAutoBinder()
    {
        EditorApplication.delayCall += BindSceneAmbientMotion;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.hierarchyChanged += BindSceneAmbientMotion;
    }

    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        BindSceneAmbientMotion();
    }

    private static void BindSceneAmbientMotion()
    {
        bool changed = false;
        changed |= EnsureCloudDrift(GameObject.Find("CLoud"));
        changed |= EnsureCloudDrift(GameObject.Find("Cloud"));
        changed |= EnsureSpriteSway(GameObject.Find("BamBoo"));
        changed |= EnsureSpriteSway(GameObject.Find("Bamboo"));
        changed |= EnsureSpriteSway(GameObject.Find("bamboo"));

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    private static bool EnsureCloudDrift(GameObject root)
    {
        if (!root)
        {
            return false;
        }

        bool changed = false;
        CloudFogDriftController controller = root.GetComponent<CloudFogDriftController>();
        if (!controller)
        {
            controller = root.AddComponent<CloudFogDriftController>();
            changed = true;
        }

        SerializedObject serialized = new SerializedObject(controller);
        SerializedProperty maxChildren = serialized.FindProperty("maxChildren");
        if (maxChildren != null && maxChildren.intValue != 5)
        {
            maxChildren.intValue = 5;
            changed = true;
        }

        SerializedProperty includeRootRenderer = serialized.FindProperty("includeRootRenderer");
        if (includeRootRenderer != null && !includeRootRenderer.boolValue)
        {
            includeRootRenderer.boolValue = true;
            changed = true;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        controller.CacheTargets();
        return changed;
    }

    private static bool EnsureSpriteSway(GameObject root)
    {
        if (!root)
        {
            return false;
        }

        bool changed = false;
        SpriteSwayController controller = root.GetComponent<SpriteSwayController>();
        if (!controller)
        {
            controller = root.AddComponent<SpriteSwayController>();
            changed = true;
        }

        controller.CacheTargets();
        return changed;
    }
}
