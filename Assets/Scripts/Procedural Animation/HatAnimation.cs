using UnityEngine;

public class HatAnimation : MonoBehaviour {
    [SerializeField] private ProceduralAnimation proceduralAnimation;

    [SerializeField] private Transform hat;
    [SerializeField] private Transform head;
    [SerializeField] private float magnitude;
    [SerializeField] private CharacterController characterController;
    [SerializeField, Range(0f, 45f)] private float hatRotationAngle;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private Vector3 origin;
    private Vector3 target;
    private Vector3 pos;
    
    private void Start() {
        startRotation = hat.localRotation;
        endRotation = startRotation * Quaternion.Euler(-hatRotationAngle, 0f, 0f);
        origin = hat.localPosition;
    }

    [Range(-10f, 10f)] public float speed = .05f;
    private void LateUpdate() {
        target.y = Mathf.InverseLerp(0f, -1f, characterController.velocity.y * speed);
        proceduralAnimation.Input =  target;
        pos = proceduralAnimation.Output;
        pos.y = Mathf.Clamp(pos.y * magnitude, 0f, float.PositiveInfinity);
        hat.localPosition = origin + head.InverseTransformDirection(pos);

        float t = Mathf.InverseLerp(0f, 1f, pos.y);
        
        hat.localRotation = Quaternion.Lerp(startRotation, endRotation, t);
    }
}
