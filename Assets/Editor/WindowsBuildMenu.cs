using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WindowsBuildMenu
{
    private const string Phase9ScenePath = "Assets/Phase9/Scenes/SampleScene.unity";
    private const string Phase9StartupScenePath = "Assets/Phase9/Scenes/Temp_Phase9Startup.unity";
    private const string Phase3ScenePath = "Assets/Phase3/Scenes/Phase3_Prototype.unity";
    private const string Phase10OverlayScenePath = "Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity";
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    private const string ProductName = "这个窑厂我包了";
    private static readonly string BuildFolderPath = Path.Combine(DesktopPath, "新建文件夹");
    private static readonly string OutputPath = Path.Combine(BuildFolderPath, ProductName + ".exe");

    [MenuItem("Build/Windows/Build")]
    public static void BuildWindows()
    {
        BuildInternal(false);
    }

    [MenuItem("Build/Windows/Build Development")]
    public static void BuildWindowsDevelopment()
    {
        BuildInternal(true);
    }

    private static void BuildInternal(bool development)
    {
        Directory.CreateDirectory(BuildFolderPath);

        string phase9StartupScenePath = BuildPhase9StartupScene();
        var additionalScenes = LoadAdditionalScenes(phase9StartupScenePath);
        var scenes = new[] { phase9StartupScenePath }
            .Concat(additionalScenes)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes found in Build Settings.");
        }

        EditorUserBuildSettings.development = development;
        EditorUserBuildSettings.connectProfiler = development;
        PlayerSettings.productName = ProductName;
        PlayerSettings.companyName = ProductName;

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = development ? BuildOptions.Development : BuildOptions.None
        };

        Debug.Log($"[WindowsBuildMenu] Building Windows player to '{Path.GetFullPath(OutputPath)}' with {scenes.Length} scene(s).");
        Debug.Log($"[WindowsBuildMenu] Build scene list: {string.Join(", ", scenes)}");

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        Debug.Log($"[WindowsBuildMenu] Build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Windows build failed with result: {summary.result}");
        }
    }

    private static string BuildPhase9StartupScene()
    {
        Scene startupScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Scene phase9Scene = EditorSceneManager.OpenScene(Phase9ScenePath, OpenSceneMode.Additive);

        foreach (GameObject root in phase9Scene.GetRootGameObjects())
        {
            GameObject clone = UnityEngine.Object.Instantiate(root);
            clone.name = root.name;
            SceneManager.MoveGameObjectToScene(clone, startupScene);
        }

        EditorSceneManager.CloseScene(phase9Scene, true);
        EditorSceneManager.SaveScene(startupScene, Phase9StartupScenePath);
        AssetDatabase.ImportAsset(Phase9StartupScenePath);
        return Phase9StartupScenePath;
    }

    private static string[] LoadAdditionalScenes(string phase9StartupScenePath)
    {
        var scenePaths = new List<string>();

        AddSceneIfAvailable(scenePaths, Phase3ScenePath);
        AddSceneIfAvailable(scenePaths, Phase10OverlayScenePath);

        string environmentScenes = Environment.GetEnvironmentVariable("YCWBL_ADDITIONAL_SCENES");
        if (environmentScenes != null)
        {
            scenePaths.AddRange(environmentScenes
                .Split(new[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(scene => scene.Trim())
                .Where(scene => !string.IsNullOrWhiteSpace(scene)));
        }

        scenePaths.AddRange(EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .Where(scene => scene != Phase9ScenePath && scene != phase9StartupScenePath));

        return scenePaths
            .Where(scene => scene != Phase9ScenePath && scene != phase9StartupScenePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddSceneIfAvailable(ICollection<string> scenePaths, string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
        {
            Debug.LogWarning($"[WindowsBuildMenu] Scene not found and will be skipped: {scenePath}");
            return;
        }

        scenePaths.Add(scenePath);
    }
}
