using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Idle))]
public class IdleStateEditor : CharacterStateEditor {
    private SerializedProperty timeToBored;

    private void OnEnable() {
        targetName = target.name;
        timeToBored = serializedObject.FindProperty("timeToBored");
    }

    protected override void DisplayFields() {
        EditorGUILayout.Slider(timeToBored, 0f, 10f, new GUIContent (timeToBored.name));
    }
}