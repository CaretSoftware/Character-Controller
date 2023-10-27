using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Move))]
public class MoveStateEditor : CharacterStateEditor {
    SerializedProperty groundSmoothTime;
    SerializedProperty characterMaxSpeed;

    private GUIContent guiContentGroundSmoothTime;
    private GUIContent guiContentCharacterMaxSpeed;
    
    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        groundSmoothTime = serializedObject.FindProperty("groundSmoothTime");
        characterMaxSpeed = serializedObject.FindProperty("characterMaxSpeed");
        guiContentGroundSmoothTime = new GUIContent(groundSmoothTime.name);
        guiContentCharacterMaxSpeed = new GUIContent(characterMaxSpeed.name);

    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(groundSmoothTime, 0f, 1f, guiContentGroundSmoothTime);
        EditorGUILayout.Slider(characterMaxSpeed, 0f, 10f, guiContentCharacterMaxSpeed);
    }
}
