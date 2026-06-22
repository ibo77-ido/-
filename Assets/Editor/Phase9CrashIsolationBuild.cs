using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class Phase9CrashIsolationBuild
{
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    private static readonly string TempScenePath = "Assets/Phase9/Scenes/Temp_NoBridgeRoot.unity";
    private static readonly string OutputFolderPath = Path.Combine(DesktopPath, "YCWBL_NoBridge");
    private static readonly string OutputPath = Path.Combine(OutputFolderPath, "YCWBL_NoBridge.exe");
    private static readonly string MinimalOutputFolderPath = Path.Combine(DesktopPath, "YCWBL_Minimal");
    private static readonly string MinimalOutputPath = Path.Combine(MinimalOutputFolderPath, "YCWBL_Minimal.exe");
    private static readonly string EmptyOutputFolderPath = Path.Combine(DesktopPath, "YCWBL_Empty");
    private static readonly string EmptyOutputPath = Path.Combine(EmptyOutputFolderPath, "YCWBL_Empty.exe");
    private static readonly string NoCameraOutputFolderPath = Path.Combine(DesktopPath, "YCWBL_NoCamera");
    private static readonly string NoCameraOutputPath = Path.Combine(NoCameraOutputFolderPath, "YCWBL_NoCamera.exe");
    private static readonly string CameraOnlyOutputFolderPath = Path.Combine(DesktopPath, "YCWBL_CameraOnly");
    private static readonly string CameraOnlyOutputPath = Path.Combine(CameraOnlyOutputFolderPath, "YCWBL_CameraOnly.exe");
    private static readonly string FreshCameraOutputFolderPath = Path.Combine(DesktopPath, "YCWBL_FreshCamera");
    private static readonly string FreshCameraOutputPath = Path.Combine(FreshCameraOutputFolderPath, "YCWBL_FreshCamera.exe");
    private static readonly string SubsetOutputFolderPath = Path.Combine(DesktopPath, "YCWBL_Subset");
    private static readonly string SubsetOutputPath = Path.Combine(SubsetOutputFolderPath, "YCWBL_Subset.exe");
    private static readonly string SubsetSelectionPath = "Assets/Editor/Phase9SubsetRoots.txt";
    private static readonly string SubsetStripComponentsPath = "Assets/Editor/Phase9SubsetStripComponents.txt";
    private const string Phase9ScenePath = "Assets/Phase9/Scenes/SampleScene.unity";
    private const string Phase3ScenePath = "Assets/Phase3/Scenes/Phase3_Prototype.unity";
    private const string Phase10OverlayScenePath = "Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity";
    private const string IntegratedScenePath = "Assets/Phase9/Scenes/Temp_Phase9Integrated.unity";

    [MenuItem("Build/Windows/Build Without Bridge Root")]
    public static void BuildWithoutBridgeRoot()
    {
        Directory.CreateDirectory(OutputFolderPath);

        Scene scene = EditorSceneManager.OpenScene("Assets/Phase9/Scenes/SampleScene.unity", OpenSceneMode.Single);
        GameObject bridgeRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "_BridgeRoot");
        if (bridgeRoot != null)
        {
            bridgeRoot.SetActive(false);
        }

        EditorSceneManager.SaveScene(scene, TempScenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { TempScenePath },
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building without _BridgeRoot to '{Path.GetFullPath(OutputPath)}'.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Isolation build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build Minimal Scene")]
    public static void BuildMinimalScene()
    {
        Directory.CreateDirectory(MinimalOutputFolderPath);

        Scene scene = EditorSceneManager.OpenScene("Assets/Phase9/Scenes/SampleScene.unity", OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != "Main Camera" && root.name != "Global Light 2D")
            {
                root.SetActive(false);
            }
        }

        string minimalScenePath = "Assets/Phase9/Scenes/Temp_Minimal.unity";
        EditorSceneManager.SaveScene(scene, minimalScenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { minimalScenePath },
            locationPathName = MinimalOutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building minimal scene to '{Path.GetFullPath(MinimalOutputPath)}'.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Minimal build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Minimal build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build Empty Scene")]
    public static void BuildEmptyScene()
    {
        Directory.CreateDirectory(EmptyOutputFolderPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        string emptyScenePath = "Assets/Phase9/Scenes/Temp_Empty.unity";
        EditorSceneManager.SaveScene(scene, emptyScenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { emptyScenePath },
            locationPathName = EmptyOutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building empty scene to '{Path.GetFullPath(EmptyOutputPath)}'.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Empty build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Empty build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build No Camera Scene")]
    public static void BuildNoCameraScene()
    {
        Directory.CreateDirectory(NoCameraOutputFolderPath);

        Scene scene = EditorSceneManager.OpenScene("Assets/Phase9/Scenes/SampleScene.unity", OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Main Camera")
            {
                root.SetActive(false);
            }
        }

        string scenePath = "Assets/Phase9/Scenes/Temp_NoCamera.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            locationPathName = NoCameraOutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building no-camera scene to '{Path.GetFullPath(NoCameraOutputPath)}'.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] No-camera build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"No-camera build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build Camera Only Scene")]
    public static void BuildCameraOnlyScene()
    {
        Directory.CreateDirectory(CameraOnlyOutputFolderPath);

        Scene scene = EditorSceneManager.OpenScene("Assets/Phase9/Scenes/SampleScene.unity", OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != "Main Camera")
            {
                root.SetActive(false);
            }
        }

        string scenePath = "Assets/Phase9/Scenes/Temp_CameraOnly.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            locationPathName = CameraOnlyOutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building camera-only scene to '{Path.GetFullPath(CameraOnlyOutputPath)}'.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Camera-only build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Camera-only build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build Fresh Camera Scene")]
    public static void BuildFreshCameraScene()
    {
        Directory.CreateDirectory(FreshCameraOutputFolderPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;

        string scenePath = "Assets/Phase9/Scenes/Temp_FreshCamera.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            locationPathName = FreshCameraOutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building fresh-camera scene to '{Path.GetFullPath(FreshCameraOutputPath)}'.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Fresh-camera build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Fresh-camera build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build Fresh Selected Roots Scene")]
    public static void BuildFreshSelectedRootsScene()
    {
        string subsetOutputName = GetSafeOutputName();
        string subsetOutputFolderPath = Path.Combine(DesktopPath, subsetOutputName);
        string subsetOutputPath = Path.Combine(subsetOutputFolderPath, subsetOutputName + ".exe");
        Directory.CreateDirectory(subsetOutputFolderPath);

        var selectedNames = LoadSubsetSelection();
        if (selectedNames.Count == 0)
        {
            throw new InvalidOperationException($"No root names found in '{SubsetSelectionPath}'.");
        }

        Scene freshScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Scene sampleScene = EditorSceneManager.OpenScene("Assets/Phase9/Scenes/SampleScene.unity", OpenSceneMode.Additive);

        var selectedRoots = sampleScene.GetRootGameObjects()
            .Where(root => ShouldIncludeRoot(root.name, selectedNames))
            .ToArray();
        var stripComponents = LoadFullPhase9StripComponents();
        bool clearSpriteRendererSprite = stripComponents.Contains("ClearSpriteRendererSprite");

        if (selectedRoots.Length == 0)
        {
            throw new InvalidOperationException($"None of the requested roots were found in SampleScene. Requested: {string.Join(", ", selectedNames)}");
        }

        foreach (GameObject root in selectedRoots)
        {
            GameObject clone = UnityEngine.Object.Instantiate(root);
            clone.name = root.name;
            PruneCloneToAllowedPaths(clone, selectedNames, root.name);
            if (stripComponents.Count > 0)
            {
                StripComponentsByName(clone, stripComponents, clearSpriteRendererSprite);
            }
            SceneManager.MoveGameObjectToScene(clone, freshScene);
        }

        EditorSceneManager.CloseScene(sampleScene, true);

        string subsetScenePath = "Assets/Phase9/Scenes/Temp_Subset.unity";
        EditorSceneManager.SaveScene(freshScene, subsetScenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { subsetScenePath },
            locationPathName = subsetOutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building selected-roots scene to '{Path.GetFullPath(subsetOutputPath)}' with roots: {string.Join(", ", selectedNames)}.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Selected-roots build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Selected-roots build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build Fresh Full Phase9 Scene")]
    public static void BuildFreshFullPhase9Scene()
    {
        string outputName = GetSafeEnvironmentOutputName("PHASE9_FULL_OUTPUT", "YCWBL_Phase9Full");
        string outputFolderPath = Path.Combine(DesktopPath, outputName);
        string outputPath = Path.Combine(outputFolderPath, outputName + ".exe");
        Directory.CreateDirectory(outputFolderPath);

        var stripComponents = LoadFullPhase9StripComponents();
        bool clearSpriteRendererSprite = stripComponents.Contains("ClearSpriteRendererSprite");

        string fullScenePath = Phase9ScenePath;
        if (stripComponents.Count > 0)
        {
            fullScenePath = "Assets/Phase9/Scenes/Temp_Phase9Full.unity";
            AssetDatabase.DeleteAsset(fullScenePath);
            if (!AssetDatabase.CopyAsset(Phase9ScenePath, fullScenePath))
            {
                throw new InvalidOperationException($"Failed to copy Phase9 scene from '{Phase9ScenePath}' to '{fullScenePath}'.");
            }

            AssetDatabase.ImportAsset(fullScenePath);
            Scene sceneCopy = EditorSceneManager.OpenScene(fullScenePath, OpenSceneMode.Single);
            foreach (GameObject root in sceneCopy.GetRootGameObjects())
            {
                StripComponentsByName(root, stripComponents, clearSpriteRendererSprite);
            }

            EditorSceneManager.SaveScene(sceneCopy, fullScenePath);
            LogCopiedPhase9VisualsBeforeBuild(sceneCopy);
        }
        else
        {
            LogPhase9SourceVisualsBeforeBuild();
        }

        var options = new BuildPlayerOptions
        {
            scenes = new[] { fullScenePath },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building full fresh Phase9 scene to '{Path.GetFullPath(outputPath)}'.");
        Debug.Log($"[Phase9CrashIsolationBuild] Build scene path: {fullScenePath}");

        Action restoreGraphicsSettings = ApplyTemporaryGraphicsApiFromEnvironment();
        BuildReport report;
        try
        {
            report = BuildPipeline.BuildPlayer(options);
        }
        finally
        {
            restoreGraphicsSettings();
        }

        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Full Phase9 build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Full Phase9 build failed with result: {summary.result}");
        }
    }

    private static void LogPhase9SourceVisualsBeforeBuild()
    {
        Scene scene = EditorSceneManager.OpenScene(Phase9ScenePath, OpenSceneMode.Single);
        int rendererCount = 0;
        int spriteRendererCount = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            rendererCount += renderers.Length;
            spriteRendererCount += root.GetComponentsInChildren<SpriteRenderer>(true).Length;
            Debug.Log($"[Phase9CrashIsolationBuild] Source root visual audit | Root={root.name} | Renderers={renderers.Length}");
        }

        Debug.Log($"[Phase9CrashIsolationBuild] Source Phase9 visual audit complete | Renderers={rendererCount} | SpriteRenderers={spriteRendererCount}");
    }

    private static void LogCopiedPhase9VisualsBeforeBuild(Scene scene)
    {
        int rendererCount = 0;
        int spriteRendererCount = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            rendererCount += renderers.Length;
            spriteRendererCount += root.GetComponentsInChildren<SpriteRenderer>(true).Length;
            Debug.Log($"[Phase9CrashIsolationBuild] Copied root visual audit | Root={root.name} | Renderers={renderers.Length}");
        }

        Debug.Log($"[Phase9CrashIsolationBuild] Copied Phase9 visual audit complete | Renderers={rendererCount} | SpriteRenderers={spriteRendererCount}");
    }

    [MenuItem("Build/Windows/Build Phase9 Integrated Scene")]
    public static void BuildPhase9IntegratedScene()
    {
        string outputName = GetSafeEnvironmentOutputName("PHASE9_INTEGRATED_OUTPUT", "YCWBL_Phase9Integrated");
        string outputFolderPath = Path.Combine(DesktopPath, outputName);
        string outputPath = Path.Combine(outputFolderPath, outputName + ".exe");

        if (Directory.Exists(outputFolderPath))
        {
            Directory.Delete(outputFolderPath, true);
        }

        Directory.CreateDirectory(outputFolderPath);

        var selectedPhase9Names = LoadSubsetSelection();
        if (selectedPhase9Names.Count == 0)
        {
            selectedPhase9Names = GetDefaultPhase9IntegratedRoots();
        }

        var stripComponents = LoadEnvironmentList("PHASE9_INTEGRATED_STRIP_COMPONENTS");
        bool clearSpriteRendererSprite = stripComponents.Contains("ClearSpriteRendererSprite");
        bool includePhase3 = !IsEnvironmentFlagDisabled("PHASE9_INTEGRATED_INCLUDE_PHASE3");
        bool includePhase10 = !IsEnvironmentFlagDisabled("PHASE9_INTEGRATED_INCLUDE_PHASE10");

        Scene integratedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CopySelectedRootsIntoScene(Phase9ScenePath, integratedScene, selectedPhase9Names, stripComponents, clearSpriteRendererSprite);
        if (includePhase3)
        {
            CopyConfiguredRootsIntoScene("PHASE9_INTEGRATED_PHASE3_ROOTS", Phase3ScenePath, integratedScene, stripComponents, clearSpriteRendererSprite);
        }
        if (includePhase10)
        {
            CopyConfiguredRootsIntoScene("PHASE9_INTEGRATED_PHASE10_ROOTS", Phase10OverlayScenePath, integratedScene, stripComponents, clearSpriteRendererSprite);
        }

        EditorSceneManager.SaveScene(integratedScene, IntegratedScenePath);
        AssetDatabase.ImportAsset(IntegratedScenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { IntegratedScenePath },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building integrated Phase9 scene to '{Path.GetFullPath(outputPath)}'. Phase9 roots: {string.Join(", ", selectedPhase9Names)}.");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Integrated build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Integrated build failed with result: {summary.result}");
        }
    }

    [MenuItem("Build/Windows/Build Rebuilt Visual Phase9 Scene")]
    public static void BuildRebuiltVisualPhase9Scene()
    {
        string outputName = GetSafeEnvironmentOutputName("PHASE9_REBUILT_OUTPUT", "YCWBL_Phase9RebuiltVisual");
        string outputFolderPath = Path.Combine(DesktopPath, outputName);
        string outputPath = Path.Combine(outputFolderPath, outputName + ".exe");
        Directory.CreateDirectory(outputFolderPath);

        string rebuiltScenePath = "Assets/Phase9/Scenes/Temp_Phase9RebuiltVisual.unity";
        Scene rebuiltScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Scene sourceScene = EditorSceneManager.OpenScene(Phase9ScenePath, OpenSceneMode.Additive);

        GameObject mapRoot = new GameObject("静态层");
        SceneManager.MoveGameObjectToScene(mapRoot, rebuiltScene);
        CopySpriteRenderers(sourceScene, rebuiltScene, mapRoot.transform);

        Bounds mapBounds = CalculateRendererBounds(mapRoot);
        GameObject player = CreateRebuiltPlayer(rebuiltScene, mapBounds.center);
        CreateRebuiltCamera(rebuiltScene, player.transform, mapRoot.transform, mapBounds);
        CreateRebuiltInteractionPoints(rebuiltScene, mapBounds);
        CreateRebuiltBridgeRoot(rebuiltScene);
        CreateRebuiltEventSystem(rebuiltScene);

        EditorSceneManager.CloseScene(sourceScene, true);
        EditorSceneManager.SaveScene(rebuiltScene, rebuiltScenePath);
        AssetDatabase.ImportAsset(rebuiltScenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { rebuiltScenePath },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[Phase9CrashIsolationBuild] Building rebuilt visual Phase9 scene to '{Path.GetFullPath(outputPath)}'.");
        Debug.Log($"[Phase9CrashIsolationBuild] Rebuilt visual scene sprite renderers={mapRoot.GetComponentsInChildren<SpriteRenderer>(true).Length}, bounds={mapBounds}");
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"[Phase9CrashIsolationBuild] Rebuilt visual build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Rebuilt visual build failed with result: {summary.result}");
        }
    }

    [MenuItem("Tools/Phase9/Log Integration Source Roots")]
    public static void LogIntegrationSourceRoots()
    {
        LogSceneRoots(Phase3ScenePath);
        LogSceneRoots(Phase10OverlayScenePath);
    }

    private static void CopySpriteRenderers(Scene sourceScene, Scene destinationScene, Transform destinationRoot)
    {
        foreach (GameObject root in sourceScene.GetRootGameObjects())
        {
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sourceRenderer = renderers[i];
                if (sourceRenderer == null || sourceRenderer.sprite == null || sourceRenderer.GetComponentInParent<Canvas>(true) != null)
                {
                    continue;
                }

                GameObject copy = new GameObject(GetRelativePath(sourceRenderer.transform).Replace('/', '_'));
                copy.transform.SetParent(destinationRoot, true);
                copy.transform.position = sourceRenderer.transform.position;
                copy.transform.rotation = sourceRenderer.transform.rotation;
                copy.transform.localScale = sourceRenderer.transform.lossyScale;

                SpriteRenderer destinationRenderer = copy.AddComponent<SpriteRenderer>();
                destinationRenderer.sprite = sourceRenderer.sprite;
                destinationRenderer.color = sourceRenderer.color;
                destinationRenderer.flipX = sourceRenderer.flipX;
                destinationRenderer.flipY = sourceRenderer.flipY;
                destinationRenderer.drawMode = sourceRenderer.drawMode;
                destinationRenderer.size = sourceRenderer.size;
                destinationRenderer.tileMode = sourceRenderer.tileMode;
                destinationRenderer.maskInteraction = sourceRenderer.maskInteraction;
                destinationRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                destinationRenderer.sortingOrder = sourceRenderer.sortingOrder;
                if (!IsEnvironmentFlagEnabled("PHASE9_REBUILT_USE_DEFAULT_SPRITE_MATERIAL"))
                {
                    destinationRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
                }
                destinationRenderer.enabled = sourceRenderer.enabled;
                copy.SetActive(sourceRenderer.gameObject.activeInHierarchy);
            }
        }
    }

    private static Bounds CalculateRendererBounds(GameObject root)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        bool hasBounds = false;
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !renderers[i].enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderers[i].bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return bounds;
    }

    private static GameObject CreateRebuiltPlayer(Scene scene, Vector3 mapCenter)
    {
        GameObject player = new GameObject("HeroineRoot");
        player.transform.position = new Vector3(mapCenter.x, mapCenter.y, mapCenter.z);
        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.color = new Color(0.9f, 0.42f, 0.25f, 1f);
        renderer.sortingOrder = 500;
        player.AddComponent<PlayerCharacter>();
        player.AddComponent<MovementController>();
        SceneManager.MoveGameObjectToScene(player, scene);
        return player;
    }

    private static void CreateRebuiltCamera(Scene scene, Transform target, Transform mapRoot, Bounds mapBounds)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(5f, Mathf.Max(mapBounds.size.x, mapBounds.size.z, mapBounds.size.y) * 0.28f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 500f;
        camera.transform.position = new Vector3(mapBounds.center.x, mapBounds.max.y + 20f, mapBounds.center.z);
        camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();
        SetSerializedField(follow, "target", target);
        SetSerializedField(follow, "boundsRoot", mapRoot);
        SetSerializedField(follow, "followXZPlane", true);
        SetSerializedField(follow, "clampToBounds", false);
        SetSerializedField(follow, "orthographicSize", camera.orthographicSize);
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
    }

    private static void CreateRebuiltInteractionPoints(Scene scene, Bounds mapBounds)
    {
        GameObject root = new GameObject("Interaction");
        Vector3 center = mapBounds.center;
        float offsetX = Mathf.Max(1.5f, mapBounds.extents.x * 0.25f);
        float offsetZ = Mathf.Max(1.5f, mapBounds.extents.z * 0.25f);
        CreateInteractionPoint(root.transform, "Order-interact", center + new Vector3(-offsetX, 0f, offsetZ));
        CreateInteractionPoint(root.transform, "Shape-interact", center + new Vector3(offsetX, 0f, offsetZ));
        CreateInteractionPoint(root.transform, "Glaze-interact", center + new Vector3(-offsetX, 0f, -offsetZ));
        CreateInteractionPoint(root.transform, "Kiln-interact", center + new Vector3(offsetX, 0f, -offsetZ));
        SceneManager.MoveGameObjectToScene(root, scene);
    }

    private static void CreateInteractionPoint(Transform parent, string name, Vector3 position)
    {
        GameObject point = new GameObject(name);
        point.transform.SetParent(parent, true);
        point.transform.position = position;
    }

    private static void CreateRebuiltBridgeRoot(Scene scene)
    {
        GameObject bridgeRoot = new GameObject("_BridgeRoot");
        bridgeRoot.AddComponent<Phase9InteractionBridge>();
        bridgeRoot.AddComponent<Phase10_Narrative.Phase9Phase10Bridge>();
        bridgeRoot.AddComponent<Phase9RebuiltRuntimeControls>();
        SceneManager.MoveGameObjectToScene(bridgeRoot, scene);
    }

    private static void CreateRebuiltEventSystem(Scene scene)
    {
        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
        SceneManager.MoveGameObjectToScene(eventSystemObject, scene);
    }

    private static void SetSerializedField(UnityEngine.Object target, string fieldName, object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property == null)
        {
            return;
        }

        if (value is UnityEngine.Object objectValue)
        {
            property.objectReferenceValue = objectValue;
        }
        else if (value is bool boolValue)
        {
            property.boolValue = boolValue;
        }
        else if (value is float floatValue)
        {
            property.floatValue = floatValue;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static HashSet<string> LoadSubsetSelection()
    {
        string environmentSelection = Environment.GetEnvironmentVariable("PHASE9_SUBSET_SELECTION");
        if (!string.IsNullOrWhiteSpace(environmentSelection))
        {
            return SplitEnvironmentList(environmentSelection).ToHashSet();
        }

        string selectionFilePath = Path.GetFullPath(SubsetSelectionPath);
        if (!File.Exists(selectionFilePath))
        {
            return new HashSet<string>();
        }

        return File.ReadAllLines(selectionFilePath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#", StringComparison.Ordinal))
            .ToHashSet();
    }

    private static HashSet<string> LoadStripComponents()
    {
        string environmentStripComponents = Environment.GetEnvironmentVariable("PHASE9_STRIP_COMPONENTS");
        if (environmentStripComponents != null)
        {
            return SplitEnvironmentList(environmentStripComponents).ToHashSet();
        }

        string selectionFilePath = Path.GetFullPath(SubsetStripComponentsPath);
        if (!File.Exists(selectionFilePath))
        {
            return new HashSet<string>();
        }

        return File.ReadAllLines(selectionFilePath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#", StringComparison.Ordinal))
            .ToHashSet();
    }

    private static HashSet<string> LoadFullPhase9StripComponents()
    {
        string environmentStripComponents = Environment.GetEnvironmentVariable("PHASE9_STRIP_COMPONENTS");
        if (environmentStripComponents != null)
        {
            return SplitEnvironmentList(environmentStripComponents).ToHashSet();
        }

        return new HashSet<string>();
    }

    private static Action ApplyTemporaryGraphicsApiFromEnvironment()
    {
        string graphicsApiName = Environment.GetEnvironmentVariable("PHASE9_GRAPHICS_API");
        if (string.IsNullOrWhiteSpace(graphicsApiName))
        {
            return () => { };
        }

        GraphicsDeviceType graphicsApi = ParseGraphicsApi(graphicsApiName);
        bool previousUseDefault = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64);
        GraphicsDeviceType[] previousApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);

        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { graphicsApi });
        Debug.Log($"[Phase9CrashIsolationBuild] Temporarily building StandaloneWindows64 with graphics API: {graphicsApi}");

        return () =>
        {
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, previousUseDefault);
            if (previousApis != null && previousApis.Length > 0)
            {
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, previousApis);
            }

            Debug.Log("[Phase9CrashIsolationBuild] Restored StandaloneWindows64 graphics API settings.");
        };
    }

    private static GraphicsDeviceType ParseGraphicsApi(string graphicsApiName)
    {
        if (string.Equals(graphicsApiName, "OpenGLCore", StringComparison.OrdinalIgnoreCase)
            || string.Equals(graphicsApiName, "OpenGL", StringComparison.OrdinalIgnoreCase)
            || string.Equals(graphicsApiName, "GLCore", StringComparison.OrdinalIgnoreCase))
        {
            return GraphicsDeviceType.OpenGLCore;
        }

        if (string.Equals(graphicsApiName, "Direct3D11", StringComparison.OrdinalIgnoreCase)
            || string.Equals(graphicsApiName, "D3D11", StringComparison.OrdinalIgnoreCase))
        {
            return GraphicsDeviceType.Direct3D11;
        }

        throw new InvalidOperationException($"Unsupported PHASE9_GRAPHICS_API value: {graphicsApiName}");
    }

    private static void CopySelectedRootsIntoScene(
        string sourceScenePath,
        Scene destinationScene,
        HashSet<string> selectedNames,
        HashSet<string> stripComponents,
        bool clearSpriteRendererSprite)
    {
        Scene sourceScene = EditorSceneManager.OpenScene(sourceScenePath, OpenSceneMode.Additive);
        var selectedRoots = sourceScene.GetRootGameObjects()
            .Where(root => ShouldIncludeRoot(root.name, selectedNames))
            .ToArray();

        foreach (GameObject root in selectedRoots)
        {
            GameObject clone = UnityEngine.Object.Instantiate(root);
            clone.name = root.name;
            PruneCloneToAllowedPaths(clone, selectedNames, root.name);
            if (stripComponents.Count > 0)
            {
                StripComponentsByName(clone, stripComponents, clearSpriteRendererSprite);
            }

            SceneManager.MoveGameObjectToScene(clone, destinationScene);
        }

        EditorSceneManager.CloseScene(sourceScene, true);
    }

    private static void CopyAllRootsIntoScene(
        string sourceScenePath,
        Scene destinationScene,
        HashSet<string> stripComponents,
        bool clearSpriteRendererSprite)
    {
        Scene sourceScene = EditorSceneManager.OpenScene(sourceScenePath, OpenSceneMode.Additive);
        foreach (GameObject root in sourceScene.GetRootGameObjects())
        {
            GameObject clone = UnityEngine.Object.Instantiate(root);
            clone.name = root.name;
            if (stripComponents.Count > 0)
            {
                StripComponentsByName(clone, stripComponents, clearSpriteRendererSprite);
            }

            SceneManager.MoveGameObjectToScene(clone, destinationScene);
        }

        EditorSceneManager.CloseScene(sourceScene, true);
    }

    private static void CopyAllRootsIntoSceneWithReferenceRemap(
        Scene sourceScene,
        Scene destinationScene,
        HashSet<string> stripComponents,
        bool clearSpriteRendererSprite)
    {
        var referenceMap = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
        var cloneRoots = new List<GameObject>();

        foreach (GameObject root in sourceScene.GetRootGameObjects())
        {
            GameObject clone = UnityEngine.Object.Instantiate(root);
            clone.name = root.name;
            RegisterObjectMapping(root, clone, referenceMap);
            cloneRoots.Add(clone);
        }

        for (int i = 0; i < cloneRoots.Count; i++)
        {
            RemapObjectReferences(cloneRoots[i], referenceMap);
            if (stripComponents.Count > 0)
            {
                StripComponentsByName(cloneRoots[i], stripComponents, clearSpriteRendererSprite);
            }

            SceneManager.MoveGameObjectToScene(cloneRoots[i], destinationScene);
        }
    }

    private static void RegisterObjectMapping(
        GameObject originalRoot,
        GameObject cloneRoot,
        Dictionary<UnityEngine.Object, UnityEngine.Object> referenceMap)
    {
        var originalTransforms = originalRoot.GetComponentsInChildren<Transform>(true);
        var cloneTransforms = cloneRoot.GetComponentsInChildren<Transform>(true);
        var cloneByPath = cloneTransforms.ToDictionary(GetRelativePath, transform => transform);

        for (int i = 0; i < originalTransforms.Length; i++)
        {
            Transform originalTransform = originalTransforms[i];
            if (!cloneByPath.TryGetValue(GetRelativePath(originalTransform), out Transform cloneTransform))
            {
                continue;
            }

            referenceMap[originalTransform.gameObject] = cloneTransform.gameObject;
            referenceMap[originalTransform] = cloneTransform;

            RegisterComponentMappings(originalTransform.gameObject, cloneTransform.gameObject, referenceMap);
        }
    }

    private static void RegisterComponentMappings(
        GameObject originalObject,
        GameObject cloneObject,
        Dictionary<UnityEngine.Object, UnityEngine.Object> referenceMap)
    {
        Component[] originalComponents = originalObject.GetComponents<Component>();
        Component[] cloneComponents = cloneObject.GetComponents<Component>();
        int count = Mathf.Min(originalComponents.Length, cloneComponents.Length);
        for (int i = 0; i < count; i++)
        {
            if (originalComponents[i] == null || cloneComponents[i] == null)
            {
                continue;
            }

            referenceMap[originalComponents[i]] = cloneComponents[i];
        }
    }

    private static void RemapObjectReferences(
        GameObject cloneRoot,
        Dictionary<UnityEngine.Object, UnityEngine.Object> referenceMap)
    {
        Component[] components = cloneRoot.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null)
            {
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.GetIterator();
            bool changed = false;

            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                UnityEngine.Object reference = property.objectReferenceValue;
                if (reference == null || !referenceMap.TryGetValue(reference, out UnityEngine.Object cloneReference))
                {
                    continue;
                }

                property.objectReferenceValue = cloneReference;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    private static string GetRelativePath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private static void CopyConfiguredRootsIntoScene(
        string variableName,
        string sourceScenePath,
        Scene destinationScene,
        HashSet<string> stripComponents,
        bool clearSpriteRendererSprite)
    {
        HashSet<string> selectedRoots = LoadEnvironmentList(variableName);
        if (selectedRoots.Count == 0)
        {
            CopyAllRootsIntoScene(sourceScenePath, destinationScene, stripComponents, clearSpriteRendererSprite);
            return;
        }

        CopySelectedRootsIntoScene(sourceScenePath, destinationScene, selectedRoots, stripComponents, clearSpriteRendererSprite);
    }

    private static void LogSceneRoots(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            string componentNames = string.Join(", ", root.GetComponents<Component>()
                .Select(component => component == null ? "<Missing>" : component.GetType().Name));
            Debug.Log($"[Phase9CrashIsolationBuild] Source root | Scene={scenePath} | Root={root.name} | Active={root.activeSelf} | Components={componentNames}");
        }

        EditorSceneManager.CloseScene(scene, true);
    }

    private static HashSet<string> LoadEnvironmentList(string variableName)
    {
        string value = Environment.GetEnvironmentVariable(variableName);
        if (value == null)
        {
            return new HashSet<string>();
        }

        return SplitEnvironmentList(value).ToHashSet();
    }

    private static HashSet<string> GetDefaultPhase9IntegratedRoots()
    {
        return new HashSet<string>
        {
            "Main Camera",
            "Global Light 2D",
            "静态层",
            "Walkable",
            "_BridgeRoot",
            "水体内部静态",
            "水体外部流动",
            "NavMesh-walkable",
            "NavMeshWalkableBakeMesh_FromSprite",
            "Phase9_NavMeshSurface",
            "NavMeshWalkableBakeMesh_FromAlpha",
            "DepthSortManager",
            "HeroineRoot",
            "HUDCanvas",
            "EventSystem",
            "卢客",
            "周掌柜",
            "徐老伯",
            "陈书院",
            "P10_CH01_FlowController"
        };
    }

    private static bool ShouldIncludeRoot(string rootName, HashSet<string> selectedNames)
    {
        if (selectedNames.Contains(rootName))
        {
            return true;
        }

        return selectedNames.Any(name => name.StartsWith(rootName + "/", StringComparison.Ordinal));
    }

    private static void PruneCloneToAllowedPaths(GameObject cloneRoot, HashSet<string> selectedNames, string rootName)
    {
        var allowedPaths = selectedNames
            .Where(name => name.StartsWith(rootName + "/", StringComparison.Ordinal))
            .ToArray();

        if (allowedPaths.Length == 0)
        {
            return;
        }

        PruneTransformRecursive(cloneRoot.transform, allowedPaths, rootName);
    }

    private static void PruneTransformRecursive(Transform transform, string[] allowedPaths, string currentPath)
    {
        var children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }

        foreach (Transform child in children)
        {
            string childPath = $"{currentPath}/{child.name}";
            bool keepChild = allowedPaths.Any(path => path == childPath);
            bool keepDescendants = allowedPaths.Any(path => path.StartsWith(childPath + "/", StringComparison.Ordinal));

            if (!keepChild && !keepDescendants)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
                continue;
            }

            PruneTransformRecursive(child, allowedPaths, childPath);
        }
    }

    private static void StripComponentsByName(GameObject root, HashSet<string> stripComponents, bool clearSpriteRendererSprite)
    {
        foreach (var component in root.GetComponentsInChildren<Component>(true))
        {
            if (component == null || component is Transform)
            {
                continue;
            }

            if (clearSpriteRendererSprite && component is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.sprite = null;
            }

            if (!stripComponents.Contains(component.GetType().Name))
            {
                continue;
            }

            UnityEngine.Object.DestroyImmediate(component);
        }
    }

    private static IEnumerable<string> SplitEnvironmentList(string value)
    {
        return value
            .Split(new[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item) && !item.StartsWith("#", StringComparison.Ordinal));
    }

    private static string GetSafeOutputName()
    {
        string outputName = Environment.GetEnvironmentVariable("PHASE9_SUBSET_OUTPUT");
        if (string.IsNullOrWhiteSpace(outputName))
        {
            return Path.GetFileNameWithoutExtension(SubsetOutputPath);
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            outputName = outputName.Replace(invalidChar, '_');
        }

        return outputName.Trim();
    }

    private static string GetSafeEnvironmentOutputName(string variableName, string fallbackName)
    {
        string outputName = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(outputName))
        {
            outputName = fallbackName;
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            outputName = outputName.Replace(invalidChar, '_');
        }

        return outputName.Trim();
    }

    private static bool IsEnvironmentFlagDisabled(string variableName)
    {
        string value = Environment.GetEnvironmentVariable(variableName);
        return string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnvironmentFlagEnabled(string variableName)
    {
        string value = Environment.GetEnvironmentVariable(variableName);
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
