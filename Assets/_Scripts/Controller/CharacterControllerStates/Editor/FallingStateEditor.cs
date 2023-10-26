using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Falling))]
public class FallingStateEditor : CharacterStateEditor {
    SerializedProperty coyoteTime;
    SerializedProperty terminalVelocity;

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        coyoteTime = serializedObject.FindProperty("coyoteTime");
        terminalVelocity = serializedObject.FindProperty("terminalVelocity");
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(coyoteTime, 0f, 1f, new GUIContent (coyoteTime.name));
        terminalVelocity.floatValue = EditorGUILayout.FloatField(terminalVelocity.name, terminalVelocity.floatValue);
    }
}
