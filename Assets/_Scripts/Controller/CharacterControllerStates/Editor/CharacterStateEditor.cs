﻿using System;
using UnityEditor;
using UnityEngine;

public static class SelectedCharacterState {
    public static CharacterState characterState = null;
}

public class CharacterStateEditor : Editor {
    public static Action<CharacterState> stateSelected;

    protected string targetName;

    private GUIStyle labelStyle;
    private GUIStyle LabelStyle =>
        labelStyle ??= new GUIStyle(GUI.skin.label) {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerLeft,
            wordWrap = false,
            padding = new RectOffset(10, 4, 4, 4),
            border = new RectOffset(3, 3, 3, 3),
            contentOffset = Vector2.zero,
            normal = { textColor = Color.white }
        };

    
    private Color inspectorGray = new Color(56 / 256f, 56 / 256f, 56 / 256f, 1f);
    private Color shadedGray = new Color(.175f, .175f, .18f, 1f);
    private GUIStyle tabStyle;

    protected virtual void OnEnable() => stateSelected += StateSelected;

    protected virtual void OnDisable() => stateSelected -= StateSelected;

    protected void StateSelected(CharacterState characterState) => 
            SelectedCharacterState.characterState = characterState;

    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        // Selection
        bool selected = SelectedCharacterState.characterState != null && SelectedCharacterState.characterState.GetType() == target.GetType();
        int selectedOffset = selected ? 5 : 0;
        LabelStyle.contentOffset = new Vector2(0, -selectedOffset);
        
        EditorGUILayout.BeginHorizontal();
        
        // State Label
        GUIContent labelContent = new GUIContent(targetName);
        Vector2 labelSize = LabelStyle.CalcSize(labelContent);
        EditorGUILayout.LabelField(labelContent, labelStyle, GUILayout.Width(labelSize.x));
        
        Rect lineRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

        // Draw Tab Outlines
        EditorGUI.DrawRect(new Rect(20, lineRect.y - selectedOffset, labelSize.x, 1), shadedGray);
        EditorGUI.DrawRect(new Rect(20, lineRect.y - selectedOffset, 1, EditorGUIUtility.singleLineHeight), shadedGray);
        EditorGUI.DrawRect(new Rect(20 + labelSize.x, lineRect.y - selectedOffset, 1, EditorGUIUtility.singleLineHeight), shadedGray);

        // Draw Tab Shading
        for (int i = 0; i < EditorGUIUtility.singleLineHeight; i++) {
            Color c = Color.Lerp(inspectorGray, shadedGray, (i / EditorGUIUtility.singleLineHeight));
            EditorGUI.DrawRect(new Rect(lineRect.x, lineRect.y + i - selectedOffset, lineRect.width, 1), c);
            EditorGUI.DrawRect(new Rect(0, lineRect.y + i - selectedOffset, 20, 1), c);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Show Variables
        DisplayFields();
        
        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void DisplayFields() { }
    
    /*  // Draw Color behind inspector
     *  serializedObject.Update();
        screenRect = GUILayoutUtility.GetRect(1, 1);
        vertRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(new Rect(screenRect.x - 13, screenRect.y - 1, screenRect.width + 17, vertRect.height + 9), style.background);
        ...
        ...
        <Draw your properties>
        ...
        ...
        EditorGUILayout.EndVertical();
     */
    
    
    // Display Dialogue Window
    //if (EditorUtility.DisplayDialog("Warning!", 
    //    "Are you sure you want to delete the wave?", "Yes", "No")) {
    //}
    
    
    // Vertical Slider
    //EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.MinHeight(50));
    //EditorGUILayout.LabelField("", GUI.skin.verticalSliderThumb);
}
