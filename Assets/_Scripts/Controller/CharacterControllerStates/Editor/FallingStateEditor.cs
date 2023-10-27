using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Falling))]
public class FallingStateEditor : CharacterStateEditor {
    SerializedProperty coyoteTime;
    SerializedProperty terminalVelocity;
    
    private GUIContent guiContentCoyoteTime;

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        coyoteTime = serializedObject.FindProperty("coyoteTime");
        terminalVelocity = serializedObject.FindProperty("terminalVelocity");
        guiContentCoyoteTime = new GUIContent(coyoteTime.name);
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(coyoteTime, 0f, 1f, guiContentCoyoteTime);
        terminalVelocity.floatValue = EditorGUILayout.FloatField(terminalVelocity.name, terminalVelocity.floatValue);
    }
}
