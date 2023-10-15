using System;
using UnityEngine;

public class FrameRateCounter : MonoBehaviour {
    public const int MaxFrameRateCount = 300;
    public const int WindowSize = 10;
    private readonly float[] dataPoints = new float[WindowSize];
    private readonly string[] numberStrings = new string[MaxFrameRateCount + 1];
    private string currentFrameRate = String.Empty;
    private float sum = 0.0f;
    private int currentIndex = 0;
    
    [SerializeField] private bool showFramerate;
    [SerializeField] private Rect screenRect;
    [SerializeField] private int targetFrameRate = 60;
    
    void Awake() {
        Application.targetFrameRate = targetFrameRate;
        
        for (int i = 0; i < numberStrings.Length; i++)
            numberStrings[i] = i.ToString();
    }

    void Update() => CalculateAverageFrameRate();

    private void CalculateAverageFrameRate() {
        int frameRate = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        AddDataPoint(frameRate);
        frameRate = Math.Clamp((int)GetAverage(), 0, MaxFrameRateCount);
        currentFrameRate = numberStrings[frameRate];
    }
    
    private void AddDataPoint(float value) {
        sum -= dataPoints[currentIndex];
        dataPoints[currentIndex] = value;
        sum += value;

        currentIndex = (currentIndex + 1) % WindowSize;
    }
    
    private float GetAverage() => sum / WindowSize;

    private void OnValidate() => Application.targetFrameRate = targetFrameRate;
    
#if UNITY_EDITOR

    void OnDrawGizmos() {
        if (!Application.isEditor || !showFramerate) return;
        
        UnityEditor.Handles.BeginGUI();

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label) {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            normal = {
                textColor = Color.black
            }
        };
        
        GUI.Label(screenRect, currentFrameRate, labelStyle);

        UnityEditor.Handles.EndGUI();
    }
    #endif
}
