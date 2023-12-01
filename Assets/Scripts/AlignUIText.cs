using TMPro;
using UnityEngine;

public class AlignUIText : MonoBehaviour {
    [SerializeField] private Transform targetObject;
    [SerializeField] private RectTransform label;
    [SerializeField] private Camera cam;
    
    private void Update() {
        Vector3 targetPosition = cam.WorldToScreenPoint(targetObject.position);
        label.position = targetPosition;
    }
}
