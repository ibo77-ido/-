using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuildMenu
{
    private const string OutputPath = "Builds/WebGL";

    [MenuItem("Build/WebGL/Build")]
    public static void BuildWebGL()
    {
        BuildInternal(false);
    }

    [MenuItem("Build/WebGL/Build Development")]
    public static void BuildWebGLDevelopment()
    {
        BuildInternal(true);
    }

    private static void BuildInternal(bool development)
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes found in Build Settings.");
        }

        Directory.CreateDirectory(OutputPath);

        EditorUserBuildSettings.development = development;
        EditorUserBuildSettings.connectProfiler = development;

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            options = development ? BuildOptions.Development : BuildOptions.None
        };

        Debug.Log($"[WebGLBuildMenu] Building WebGL to '{Path.GetFullPath(OutputPath)}' with {scenes.Length} scene(s).");

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        Debug.Log($"[WebGLBuildMenu] Build finished: {summary.result}, size={summary.totalSize} bytes, time={summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"WebGL build failed with result: {summary.result}");
        }
    }
}
