using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Jump))]
public class JumpStateEditor : CharacterStateEditor {
    private SerializedProperty jumpBufferTime;

    SerializedProperty jumpHeight;
    SerializedProperty airSmoothTime;
    SerializedProperty airControlSmoothTime;
    SerializedProperty gravity;
    SerializedProperty fallGravityMultiplier;
    SerializedProperty minApexVelocityThreshold;
    SerializedProperty maxApexVelocityThreshold;

    void OnEnable() {

        targetName = target.name;
        jumpHeight = serializedObject.FindProperty("jumpHeight");
        jumpBufferTime = serializedObject.FindProperty("jumpBufferDuration");
        airSmoothTime = serializedObject.FindProperty("airSmoothTime");
        airControlSmoothTime = serializedObject.FindProperty("airControlSmoothTime");
        gravity = serializedObject.FindProperty("gravity");
        fallGravityMultiplier = serializedObject.FindProperty("fallGravityMultiplier");
        minApexVelocityThreshold = serializedObject.FindProperty("minApexVelocityThreshold");
        maxApexVelocityThreshold = serializedObject.FindProperty("maxApexVelocityThreshold");
    }

    protected override void DisplayFields() {

        EditorGUILayout.Slider(jumpHeight, 0f, 10f, new GUIContent (jumpHeight.name));
        EditorGUILayout.Slider(jumpBufferTime, 0f, 1f, new GUIContent (jumpBufferTime.name));
        EditorGUILayout.Slider(airSmoothTime, 0f, 2f, new GUIContent (airSmoothTime.name));
        EditorGUILayout.Slider(airControlSmoothTime, 0f, 1f, new GUIContent (airControlSmoothTime.name));
        EditorGUILayout.Slider(gravity, -20f, 0f, new GUIContent (gravity.name));
        EditorGUILayout.Slider(fallGravityMultiplier, 1f, 20f, new GUIContent (fallGravityMultiplier.name));
        
        float min = minApexVelocityThreshold.floatValue;
        float max = maxApexVelocityThreshold.floatValue;
        EditorGUILayout.MinMaxSlider("Apex Velocity Thresholds", ref min, ref max, -2, 2);
        minApexVelocityThreshold.floatValue = min;
        maxApexVelocityThreshold.floatValue = max;
    }
}
