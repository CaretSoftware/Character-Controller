using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Slide))]
public class SlideStateEditor : CharacterStateEditor {
    private SerializedProperty slipSpeed;
    private SerializedProperty minSlideVelocity;
    private SerializedProperty inputSmoothTime;
    private SerializedProperty rotationSmoothTime;
    
    private GUIContent guiContentSlipSpeed;
    private GUIContent guiContentMinSlideVelocity;
    private GUIContent guiContentInputSmoothTime;
    private GUIContent guiContentRotationSmoothTime;

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        
        slipSpeed = serializedObject.FindProperty("slipSpeed");
        minSlideVelocity = serializedObject.FindProperty("minSlideVelocity");
        inputSmoothTime = serializedObject.FindProperty("inputSmoothTime");
        rotationSmoothTime = serializedObject.FindProperty("rotationSmoothTime");
        
        guiContentSlipSpeed = new GUIContent (slipSpeed.name);
        guiContentMinSlideVelocity = new GUIContent (minSlideVelocity.name);
        guiContentInputSmoothTime = new GUIContent (inputSmoothTime.name);
        guiContentRotationSmoothTime = new GUIContent (rotationSmoothTime.name);
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
        
        EditorGUILayout.Slider(slipSpeed, 0f, 20f, guiContentSlipSpeed);
        EditorGUILayout.Slider(minSlideVelocity, 0f, 10f, guiContentMinSlideVelocity);
        EditorGUILayout.Slider(inputSmoothTime, 0f, 1f, guiContentInputSmoothTime);
        EditorGUILayout.Slider(rotationSmoothTime, 0f, 1f, guiContentRotationSmoothTime);
    }
}