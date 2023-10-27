using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(Jump))]
public class JumpStateEditor : CharacterStateEditor {
    private SerializedProperty jumpBufferTime;

    SerializedProperty jumpHeight;
    SerializedProperty airSmoothTime;
    SerializedProperty airControlSmoothTime;
    SerializedProperty gravity;
    SerializedProperty fallGravityMultiplier;
    SerializedProperty minApexVelocityThreshold;
    SerializedProperty maxApexVelocityThreshold;
    
    private GUIContent guiContentJumpHeight;
    private GUIContent guiContentjumpBufferTime;
    private GUIContent guiContentairSmoothTime;
    private GUIContent guiContentairControlSmoothTime;
    private GUIContent guiContentgravity;
    private GUIContent guiContentfallGravityMultiplier;
    
    //private static GUIStyle labelStyle;// = EditorStyles.label;
    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
        jumpHeight = serializedObject.FindProperty("jumpHeight");
        jumpBufferTime = serializedObject.FindProperty("jumpBufferDuration");
        airSmoothTime = serializedObject.FindProperty("airSmoothTime");
        airControlSmoothTime = serializedObject.FindProperty("airControlSmoothTime");
        gravity = serializedObject.FindProperty("gravity");
        fallGravityMultiplier = serializedObject.FindProperty("fallGravityMultiplier");
        minApexVelocityThreshold = serializedObject.FindProperty("minApexVelocityThreshold");
        maxApexVelocityThreshold = serializedObject.FindProperty("maxApexVelocityThreshold");
        
