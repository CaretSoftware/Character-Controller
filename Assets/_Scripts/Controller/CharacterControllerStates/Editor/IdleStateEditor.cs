using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Idle))]
public class IdleStateEditor : CharacterStateEditor {
    private SerializedProperty timeToBored;

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        timeToBored = serializedObject.FindProperty("timeToBored");
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        EditorGUILayout.Slider(timeToBored, 0f, 10f, new GUIContent (timeToBored.name));
    }
}