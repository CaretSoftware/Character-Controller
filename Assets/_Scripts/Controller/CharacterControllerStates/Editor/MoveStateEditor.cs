using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Move))]
public class MoveStateEditor : CharacterStateEditor {
    SerializedProperty characterMaxSpeed;
    SerializedProperty groundSmoothTime;

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        groundSmoothTime = serializedObject.FindProperty("groundSmoothTime");
        characterMaxSpeed = serializedObject.FindProperty("characterMaxSpeed");
    }

    protected override void DisplayFields() {
        EditorGUILayout.Slider(groundSmoothTime, 0f, 1f, new GUIContent (groundSmoothTime.name));
        EditorGUILayout.Slider(characterMaxSpeed, 0f, 10f, new GUIContent (characterMaxSpeed.name));
    }
}
