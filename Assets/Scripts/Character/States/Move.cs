using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Move")]
public class Move : Grounded {

    [SerializeField] private float characterMaxSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float groundSmoothTime = .1f;
    [SerializeField, Range(0f, 1f)] private float rotationSmoothTime = .1f;
    
    public float CharacterMaxSpeed => characterMaxSpeed;
    private Ray ray;
    private Vector2 smoothInput;
    private float xCurrentVelocity;
    private float yCurrentVelocity;

    public override void Enter() {
        xCurrentVelocity = 0f;
        yCurrentVelocity = 0f;
        smoothInput = Vector2.zero;
    }

    private void OnValidate() {
        foreach (CharacterState characterState in instanceCopies) {
            Move copy = Cast<Move>(characterState);
            if (copy == null) continue;
            
            if (copy.characterMaxSpeed != characterMaxSpeed) {
                copy.characterMaxSpeed = characterMaxSpeed;
                copy.setMaxSpeed?.Invoke(characterMaxSpeed);
            }
            if (copy.groundSmoothTime != groundSmoothTime)
                copy.groundSmoothTime = groundSmoothTime;
            if (copy.rotationSmoothTime != rotationSmoothTime)
                copy.rotationSmoothTime = rotationSmoothTime;
        }
    }

    public override void Update() {
        rotateForward?.Invoke(rotationSmoothTime);
        Vector3 horizontalVelocity = SetHorizontalVelocity(movementStateMachine.HorizontalVelocity);
        AdjustVelocityToSlope(ref horizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontalVelocity);
        characterController.Move((horizontalVelocity + movementStateMachine.VerticalVelocity) * Time.deltaTime);

        base.Update();
    }

    private Vector3 SetHorizontalVelocity(Vector3 horizontalVelocity) {
        float currSmoothX = input.Axis.x != 0 ? 0f : groundSmoothTime;
        float currSmoothY = input.Axis.y != 0 ? 0f : groundSmoothTime;
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.Axis.x, ref xCurrentVelocity, currSmoothX);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.Axis.y, ref yCurrentVelocity, currSmoothY);
        horizontalVelocity.x = Mathf.Abs(smoothInput.x) > .1f ? smoothInput.x * characterMaxSpeed : 0f;
        horizontalVelocity.z = Mathf.Abs(smoothInput.y) > .1f ? smoothInput.y * characterMaxSpeed : 0f;
        return horizontalVelocity;
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() => animator.SetBool(Sliding, false);
}
