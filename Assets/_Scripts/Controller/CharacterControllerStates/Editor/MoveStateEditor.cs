using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Move))]
public class MoveStateEditor : CharacterStateEditor {
    SerializedProperty groundSmoothTime;
    SerializedProperty characterMaxSpeed;
    SerializedProperty rotationSmoothTime;

    private GUIContent guiContentGroundSmoothTime;
    private GUIContent guiContentCharacterMaxSpeed;
    private GUIContent guiContentRotationSmoothTime;
    
    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        
        groundSmoothTime = serializedObject.FindProperty("groundSmoothTime");
        characterMaxSpeed = serializedObject.FindProperty("characterMaxSpeed");
        rotationSmoothTime = serializedObject.FindProperty("rotationSmoothTime");
        
        guiContentGroundSmoothTime = new GUIContent(groundSmoothTime.name);
        guiContentCharacterMaxSpeed = new GUIContent(characterMaxSpeed.name);
        guiContentRotationSmoothTime = new GUIContent(rotationSmoothTime.name);

    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(groundSmoothTime, 0f, 1f, guiContentGroundSmoothTime);
        EditorGUILayout.Slider(characterMaxSpeed, 0f, 10f, guiContentCharacterMaxSpeed);
        EditorGUILayout.Slider(rotationSmoothTime, 0f, 1f, guiContentRotationSmoothTime);
    }
}
