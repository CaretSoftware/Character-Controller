using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SelectedCharacterState {
    public static CharacterState characterState = null;
}

public class CharacterStateEditor : Editor {
    public static Action<CharacterState> stateSelected;

    protected string targetName = string.Empty;

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
    private static int staticTabOffset;

    protected virtual void OnEnable() {
        guiContentLabelContent = new GUIContent(targetName);
        stateSelected += StateSelected;
    }

    protected virtual void OnDisable() => stateSelected -= StateSelected;

    protected void StateSelected(CharacterState characterState) => 
            SelectedCharacterState.characterState = characterState;

    protected GUIContent guiContentLabelContent;

    public 
        //override 
        void OnInspectorGUIXXXXXXXXXX() {
        serializedObject.Update();
        
        // Selection
        bool selected = SelectedCharacterState.characterState != null && SelectedCharacterState.characterState.GetType() == target.GetType();
        int previousSelectedOffset = previousSelected ? -5 : 0;
        int selectedOffset = selected ? 5 : previousSelectedOffset;
        LabelStyle.contentOffset = new Vector2(0, -selectedOffset);
        
        EditorGUILayout.BeginHorizontal();
        
        // State Label
        guiContentLabelContent.text = targetName;
        GUIContent labelContent = guiContentLabelContent;
        Vector2 labelSize = LabelStyle.CalcSize(labelContent);
        EditorGUILayout.LabelField(labelContent, LabelStyle, GUILayout.Width(labelSize.x));
        
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
        DisplayFields(selected);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    public override void OnInspectorGUI() {
        serializedObject.Update();

        // Selection
        bool selected = SelectedCharacterState.characterState != null && SelectedCharacterState.characterState.GetType() == target.GetType();
        int previousSelectedOffset = previousSelected ? -5 : 0;
        int selectedOffset = selected ? 5 : previousSelectedOffset;
        LabelStyle.contentOffset = new Vector2(0, -selectedOffset);

        EditorGUILayout.BeginHorizontal();

        // State Label
        GUIContent labelContent = GUIContentLabelContent();
        Vector2 labelSize = LabelStyle.CalcSize(labelContent);
        EditorGUILayout.LabelField(labelContent, LabelStyle, GUILayout.Width(labelSize.x));

        Rect lineRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

        // Draw Tab Outlines
        DrawOutline(new Rect(20, lineRect.y - selectedOffset, labelSize.x, 1), shadedGray);
        DrawOutline(new Rect(20, lineRect.y - selectedOffset, 1, EditorGUIUtility.singleLineHeight), shadedGray);
        DrawOutline(new Rect(20 + labelSize.x, lineRect.y - selectedOffset, 1, EditorGUIUtility.singleLineHeight), shadedGray);

        // Draw Tab Shading
        for (int i = 0; i < EditorGUIUtility.singleLineHeight; i++) {
            Color c = Color.Lerp(inspectorGray, shadedGray, (i / EditorGUIUtility.singleLineHeight));

            DrawOutline(new Rect(lineRect.x, lineRect.y + i - selectedOffset, lineRect.width, 1), c);
            DrawOutline(new Rect(0, lineRect.y + i - selectedOffset, 20, 1), c);
        }

        EditorGUILayout.EndHorizontal();

        // Show Variables
        DisplayFields(selected);

        serializedObject.ApplyModifiedProperties();
    }

    private GUIContent GUIContentLabelContent() {
        guiContentLabelContent.text = targetName;
        return guiContentLabelContent;
    }

    private void DrawOutline(Rect rect, Color color) {
        EditorGUI.DrawRect(rect, color);
    }


    private static bool previousSelected;
    protected virtual void DisplayFields(bool selected) {
        int previousSelectedOffset = previousSelected ? +5 : 0;
        EditorGUILayout.Space(selected ? -5 : previousSelectedOffset);
        previousSelected = selected;
    }
    
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
    
    /*
    GUI.BeginClip(rect);
    Handles.color = Color.red;
    Handles.DrawAAPolyLine(
        Texture2D.whiteTexture,
        15,
        Vector3.zero,
        new Vector3(120, 91, 0),
        new Vector3(220, 91, 0),
        new Vector3(350, 20, 0));
    GUI.EndClip();
    */
    
    // Display Dialogue Window
    //if (EditorUtility.DisplayDialog("Warning!", 
    //    "Are you sure you want to delete the wave?", "Yes", "No")) {
    //}
    
    
    // Vertical Slider
    //EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.MinHeight(50));
    //EditorGUILayout.LabelField("", GUI.skin.verticalSliderThumb);
}
