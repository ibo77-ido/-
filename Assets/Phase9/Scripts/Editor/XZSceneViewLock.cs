using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class XZSceneViewLock
{
    private const string EnabledPrefKey = "Phase9.XZSceneViewLock.Enabled";
    private const string InitializedPrefKey = "Phase9.XZSceneViewLock.Initialized";
    private static readonly Quaternion XZTopDownRotation = Quaternion.Euler(90f, 0f, 0f);

    static XZSceneViewLock()
    {
        if (!EditorPrefs.GetBool(InitializedPrefKey, false))
        {
            EditorPrefs.SetBool(InitializedPrefKey, true);
            EditorPrefs.SetBool(EnabledPrefKey, true);
        }

        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.delayCall += ApplyToAllSceneViews;
    }

    [MenuItem("Phase9/View/Toggle XZ Scene View Lock %#x")]
    public static void ToggleLock()
    {
        SetEnabled(!IsEnabled());
    }

    [MenuItem("Phase9/View/Enable XZ Scene View Lock")]
    public static void EnableLock()
    {
        SetEnabled(true);
    }

    [MenuItem("Phase9/View/Disable XZ Scene View Lock")]
    public static void DisableLock()
    {
        SetEnabled(false);
    }

    [MenuItem("Phase9/View/Frame Scene View On XZ Once")]
    public static void FrameXZOnce()
    {
        ApplyToAllSceneViews();
    }

    [MenuItem("Phase9/View/Toggle XZ Scene View Lock %#x", true)]
    private static bool ToggleLockValidate()
    {
        Menu.SetChecked("Phase9/View/Toggle XZ Scene View Lock", IsEnabled());
        return true;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!IsEnabled())
        {
            return;
        }

        Event current = Event.current;
        if (current == null || current.type == EventType.Layout || current.type == EventType.Repaint)
        {
            Apply(sceneView, false);
        }
    }

    private static bool IsEnabled()
    {
        return EditorPrefs.GetBool(EnabledPrefKey, false);
    }

    private static void SetEnabled(bool enabled)
    {
        EditorPrefs.SetBool(EnabledPrefKey, enabled);
        if (enabled)
        {
            ApplyToAllSceneViews();
        }

        SceneView.RepaintAll();
    }

    private static void ApplyToAllSceneViews()
    {
        SceneView[] sceneViews = Resources.FindObjectsOfTypeAll<SceneView>();
        for (int i = 0; i < sceneViews.Length; i++)
        {
            Apply(sceneViews[i], true);
        }
    }

    private static void Apply(SceneView sceneView, bool instant)
    {
        if (sceneView == null)
        {
            return;
        }

        Vector3 pivot = sceneView.pivot;
        float size = Mathf.Max(0.01f, sceneView.size);

        sceneView.in2DMode = false;
        sceneView.orthographic = true;

        if (Quaternion.Angle(sceneView.rotation, XZTopDownRotation) > 0.05f || !sceneView.orthographic)
        {
            sceneView.LookAt(pivot, XZTopDownRotation, size, true, instant);
        }

        sceneView.Repaint();
    }
}
