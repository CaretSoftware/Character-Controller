using UnityEngine;

public class HatAnimation : MonoBehaviour {
    [SerializeField] private ProceduralAnimation proceduralAnimation;

    [SerializeField] private Transform hat;
    [SerializeField] private float magnitude;
    [SerializeField] private CharacterController characterController;
    private Vector3 origin;
    private Vector3 pos;
    
    private void Start() {
        origin = hat.localPosition;
        proceduralAnimation.Input = origin;
    }

    private void Update() {
        Vector3 target = 
                new Vector3(
                    characterController.velocity.y * .05f, 
                    0f,
                    0f);
        proceduralAnimation.Input = target;
        pos = proceduralAnimation.Output;
        pos.x = Mathf.Clamp(pos.x * magnitude, float.NegativeInfinity, 0f);
        hat.localPosition = origin + pos;
    }
}
