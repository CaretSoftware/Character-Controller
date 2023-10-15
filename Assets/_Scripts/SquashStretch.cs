using UnityEngine;

public class SquashStretch : MonoBehaviour {
    [SerializeField] private Transform transformToScale;
    [SerializeField] private ProceduralAnimation proceduralAnimation;
    [SerializeField] private CharacterController characterController;
    
    private void Update() {
        Vector3 scale = transformToScale.localScale;
        Vector3 target = 
            new Vector3(1f, 
                Mathf.Clamp(1f + characterController.velocity.y * .1f, 0.75f, 1.25f), 
                1f);
        proceduralAnimation.Input = target;
        scale = proceduralAnimation.Output;
        transformToScale.localScale = scale;
    }
}