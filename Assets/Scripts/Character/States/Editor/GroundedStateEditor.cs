using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grounded))]
public class GroundedStateEditor : CharacterStateEditor {
    SerializedProperty groundedGravity;
    private GUIContent guiContentGroundedGravity;

    
    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        groundedGravity = serializedObject.FindProperty("groundedGravity");
        guiContentGroundedGravity = new GUIContent(groundedGravity.name);
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(groundedGravity, 0f, -30f, guiContentGroundedGravity);
    }
}
