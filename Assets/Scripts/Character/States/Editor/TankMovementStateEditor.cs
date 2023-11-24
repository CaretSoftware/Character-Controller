using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TankMovement))]
public class TankMovementStateEditor : CharacterStateEditor {
    private SerializedProperty forwardVelocity;
    private SerializedProperty rotationVelocity;
    private SerializedProperty smoothAcceleration;
    private SerializedProperty smoothDeceleration;
    private SerializedProperty smoothRotationAcceleration;
    private SerializedProperty reloadTime;
    
    private GUIContent guiContentForwardVelocity;
    private GUIContent guiContentRotationVelocity;
    private GUIContent guiContentSmoothAcceleration;
    private GUIContent guiContentSmoothDeceleration;
    private GUIContent guiContentSmoothRotationAcceleration;
    private GUIContent guiContentReloadTime;

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;

        forwardVelocity = serializedObject.FindProperty("forwardVelocity");
        rotationVelocity = serializedObject.FindProperty("rotationVelocity");
        smoothAcceleration = serializedObject.FindProperty("smoothAcceleration");
        smoothDeceleration = serializedObject.FindProperty("smoothDeceleration");
        smoothRotationAcceleration = serializedObject.FindProperty("smoothRotationAcceleration");
        reloadTime = serializedObject.FindProperty("reloadTime");

        guiContentForwardVelocity = new GUIContent(forwardVelocity.name);
        guiContentRotationVelocity = new GUIContent(rotationVelocity.name);
        guiContentSmoothAcceleration = new GUIContent(smoothAcceleration.name);
        guiContentSmoothDeceleration = new GUIContent(smoothDeceleration.name);
        guiContentSmoothRotationAcceleration = new GUIContent(rotationVelocity.name);
        guiContentReloadTime = new GUIContent(reloadTime.name);
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(forwardVelocity, 0f, 20f, guiContentForwardVelocity);
        EditorGUILayout.Slider(rotationVelocity, 0f, 200f, guiContentRotationVelocity);
        EditorGUILayout.Slider(smoothAcceleration, 0f, 1f, guiContentSmoothAcceleration);
        EditorGUILayout.Slider(smoothDeceleration, 0f, 1f, guiContentSmoothDeceleration);
        EditorGUILayout.Slider(smoothRotationAcceleration, 0f, 2f, guiContentSmoothRotationAcceleration);
        EditorGUILayout.Slider(reloadTime, 0f, 10f, guiContentReloadTime);
    }

}