using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Phase9SceneAudit
{
    [MenuItem("Tools/Phase9/Audit Scene")]
    public static void AuditScene()
    {
        string scenePath = "Assets/Phase9/Scenes/SampleScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Debug.Log($"[Phase9SceneAudit] Scene: {scene.name}, roots={scene.rootCount}");

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            Debug.Log($"[Phase9SceneAudit] Root {i}: {root.name}");
            Component[] components = root.GetComponents<Component>();
            for (int j = 0; j < components.Length; j++)
            {
                Component component = components[j];
                if (component == null)
                {
                    Debug.Log($"[Phase9SceneAudit]   - Missing Component");
                    continue;
                }

                Debug.Log($"[Phase9SceneAudit]   - {component.GetType().FullName}");
            }
        }
    }
}
