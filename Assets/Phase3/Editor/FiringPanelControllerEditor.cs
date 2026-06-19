using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FiringPanelController))]
public class FiringPanelControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Layout Tools", EditorStyles.boldLabel);

        FiringPanelController controller = (FiringPanelController)target;

        if (GUILayout.Button("Apply Firing Layout Now"))
        {
            Undo.RecordObject(controller, "Apply Firing Layout Now");
            controller.ApplyFiringLayoutNow();
            EditorUtility.SetDirty(controller);
        }

        if (GUILayout.Button("Reset Phase3 Firing Window Defaults"))
        {
            Undo.RecordObject(controller, "Reset Phase3 Firing Window Defaults");
            controller.ResetPhase3FiringWindowDefaults();
            EditorUtility.SetDirty(controller);
        }

        if (GUILayout.Button("Capture Current Manual UI Layout"))
        {
            Undo.RecordObject(controller, "Capture Current Manual UI Layout");
            controller.CaptureCurrentManualUiLayout();
            EditorUtility.SetDirty(controller);
        }
    }
}
