using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Grounded")]
public class Grounded : CharacterState {
    public override void Enter() {
        Vector3 horizontalVelocity = movementStateMachine.HorizontalVelocity;
        AdjustVelocityToSlope(ref horizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontalVelocity);
        if (GroundSlope() >= characterController.slopeLimit)
            setVerticalVelocity?.Invoke(Vector3.down * 9.81f);
        characterController.Move(
                (movementStateMachine.HorizontalVelocity + movementStateMachine.VerticalVelocity) 
                * Time.deltaTime);
        animator.SetBool(IsGrounded, true);
    }

    public override void Update() {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;
        animator.SetFloat(MoveZ, velocity.magnitude);
        
        LimitVerticalVelocity();
        
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
    }

    private void LimitVerticalVelocity() {
        Vector3 verticalVelocity = movementStateMachine.VerticalVelocity;
        verticalVelocity.y += Time.deltaTime * -9.81f;
        verticalVelocity.y = Mathf.Max(-Mathf.Abs(movementStateMachine.TerminalVelocity), verticalVelocity.y);
        setVerticalVelocity?.Invoke(verticalVelocity);
    }
    
    protected void AdjustVelocityToSlope(ref Vector3 velocity) {
        Vector3 horizontal = velocity;
        horizontal.y = 0f;
        float radius = characterController.radius;
        ray.origin = myTransform.position + radius * Vector3.up;
        ray.direction = Vector3.down;

        float maxDistance = (radius + characterController.skinWidth) / Mathf.Cos(characterController.slopeLimit * Mathf.Deg2Rad);
        
        if (Physics.Raycast(ray, out slopeHitInfo, maxDistance)){
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, slopeHitInfo.normal);
            
            Debug.DrawRay(myTransform.position, slopeRotation * Vector3.up * 5f);
            
            velocity = slopeRotation * horizontal;
        }
        Debug.DrawRay(myTransform.position + Vector3.up * characterController.radius, Vector3.down * maxDistance);
    }

    private Ray ray;
    private RaycastHit slopeHitInfo;
    protected float GroundSlope() {
        if (!characterController.isGrounded) 
            return 0f;
            
        float radius = characterController.radius;
        ray.origin = myTransform.position + radius * Vector3.up;
        ray.direction = Vector3.down;

        // TODO raycast doesn't reach if too short
        float maxDistance = (radius + characterController.skinWidth + characterController.stepOffset) / Mathf.Cos(characterController.slopeLimit * Mathf.Deg2Rad);
        float angle = 180f;
        
        if (Physics.Raycast(ray, out slopeHitInfo, maxDistance))
            angle = Vector3.Angle(Vector3.up, slopeHitInfo.normal);

        return angle;
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() { }
    
    
}
