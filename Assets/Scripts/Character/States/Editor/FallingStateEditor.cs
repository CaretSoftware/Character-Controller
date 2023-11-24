using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Falling))]
public class FallingStateEditor : CharacterStateEditor {
    SerializedProperty coyoteTime;
    SerializedProperty fallingAcceleration;
    SerializedProperty terminalVelocity;
    
    private GUIContent guiContentCoyoteTime;
    private GUIContent guiContentFallingAcceleration;

    //[SerializeField] private float fallingAcceleration = 19.62f;

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        
        coyoteTime = serializedObject.FindProperty("coyoteTime");
        fallingAcceleration = serializedObject.FindProperty("fallingAcceleration");
        terminalVelocity = serializedObject.FindProperty("terminalVelocity");
        
        guiContentCoyoteTime = new GUIContent(coyoteTime.name);
        guiContentFallingAcceleration = new GUIContent(fallingAcceleration.name);
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(coyoteTime, 0f, 1f, guiContentCoyoteTime);
        EditorGUILayout.Slider(fallingAcceleration, 0f, 50f, guiContentFallingAcceleration);
        terminalVelocity.floatValue = EditorGUILayout.FloatField(terminalVelocity.name, terminalVelocity.floatValue);
    }
}
