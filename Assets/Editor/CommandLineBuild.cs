using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class CommandLineBuild
{
    [MenuItem("Build/Build Windows x64")]
    public static void BuildWindows()
    {
        string[] scenes = new string[]
        {
            "Assets/Phase9/Scenes/SampleScene.unity"
        };

        string buildPath = "C:/Users/lenovo/Desktop/JingdezhenKiln_Build/JingdezhenKiln.exe";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.Development | BuildOptions.AllowDebugging;

        Debug.Log("[CommandLineBuild] Starting build...");
        Debug.Log("[CommandLineBuild] Scenes: " + string.Join(", ", scenes));
        Debug.Log("[CommandLineBuild] Target: StandaloneWindows64");
        Debug.Log("[CommandLineBuild] Output: " + buildPath);

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[CommandLineBuild] Build SUCCEEDED: " + report.summary.totalSize + " bytes");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.Log("[CommandLineBuild] Build FAILED: " + report.summary.result);
            foreach (var step in report.steps)
            {
                foreach (var message in step.messages)
                {
                    if (message.type == LogType.Error)
                    {
                        Debug.LogError("[CommandLineBuild] ERROR: " + message.content);
                    }
                }
            }
            EditorApplication.Exit(1);
        }
    }
}
