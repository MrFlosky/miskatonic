using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ModifyValsInterface))]
public class BalanceValsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // This draws all your [SerializeField] variables above
        DrawDefaultInspector();
        
        GUILayout.Space(40);
        
        ModifyValsInterface script = (ModifyValsInterface)target;
        
        // Buttons that call functions in script
        if (GUILayout.Button("Apply Changes", GUILayout.Height(30)))
        {
            script.ApplyValues();
        }
        
        if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
        {
            // confirm user wants to reset to default values with dialog box
                bool confirm = EditorUtility.DisplayDialog(
                "Confirm Reset", 
                "Are you sure you want to reset all values to DEFAULTS?", 
                "Yes", 
                "Cancel"
            );
            
            if (confirm)
            {
                script.ResetToDefaults();
            }
        }
        
        EditorGUILayout.HelpBox("After editing values press Apply Changes for them to take effect", MessageType.Info);
    }
}