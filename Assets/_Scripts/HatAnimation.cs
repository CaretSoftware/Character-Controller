using UnityEngine;

public class HatAnimation : MonoBehaviour {
    [SerializeField] private ProceduralAnimation proceduralAnimation;

    [SerializeField] private Transform hat;
    [SerializeField] private Transform head;
    [SerializeField] private float magnitude;
    [SerializeField] private CharacterController characterController;
    private Vector3 origin;
    private Vector3 target;
    private Vector3 pos;
    
    private void Start() {
        origin = hat.localPosition;
    }

    private void LateUpdate() {
        target.y = -characterController.velocity.y * .05f;
        proceduralAnimation.Input = target;
        pos = proceduralAnimation.Output;
        pos.y = Mathf.Clamp(pos.y * magnitude, 0f, float.PositiveInfinity);
        hat.localPosition = origin + head.InverseTransformDirection(pos);
    }
}