        guiContentJumpHeight = new GUIContent(jumpHeight.name);
        guiContentjumpBufferTime = new GUIContent(jumpBufferTime.name);
        guiContentairSmoothTime = new GUIContent(airSmoothTime.name);
        guiContentairControlSmoothTime = new GUIContent(airControlSmoothTime.name);
        guiContentgravity = new GUIContent(gravity.name);
        guiContentfallGravityMultiplier = new GUIContent(fallGravityMultiplier.name);
    }

    private float angle = 0f;
    private static float verticalSliderValue = -90f;

    protected override void DisplayFields(bool selected) {

        base.DisplayFields(selected);
        ///////////////

if (false ){
    //// Vertical slider

        EditorGUILayout.LabelField("Vertical Slider Example");

        float sliderHeight = 100f;
        float sliderWidth = 20f;

        Rect r = EditorGUILayout.GetControlRect(false, sliderHeight);
        // Calculate the position of the slider

        // Draw the slider
        verticalSliderValue = GUI.VerticalSlider(r, verticalSliderValue, -90.0f, 90.0f);

        //EditorGUILayout.LabelField("Value: " + verticalSliderValue.ToString("F1"));




        //// Slanted Text

        angle = verticalSliderValue;

        GUIContent[] labelContents = new[] {
            new GUIContent("Slanted Text"),
            new GUIContent("Slanted Text l"),
            new GUIContent("Slanted Text long"),
            new GUIContent("Slanted Text longest"),
        };

        Vector2 labelSize = Vector2.zero;
        for (int i = 0; i < labelContents.Length; i++) {
            Vector2 currLabelSize = EditorStyles.label.CalcSize(labelContents[i]);
            labelSize = currLabelSize.x > labelSize.x ? currLabelSize : labelSize;
        }

        Rect textRect = EditorGUILayout.GetControlRect(false, labelSize.x);
        Rect off = textRect;

        EditorGUILayout.BeginVertical();

        // Apply the rotation to the GUI
        float padding = 10;
        float startX = off.x;
        off.x -= labelSize.x;
        off.y -= labelSize.x / 2 - 3;
        GUIUtility.RotateAroundPivot(angle, new Vector2(textRect.x, textRect.y));

        for (int i = 0; i < labelContents.Length; i++) {
            // Draw the slanted text using GUI
            labelSize = EditorStyles.label.CalcSize(labelContents[i]);
            off.x = startX - labelSize.x - padding; // Height
            GUI.Label(off, labelContents[i]);
            off.y += labelSize.y * 1.5f; // Width
        }

        // Reset the GUI matrix to avoid affecting other GUI elements
        GUIUtility.RotateAroundPivot(-angle, new Vector2(textRect.x, textRect.y));

        // Use EditorGUI.EndVertical to reset the pivot
        EditorGUILayout.EndVertical();

}

        DrawJumpDiagram();

    ///////////////
        EditorGUILayout.Slider(jumpHeight, 0f, 10f, new GUIContent (guiContentJumpHeight));
        EditorGUILayout.Slider(jumpBufferTime, 0f, 1f, new GUIContent (guiContentjumpBufferTime));
        EditorGUILayout.Slider(airSmoothTime, 0f, 2f, new GUIContent (guiContentairSmoothTime));
        EditorGUILayout.Slider(airControlSmoothTime, 0f, 1f, new GUIContent (guiContentairControlSmoothTime));
        EditorGUILayout.Slider(gravity, -20f, 0f, new GUIContent (guiContentgravity));
        EditorGUILayout.Slider(fallGravityMultiplier, 1f, 20f, new GUIContent (guiContentfallGravityMultiplier));
        float min = minApexVelocityThreshold.floatValue;
        float max = maxApexVelocityThreshold.floatValue;
        EditorGUILayout.MinMaxSlider("Apex Velocity Thresholds", ref min, ref max, -2, 2);
        minApexVelocityThreshold.floatValue = min;
        maxApexVelocityThreshold.floatValue = max;
    }

    private Texture2D _previousTexture2D;
    private static Color _inspectorGray = new Color(0.22352941176f, 0.22352941176f, 0.22352941176f);
    private static Color lineColorZero = new Color(.15f, .15f, .15f);
    private static Color lineColorOne = new Color(.3f, .3f, .3f);
    private void DrawJumpDiagram() {
        Jump component = target as Jump;

        Vector2[] dataPoints = component.JumpGraph(out float zero, out float one);
        
        int inspectorWidth = (int)EditorGUIUtility.currentViewWidth;

        DestroyImmediate(_previousTexture2D);
        Texture2D graphTexture = new Texture2D(inspectorWidth <= 0 ? 100 : inspectorWidth, 100);
        _previousTexture2D = graphTexture;
        Color backgroundColor = _inspectorGray;
        Color graphColor = Color.white;

        // Clear the texture with the background color
        Color[] pixels = new Color[graphTexture.width * graphTexture.height];
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = backgroundColor;
        }

        graphTexture.SetPixels(pixels);

        // Draw the scalar lines
        float height = graphTexture.height;
        for (int i = 0; i < graphTexture.width; i++) {
            graphTexture.SetPixel(i, (int)(height*zero), lineColorZero);
            graphTexture.SetPixel(i, (int)(height*one), lineColorOne);
        }
        
        // Draw the graph line
        for (int i = 0; i < dataPoints.Length - 1; i++) {
            Vector2 startPoint = new Vector2(
                Mathf.Lerp(0, graphTexture.width, dataPoints[i].x),
                Mathf.Lerp(0, graphTexture.height, dataPoints[i].y)
            );
            Vector2 endPoint = new Vector2(
                Mathf.Lerp(0, graphTexture.width, dataPoints[i + 1].x),
                Mathf.Lerp(0, graphTexture.height, dataPoints[i + 1].y)
            );
            DrawLine(graphTexture, startPoint, endPoint, graphColor);
        }

        DrawCapsule(graphTexture, one);
        
        graphTexture.Apply();

        // Display the graph in the inspector
        GUILayout.Label(graphTexture);
    }
    
    private static void DrawCapsule(Texture2D texture, float one) {
        float unitPixels = texture.height * one;

        // Define capsule parameters
        int capsuleWidth = (int)(unitPixels + 1); // Width of the capsule
        int capsuleHeight = (int)(unitPixels * 2f); // Height of the capsule
        int centerX = capsuleWidth / 2  + 1;
        int centerY = (int)(unitPixels * 1.5f);
        int radius = capsuleWidth / 2;

        // Draw the capsule
        for (int x = 2; x < capsuleWidth; x++) {
            for (int y = 0; y < capsuleHeight && y < texture.height; y++) {
                // Calculate the distance from the center of the capsule
                float distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2));
                float distance2 = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - radius, 2));
                float min = Mathf.Min(distance, distance2);

                if (min <= radius || (y > radius && y < centerY)) {
                    texture.SetPixel(x, y, Color.cyan);
                }
            }
        }
    }
    
    private void DrawLine(Texture2D texture, Vector2 p1, Vector2 p2, Color color, int height = 100) {
        var x0 = (int)p1.x;
        var y0 = (int)p1.y;
        var x1 = (int)p2.x;
        var y1 = (int)p2.y;

        int dx = Mathf.Abs(x1 - x0); // horizontal change 
        int dy = Mathf.Abs(y1 - y0); // vertical change

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        Color drawColor = color;
        
        int err = dx - dy;
        int safety = 0;
        while (safety++ < height) {

            bool draw = y0 < texture.height && y0 > 0;
            
            drawColor = draw ? color : _inspectorGray;
            
            texture.SetPixel(x0, y0, drawColor);

            if (x0 == x1 && y0 == y1)
                break;
            
            int e2 = 2 * err;

            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
    }
}
