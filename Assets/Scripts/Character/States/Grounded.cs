using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Grounded")]
public class Grounded : CharacterState {
    public override void Enter() {
        setVerticalVelocity?.Invoke(new Vector3(0f, -9.81f, 0f));
        Vector3 velocity = characterController.velocity;
        velocity.y = -1f;
        characterController.Move(velocity * Time.deltaTime);
        animator.SetBool(IsGrounded, true);
    }

    public override void Update() {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;
        animator.SetFloat(MoveZ, velocity.magnitude);
        animator.SetBool(Sliding, characterMovement.SlopeSlideVelocity.magnitude > 5f);

        if (characterController.velocity.sqrMagnitude <= float.Epsilon)
            characterMovement.TransitionTo<Idle>();

        if (input.Axis.sqrMagnitude > 0f 
            || Vector3.ProjectOnPlane(characterController.velocity, Vector3.up).sqrMagnitude > .1f
            || GroundSlope() >= characterController.slopeLimit)
            characterMovement.TransitionTo<Move>();

        if (!characterController.isGrounded)
            characterMovement.TransitionTo<Falling>();

        if (input.JumpPressed || Time.time <= input.JumpPressedLast + characterMovement.JumpBufferDuration)
            characterMovement.TransitionTo<Jump>();
    }

    private Ray ray;
    private RaycastHit slopeHitInfo;
    private float GroundSlope() {
        ray.origin = myTransform.position + characterController.radius * Vector3.up;
        ray.direction = Vector3.down;

        float maxDistance = (characterController.radius + characterController.skinWidth) / Mathf.Cos(characterController.slopeLimit * Mathf.Deg2Rad);

        if (Physics.Raycast(ray, out slopeHitInfo, maxDistance))
            return Vector3.Angle(Vector3.up, slopeHitInfo.normal);
        
        return 0f;
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() { }
    
    
}
