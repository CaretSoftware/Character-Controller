using System;
using UnityEngine;
using UnityEngine.Rendering;

public class FPSLimiter : MonoBehaviour {
    public readonly int[] frameRates = { 15, 30, 60, 144 };
    private const int MaxFrameRateCount = 300;
    private const int WindowSize = 10;
    private readonly float[] dataPoints = new float[WindowSize];
    private readonly string[] numberStrings = new string[MaxFrameRateCount + 1];
    private string currentFrameRate = String.Empty;
    private float sum = 0.0f;
    private int currentIndex = 0;
    private readonly Rect screenRect = Rect.MinMaxRect(20, 0, 70, 30);
    
    [SerializeField] public bool showFramerate = true;
    private int targetFrameRate = 144;
    public int TargetFrameRate {
        get => targetFrameRate;
        set => Application.targetFrameRate = targetFrameRate = value;
    }

    void Awake() {
        Application.targetFrameRate = TargetFrameRate;
        
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

    private void OnValidate() => Application.targetFrameRate = TargetFrameRate;
    
#if UNITY_EDITOR

    private Color contrastColor = Color.black;
    private Color lastColor = Color.black;
    private float updateColorTimer;
    void OnDrawGizmos() {
        if (!Application.isEditor || !showFramerate) return;

        updateColorTimer += Time.deltaTime;
        if (updateColorTimer >= 1f) {
            shouldScreenGrab = true;
            updateColorTimer = 0f;
            lastColor = contrastColor;
        }

        Color interpolatedColor = Color.Lerp(lastColor, contrastColor, updateColorTimer);
        UnityEditor.Handles.BeginGUI();
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label) {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            normal = {
                textColor = interpolatedColor
            }
        };
        
        GUI.Label(screenRect, currentFrameRate, labelStyle);

        UnityEditor.Handles.EndGUI();
    }

    private void Start() {
        RenderPipelineManager.endCameraRendering += ReadGameViewPixelColor;
        // Create a new Texture2D with the width and height of the screen, and cache it for reuse
        destinationTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
    }

    void OnDestroy() =>  RenderPipelineManager.endCameraRendering -= ReadGameViewPixelColor;

    // Set Renderer to a GameObject that has a Renderer component and a material that displays a texture
    private Texture2D destinationTexture;
    private bool shouldScreenGrab;
    void ReadGameViewPixelColor(ScriptableRenderContext context, Camera cam) {
        if (!shouldScreenGrab || cam != Camera.main) return;
        shouldScreenGrab = false;
        
        // screen-grab 1x1 pixel top left corner
        Rect regionToReadFrom = new Rect(screenRect.x, Screen.height - 30, 1, 1);

        destinationTexture.ReadPixels(regionToReadFrom, 0, 0, false);

        Color backgroundColor = GetColorAtPixel(destinationTexture, 1, 1);
        float avg = 1f - (backgroundColor.r + backgroundColor.g + backgroundColor.b) / 3f;
        avg = Mathf.RoundToInt(avg);
        contrastColor = new Color(avg, avg, avg, 1f);
    }
    
    private Color GetColorAtPixel(Texture2D texture, int x, int y)
    {
        return texture.GetPixel(x, y);
    }
#endif
}
