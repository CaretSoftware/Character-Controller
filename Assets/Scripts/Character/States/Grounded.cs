using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Grounded")]
public class Grounded : CharacterState {
    [SerializeField, Range(0f, -30f)] private float groundedGravity = -9.81f;

    public override void Enter() {
        Vector3 horizontalVelocity = movementStateMachine.HorizontalVelocity;
        AdjustVelocityToSlope(ref horizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontalVelocity);
        setVerticalVelocity?.Invoke(Vector3.up * groundedGravity);
        characterController.Move(
            (movementStateMachine.HorizontalVelocity + movementStateMachine.VerticalVelocity)
            * Time.deltaTime);
        animator.SetBool(IsGrounded, true);
    }

    public override void Update() {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;
        animator.SetFloat(MoveZ, velocity.magnitude);
        
        AddVerticalVelocity();
        
        if (characterController.velocity.sqrMagnitude <= float.Epsilon)
            movementStateMachine.TransitionTo<Idle>();

        if (input.Axis.sqrMagnitude > 0f 
               || Vector3.ProjectOnPlane(characterController.velocity, Vector3.up).sqrMagnitude > .1f)
            movementStateMachine.TransitionTo<Move>();

        if (GroundSlope() >= characterController.slopeLimit)
            movementStateMachine.TransitionTo<Slide>();

        if (!characterController.isGrounded)
            movementStateMachine.TransitionTo<Falling>();

        if (input.JumpPressed || Time.time <= input.JumpPressedLast + movementStateMachine.JumpBufferDuration)
            movementStateMachine.TransitionTo<Jump>();
        
        if (!movementStateMachine.CharacterActive)
            movementStateMachine.TransitionTo<Inactive>();
    }

    private void AddVerticalVelocity() {
        Vector3 verticalVelocity = movementStateMachine.VerticalVelocity;
        verticalVelocity.y += Time.deltaTime * groundedGravity;
        verticalVelocity.y = Mathf.Max(-Mathf.Abs(movementStateMachine.TerminalVelocity), verticalVelocity.y);
        setVerticalVelocity?.Invoke(verticalVelocity);
    }
    
    protected void AdjustVelocityToSlope(ref Vector3 velocity) {
        Vector3 horizontal = velocity;
        horizontal.y = 0f;
        float radius = characterController.radius;
        ray.origin = myTransform.position + radius * Vector3.up;
        ray.direction = Vector3.down;

        float maxDistance = (radius + characterController.skinWidth) / 
                            Mathf.Cos(characterController.slopeLimit * Mathf.Deg2Rad);
        
        if (Physics.Raycast(ray, out slopeHitInfo, maxDistance)){
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, slopeHitInfo.normal);
            velocity = slopeRotation * horizontal;
        }
    }

    private Ray ray;
    private RaycastHit slopeHitInfo;
    protected float GroundSlope() {
        if (!characterController.isGrounded) 
            return 0f;
        
        float radius = characterController.radius * myTransform.localScale.x;
        ray.origin = myTransform.position + radius * Vector3.up;
        ray.direction = Vector3.down;

        float maxDistance = Mathf.Infinity;// (radius + characterController.skinWidth + characterController.stepOffset) / Mathf.Cos(characterController.slopeLimit * Mathf.Deg2Rad);
        float stairAngle = 180f;
        float slopeAngle = 0f;
        
        if (Physics.Raycast(ray, out slopeHitInfo, maxDistance))
            stairAngle = Vector3.Angle(Vector3.up, slopeHitInfo.normal);

        if (Physics.SphereCast(ray, radius, out RaycastHit sphereHitInfo, 1f))
            slopeAngle = Vector3.Angle(Vector3.up, sphereHitInfo.normal);
        
        return Mathf.Min(stairAngle, slopeAngle);
    }

    public override void Exit() { }
}
