using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FPSLimiter))]
public class FPSLimiterEditor : Editor{
    private Color deselectedColor = new Color(.15f, .15f, .15f, 1f);
    private const string showFramerateProtertyName = "showFramerate";
    
    public override void OnInspectorGUI() {
        FPSLimiter fpsLimiter = (FPSLimiter)target;
        
        //DrawDefaultInspector();
        
        GUIStyle selectedButtonStyle = new GUIStyle(GUI.skin.button);
        selectedButtonStyle.normal.textColor = Color.white;
        selectedButtonStyle.hover.textColor = Color.white;
        selectedButtonStyle.fontStyle = FontStyle.Bold;
        
        GUIStyle deselectedButtonStyle = new GUIStyle(GUI.skin.button);
        deselectedButtonStyle.normal.textColor = deselectedColor;
        deselectedButtonStyle.hover.textColor = deselectedColor;
        int numFrameRates = fpsLimiter.frameRates.Length;

        SerializedProperty showFrameRateProperty = serializedObject.FindProperty(showFramerateProtertyName);

        EditorGUILayout.BeginHorizontal();

        bool show = showFrameRateProperty.boolValue;
        GUIContent eyeOpen = EditorGUIUtility.IconContent("d_animationvisibilitytoggleon");
        GUIContent eyeClosed = EditorGUIUtility.IconContent("d_animationvisibilitytoggleoff");
        if (GUILayout.Button(show ? eyeOpen : eyeClosed, GUILayout.Width(30)))
            fpsLimiter.showFramerate = !show;

        for (int i = 0; i < numFrameRates; i++) {
            if (GUILayout.Button(fpsLimiter.frameRates[i].ToString(), 
                    fpsLimiter.frameRates[i] == fpsLimiter.TargetFrameRate ? deselectedButtonStyle : selectedButtonStyle))
                fpsLimiter.TargetFrameRate = fpsLimiter.frameRates[i];
        }
        if (GUILayout.Button("∞",
                fpsLimiter.TargetFrameRate == 0 ? deselectedButtonStyle : selectedButtonStyle))
            fpsLimiter.TargetFrameRate = 0;
        
        EditorGUILayout.EndHorizontal();
    }
}
