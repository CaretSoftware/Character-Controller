using UnityEditor.ShaderGraph;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Character/Slide")]
public class Slide : Grounded {
    [SerializeField, Range(0f, 1f)] private float groundSmoothTime = .1f;
    [SerializeField, Range(0f, 1f)] private float rotationSmoothTime = .1f;
    [SerializeField, Range(0f, 10f)] private float slipSpeed = 5f;
    private Ray ray;

    public override void Enter() {         
        animator.SetBool(Sliding, true);
        slopeSlideVelocity = Vector3.down * slipSpeed;
        xCurrentVelocity = 0f;
        yCurrentVelocity = 0f;
        smoothInput = Vector2.zero;
        setVerticalVelocity?.Invoke(slopeSlideVelocity);
    }

    private void OnValidate() {
        foreach (CharacterState characterState in instanceCopies) {
            Slide copy = Cast<Slide>(characterState);
            if (copy == null) continue;
            
            if (copy.groundSmoothTime != groundSmoothTime)
                copy.groundSmoothTime = groundSmoothTime;
            if (copy.rotationSmoothTime != rotationSmoothTime)
                copy.rotationSmoothTime = rotationSmoothTime;
            if (copy.slipSpeed != slipSpeed)
                copy.slipSpeed = slipSpeed;
        }
    }
    
    public override void Update() {
        rotateForward?.Invoke(rotationSmoothTime);
        SetSlopeSlideVelocity();
        Vector3 horizontalVelocity = SetHorizontalVelocity();
        AdjustVelocityToSlope(ref horizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontalVelocity);
        
        // TODO limit velocity to terminal velocity
        
        characterController.Move((movementStateMachine.HorizontalVelocity + movementStateMachine.VerticalVelocity + slopeSlideVelocity) * Time.deltaTime);

        if (GroundSlope() < characterController.slopeLimit && slopeSlideVelocity.magnitude < 3f)  // TODO Variable
            movementStateMachine.TransitionTo<Grounded>();
            
        if (!characterController.isGrounded)
            movementStateMachine.TransitionTo<Falling>();
        
        if (input.JumpPressed || Time.time <= input.JumpPressedLast + movementStateMachine.JumpBufferDuration)
            movementStateMachine.TransitionTo<Jump>();
        
        // if Axis and not on slope
            // transition to Move
        //base.Update();
    }

    private Vector3 slopeSlideVelocity;
    private RaycastHit sphereHitInfo;
    private RaycastHit rayHitInfo;
    private float slopeSlideMagnitude;
    private float stairAngle;
    private float angle;
    private void SetSlopeSlideVelocity() {

        this.slopeSlideVelocity -= this.slopeSlideVelocity * (Time.deltaTime * 3f);   // TODO Smooth damp
        Vector3 slopeSlideVelocity = this.slopeSlideVelocity;
        slopeSlideVelocity.y = Mathf.Min(0f, this.slopeSlideVelocity.y);
        this.slopeSlideVelocity = slopeSlideVelocity;

        float radius = characterController.radius;
        Vector3 position = myTransform.position;
        
        ray.origin = position + 1.01f * radius * Vector3.up;
        ray.direction = Vector3.down;
        
        // Spherecast Slope angle
        if (Physics.SphereCast(ray, radius, out sphereHitInfo, 2.1f * radius) ) {
            Debug.Log("Sphere");
            // Raycast Stair angle
            stairAngle = float.MaxValue;
            if (Physics.Raycast(ray, out rayHitInfo, 10f)) { //1.75f))
                Debug.Log("Stair");
                stairAngle = Vector3.Angle(rayHitInfo.normal, Vector3.up);
            }
            
            // Take shallowest angle to not slide down stairs
            float slopeAngle = Vector3.Angle(sphereHitInfo.normal, Vector3.up);
            angle = Mathf.Min(slopeAngle, stairAngle);
            
            if (angle >= characterController.slopeLimit) {
                this.slopeSlideVelocity =    // TODO
                    Vector3.ProjectOnPlane(new Vector3(0f, -Mathf.Abs(movementStateMachine.VerticalVelocity.y - 3f), 0f), sphereHitInfo.normal);//-Mathf.Abs(VerticalVelocity.y), 0f), sphereHitInfo.normal);
                return;
            }
        }

        slopeSlideMagnitude = Vector3.ProjectOnPlane(movementStateMachine.SlopeSlideVelocity, Vector3.up).magnitude;
        if (slopeSlideMagnitude == 0f)
            return;

        if (slopeSlideMagnitude > 3f)                                       // TODO parameter
            return;
         
        this.slopeSlideVelocity = Vector3.zero;
    }

    private float xCurrentVelocity;
    private float yCurrentVelocity;
    private Vector2 smoothInput;
    private Vector3 SetHorizontalVelocity() {
        Vector3 horizontalVelocity;
        
        float currSmoothX = input.Axis.x != 0 ? 0f : groundSmoothTime;
        float currSmoothY = input.Axis.y != 0 ? 0f : groundSmoothTime;
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.Axis.x, ref xCurrentVelocity, currSmoothX);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.Axis.y, ref yCurrentVelocity, currSmoothY);
        horizontalVelocity.x = Mathf.Abs(smoothInput.x) > .1f ? smoothInput.x * movementStateMachine.MaxVelocity : 0f;
        horizontalVelocity.y = 0f;
        horizontalVelocity.z = Mathf.Abs(smoothInput.y) > .1f ? smoothInput.y * movementStateMachine.MaxVelocity : 0f;
        return horizontalVelocity;
    }
    
    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() {
        animator.SetBool(Sliding, false);
        //Vector3 endVelocity = SlopeSlideVelocity;
        //endVelocity.y = 0f;
        //setHorizontalVelocity?.Invoke(endVelocity);
    }
}
