using UnityEditor;
using UnityEngine;

public class CharacterStateEditor : Editor {
    protected string targetName;

    private GUIStyle labelStyle;
    private GUIStyle LabelStyle =>
        labelStyle ??= new GUIStyle(GUI.skin.label) {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerLeft,
            wordWrap = false,
            normal = { textColor = Color.white }
        };

    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        EditorGUILayout.LabelField(targetName, LabelStyle);
        
        DisplayFields();
        
        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void DisplayFields() { }
}
