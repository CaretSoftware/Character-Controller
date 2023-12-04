using UnityEngine;
using TMPro;

public class UIText : MonoBehaviour {
    public delegate void UITextDelegate(string text);
    public static UITextDelegate updateText;
    private TextMeshProUGUI uiText;
    
    private void Awake() {
        uiText = GetComponent<TextMeshProUGUI>();
        updateText += UpdateText;
    }
    
    private void UpdateText(string text) => uiText.text = text;
    
    private void OnDestroy() => updateText -= UpdateText;
}
