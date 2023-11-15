using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraFollowClose))]
public class FPSClampEditor : Editor {
    private SerializedProperty smoothDampMinVal;
    private SerializedProperty smoothDampMaxVal;
    private SerializedProperty clampLookupMin;
    private SerializedProperty clampLookupMax;

    private const int _minLimit = 1;
    private const int _maxLimit = 179;
    private float _minVal = _minLimit;
    private float _maxVal = _maxLimit;

    private void OnEnable() {
        smoothDampMinVal = serializedObject.FindProperty("smoothDampMinVal");
        smoothDampMaxVal = serializedObject.FindProperty("smoothDampMaxVal");
        clampLookupMin = serializedObject.FindProperty("clampLookupMin");
        clampLookupMax = serializedObject.FindProperty("clampLookupMax");
        
        serializedObject.Update();
        _minVal = 180 - clampLookupMin.floatValue;
        _maxVal = 180 - clampLookupMax.floatValue;
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
		
        serializedObject.Update();
        LookUpClampMinMaxSlider();
        SmoothDampSmoothTimeMinMaxSlider();
        serializedObject.ApplyModifiedProperties();
    }

    private void LookUpClampMinMaxSlider() {
        EditorGUILayout.LabelField("Clamp Look Range:", $"{Mathf.Floor(_minVal)} - {Mathf.Ceil(_maxVal)}");
        EditorGUILayout.MinMaxSlider(ref _minVal, ref _maxVal, _minLimit, _maxLimit);

        clampLookupMax.floatValue = 180 - Mathf.CeilToInt(_maxVal);
        clampLookupMin.floatValue = 180 - (int)_minVal;
    }

    private void SmoothDampSmoothTimeMinMaxSlider() {
        float refMinValue = smoothDampMinVal.floatValue;
        float refMaxVal = smoothDampMaxVal.floatValue;
        EditorGUILayout.LabelField("SmoothDamp Min - Max: ", $"{smoothDampMinVal.floatValue:N2} - {smoothDampMaxVal.floatValue:N2}");
        EditorGUILayout.MinMaxSlider(ref refMinValue, ref refMaxVal, float.Epsilon, 1.0f);
		
        smoothDampMinVal.floatValue = refMinValue;
        smoothDampMaxVal.floatValue = refMaxVal;
    }
}