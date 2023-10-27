using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralAnimation))]
public class SecondOrderEditor : Editor {
    private static Color _inspectorGray = new Color(0.22352941176f, 0.22352941176f, 0.22352941176f);
    private static Color lineColorZero = new Color(.15f, .15f, .15f);
    private static Color lineColorOne = new Color(.3f, .3f, .3f);
    private Texture2D _previousTexture2D;

    private void OnEnable() {
        _previousTexture2D = new Texture2D(1, 1);
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        ProceduralAnimation component = target as ProceduralAnimation;
        Vector2[] dataPoints = component.GetGraph(out float zero, out float one);

        DrawEditorGraph.DrawGraph(dataPoints, zero, one, DrawEditorGraph._inspectorGray, 
            Color.white, DrawEditorGraph.lineColor0, DrawEditorGraph.lineColor1);

        serializedObject.ApplyModifiedProperties();
    }
}
