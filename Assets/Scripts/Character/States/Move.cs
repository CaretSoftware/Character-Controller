using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
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

    public override void Enter() {
        //animator.SetBool(IsMoving, true);
    }

    private void OnValidate() {
        if (characterMovement != null && characterMovement.MaxVelocity != characterMaxSpeed)
            setMaxSpeed?.Invoke(characterMaxSpeed);
        
        foreach (CharacterState characterState in instanceCopies) {
            if (characterState == null) continue;
            Move copy = Cast<Move>(characterState);
            if (copy.characterMaxSpeed != characterMaxSpeed)
                copy.characterMaxSpeed = characterMaxSpeed;
            if (copy.groundSmoothTime != groundSmoothTime)
                copy.groundSmoothTime = groundSmoothTime;
            if (copy.rotationSmoothTime != rotationSmoothTime)
                copy.rotationSmoothTime = rotationSmoothTime;
        }
    }

    public override void Update() {
        rotateForward?.Invoke(rotationSmoothTime);
        Vector3 horizontalVelocity = SetHorizontalVelocity(characterMovement.HorizontalVelocity);
        AdjustVelocityToSlope(ref horizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontalVelocity);
        characterController.Move((horizontalVelocity + characterMovement.VerticalVelocity + characterMovement.SlopeSlideVelocity) * Time.deltaTime);
        base.Update();
    }

    private float xCurrentVelocity;
    private float yCurrentVelocity;
    private Vector2 smoothInput;
    private Vector3 SetHorizontalVelocity(Vector3 horizontalVelocity) {
        float currSmoothX = input.Axis.x != 0 ? 0f : groundSmoothTime;
        float currSmoothY = input.Axis.y != 0 ? 0f : groundSmoothTime;
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.Axis.x, ref xCurrentVelocity, currSmoothX);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.Axis.y, ref yCurrentVelocity, currSmoothY);
        horizontalVelocity.x = Mathf.Abs(smoothInput.x) > .1f ? smoothInput.x * characterMaxSpeed : 0f;
        horizontalVelocity.z = Mathf.Abs(smoothInput.y) > .1f ? smoothInput.y * characterMaxSpeed : 0f;
        return horizontalVelocity;
    }
    
    private Vector3 horizontal;
    private RaycastHit slopeHitInfo;
    private Quaternion slopeRotation;
    private void AdjustVelocityToSlope(ref Vector3 velocity) {
        horizontal = velocity;
        horizontal.y = 0f;
        ray.origin = myTransform.position + characterController.radius * Vector3.up;
        ray.direction = Vector3.down;

        float maxDistance = (characterController.radius + characterController.skinWidth) / Mathf.Cos(characterController.slopeLimit * Mathf.Deg2Rad);
        
        if (Physics.Raycast(ray, out slopeHitInfo, maxDistance)){
            slopeRotation = Quaternion.FromToRotation(Vector3.up, slopeHitInfo.normal);
            velocity = slopeRotation * horizontal;
        }
        Debug.DrawRay(myTransform.position + Vector3.up * characterController.radius, Vector3.down * maxDistance);
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() {
        animator.SetBool(Sliding, false);
    }
}
