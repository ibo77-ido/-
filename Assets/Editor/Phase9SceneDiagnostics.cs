using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Phase9SceneDiagnostics
{
    [MenuItem("Tools/Phase9/Log Sample Scene Roots")]
    public static void LogSampleSceneRoots()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Phase9/Scenes/SampleScene.unity", OpenSceneMode.Single);

        Debug.Log($"[Phase9SceneDiagnostics] Scene root count: {scene.rootCount}");

        foreach (var root in scene.GetRootGameObjects())
        {
            var componentNames = root
                .GetComponents<Component>()
                .Select(component => component == null ? "<Missing>" : component.GetType().Name)
                .ToArray();

            Debug.Log($"[Phase9SceneDiagnostics] Root: {root.name} | Active={root.activeSelf} | Components={string.Join(", ", componentNames)}");
        }
    }

    [MenuItem("Tools/Phase9/Log Static Layer SpriteRenderers")]
    public static void LogStaticLayerSpriteRenderers()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Phase9/Scenes/SampleScene.unity", OpenSceneMode.Single);
        var staticLayer = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "静态层");

        if (staticLayer == null)
        {
            Debug.LogError("[Phase9SceneDiagnostics] Root '静态层' was not found.");
            return;
        }

        var renderers = staticLayer.GetComponentsInChildren<SpriteRenderer>(true);
        Debug.Log($"[Phase9SceneDiagnostics] SpriteRenderer count under 静态层: {renderers.Length}");

        foreach (var spriteRenderer in renderers)
        {
            var transform = spriteRenderer.transform;
            var spriteName = spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "<null>";
            var materialName = spriteRenderer.sharedMaterial != null ? spriteRenderer.sharedMaterial.name : "<null>";
            Debug.Log(
                $"[Phase9SceneDiagnostics] SpriteRenderer: {GetTransformPath(transform)} | Active={transform.gameObject.activeSelf} | " +
                $"Sprite={spriteName} | Material={materialName} | SortingLayer={spriteRenderer.sortingLayerName} | SortingOrder={spriteRenderer.sortingOrder}");
        }
    }

    [MenuItem("Tools/Phase9/Log Runtime Visual Components")]
    public static void LogRuntimeVisualComponents()
    {
        LogRuntimeVisualComponentsForScene("Assets/Phase9/Scenes/SampleScene.unity");
    }

    public static void LogTempFullRuntimeVisualComponents()
    {
        LogRuntimeVisualComponentsForScene("Assets/Phase9/Scenes/Temp_Phase9Full.unity");
    }

    public static void LogTempFullComponentTypes()
    {
        LogComponentTypesForScene("Assets/Phase9/Scenes/Temp_Phase9Full.unity");
    }

    private static void LogRuntimeVisualComponentsForScene(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();
        Debug.Log($"[Phase9SceneDiagnostics] Runtime visual audit scene={scenePath} roots={roots.Length}");

        foreach (var root in roots)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var graphics = root.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            Debug.Log($"[Phase9SceneDiagnostics] Root={root.name} | active={root.activeSelf} | renderers={renderers.Length} | graphics={graphics.Length}");

            for (int i = 0; i < renderers.Length && i < 12; i++)
            {
                var renderer = renderers[i];
                Debug.Log($"[Phase9SceneDiagnostics] Renderer {GetTransformPath(renderer.transform)} | type={renderer.GetType().Name} | active={renderer.gameObject.activeInHierarchy} | enabled={renderer.enabled} | material={(renderer.sharedMaterial != null ? renderer.sharedMaterial.name : "<null>")} | bounds={renderer.bounds}");
            }

            for (int i = 0; i < graphics.Length && i < 12; i++)
            {
                var graphic = graphics[i];
                Debug.Log($"[Phase9SceneDiagnostics] Graphic {GetTransformPath(graphic.transform)} | type={graphic.GetType().Name} | active={graphic.gameObject.activeInHierarchy} | enabled={graphic.enabled} | color={graphic.color}");
            }
        }
    }

    private static void LogComponentTypesForScene(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var counts = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Component>(true))
            .GroupBy(component => component == null ? "<Missing>" : component.GetType().FullName)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key);

        foreach (var group in counts)
        {
            Debug.Log($"[Phase9SceneDiagnostics] ComponentType {group.Key} | Count={group.Count()}");
        }
    }

    private static string GetTransformPath(Transform transform)
    {
        var path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = $"{transform.name}/{path}";
        }

        return path;
    }
}
