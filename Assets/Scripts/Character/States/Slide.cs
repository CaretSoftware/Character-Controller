using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "States/Character/Slide")]
public class Slide : Grounded {
    [SerializeField, Range(0f, 20f)] private float slipSpeed = 5f;
    [SerializeField, Range(0f, 10f)] private float minSlideVelocity = 3f;
    [SerializeField, Range(0f, 1f)] private float inputSmoothTime = .1f;
    [SerializeField, Range(0f, 1f)] private float rotationSmoothTime = .1f;
    private Ray ray;
    
    // Slope Slide Velocity variables 
    private Vector3 slopeSlideVelocity;
    private RaycastHit sphereHitInfo;
    private RaycastHit rayHitInfo;
    private float slopeSlideMagnitude;
    private float stairAngle;
    private float angle;
    
    // Input Smoothing
    private Vector2 smoothInput;
    private float xCurrentVelocity;
    private float yCurrentVelocity;
    
    public override void Enter() {         
        animator.SetBool(Sliding, true);
        slopeSlideVelocity = Vector3.down * slipSpeed;
        xCurrentVelocity = 0f;
        yCurrentVelocity = 0f;
        smoothInput = Vector2.zero;

        // Calculate the vertical velocity preservation based on slope angle
        float slopeAngle = 90f - GroundSlope();
        Vector3 slopeNormal = new Vector3(Mathf.Cos(slopeAngle), 0f, 0f);
        Vector3 verticalVelocityOnSlope = movementStateMachine.VerticalVelocity;
        float dot = verticalVelocityOnSlope.y * Vector3.Dot(slopeNormal, Vector3.up);
        verticalVelocityOnSlope.y = dot;
        setVerticalVelocity?.Invoke(slopeSlideVelocity + verticalVelocityOnSlope);
    }

    private void OnValidate() {
        foreach (CharacterState characterState in instanceCopies) {
            Slide copy = Cast<Slide>(characterState);
            if (copy == null) continue;
            
            if (copy.slipSpeed != slipSpeed)
                copy.slipSpeed = slipSpeed;
            if (copy.minSlideVelocity != minSlideVelocity)
                copy.minSlideVelocity = minSlideVelocity;
            if (copy.inputSmoothTime != inputSmoothTime)
                copy.inputSmoothTime = inputSmoothTime;
            if (copy.rotationSmoothTime != rotationSmoothTime)
                copy.rotationSmoothTime = rotationSmoothTime;
        }
    }
    
    public override void Update() {
        rotateForward?.Invoke(rotationSmoothTime);
        SetSlopeSlideVelocity();
        Vector3 horizontalVelocity = SetHorizontalVelocity();
        AdjustVelocityToSlope(ref horizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontalVelocity);
        
        characterController.Move((movementStateMachine.HorizontalVelocity + movementStateMachine.VerticalVelocity + slopeSlideVelocity) * Time.deltaTime);

        if (GroundSlope() < characterController.slopeLimit && slopeSlideVelocity.magnitude < minSlideVelocity)
            movementStateMachine.TransitionTo<Grounded>();
            
        if (!characterController.isGrounded)
            movementStateMachine.TransitionTo<Falling>();
        
        if (input.JumpPressed || Time.time <= input.JumpPressedLast + movementStateMachine.JumpBufferDuration)
            movementStateMachine.TransitionTo<Jump>();
    }

    private void SetSlopeSlideVelocity() {
        this.slopeSlideVelocity -= this.slopeSlideVelocity * (Time.deltaTime * 3f);
        Vector3 slopeSlideVelocity = this.slopeSlideVelocity;
        slopeSlideVelocity.y = Mathf.Min(0f, this.slopeSlideVelocity.y);
        this.slopeSlideVelocity = slopeSlideVelocity;

        float radius = characterController.radius * myTransform.localScale.x;
        Vector3 position = myTransform.position;
        
        ray.origin = position + 1.01f * radius * Vector3.up;
        ray.direction = Vector3.down;

        // Spherecast Slope angle
        if (characterController.isGrounded && Physics.SphereCast(ray, radius, out sphereHitInfo, 2.1f * radius) ) {
            // Raycast Stair angle
            stairAngle = float.MaxValue;
            if (Physics.Raycast(ray, out rayHitInfo, 10f)) { //1.75f))
                stairAngle = Vector3.Angle(rayHitInfo.normal, Vector3.up);
            }
            
            // Take shallowest angle to not slide down stairs
            float slopeAngle = Vector3.Angle(sphereHitInfo.normal, Vector3.up);
            angle = Mathf.Min(slopeAngle, stairAngle);
            
            if (angle >= characterController.slopeLimit) {
                this.slopeSlideVelocity =
                        Vector3.ProjectOnPlane(
                                Vector3.up * -Mathf.Abs(movementStateMachine.VerticalVelocity.y - 3f), 
                                sphereHitInfo.normal);
                return;
            }
        }

        slopeSlideMagnitude = Vector3.ProjectOnPlane(slopeSlideVelocity, Vector3.up).magnitude;
        if (slopeSlideMagnitude == 0f)
            return;

        if (slopeSlideMagnitude > minSlideVelocity)
            return;
         
        this.slopeSlideVelocity = Vector3.zero;
    }

    private Vector3 SetHorizontalVelocity() {
        Vector3 horizontalVelocity;
        
        float currSmoothX = input.Axis.x != 0 ? 0f : inputSmoothTime;
        float currSmoothY = input.Axis.y != 0 ? 0f : inputSmoothTime;
        float angleFraction = Mathf.InverseLerp(90f, 0f, angle);
        angleFraction *= angleFraction;
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.Axis.x * angleFraction, ref xCurrentVelocity, currSmoothX);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.Axis.y * angleFraction, ref yCurrentVelocity, currSmoothY);
        horizontalVelocity.x = Mathf.Abs(smoothInput.x) > .1f ? smoothInput.x * movementStateMachine.MaxVelocity : 0f;
        horizontalVelocity.y = 0f;
        horizontalVelocity.z = Mathf.Abs(smoothInput.y) > .1f ? smoothInput.y * movementStateMachine.MaxVelocity : 0f;
        return Vector3.zero;// horizontalVelocity;
    }
    
    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() {
        animator.SetBool(Sliding, false);
    }
}
