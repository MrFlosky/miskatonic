#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// This class enables custom editor for mission data scriptable objects,
/// only showing appropriate fields for a given reward type.
/// </summary>
[CustomEditor(typeof(MissionData))]
public class MissionDataEditor : Editor
{
    // called every frame inspector window is visible
    public override void OnInspectorGUI()
    {
        MissionData mission = (MissionData)target;

        serializedObject.Update();

        // Draw default fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cultistCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnActivateTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("oneTimeUse"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reward"));

        // Conditional fields based on reward type
        if (mission.reward != null)
        {
            if (mission.reward is SkillReward)
            {
                // property fields draws property in inspector
                EditorGUILayout.PropertyField(serializedObject.FindProperty("usages"));
            }
            else if (mission.reward is RewardCultist)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cultistsGranted"));
            }
            else if (mission.reward is NarrativeReward)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("character"));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif