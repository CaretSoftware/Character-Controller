using System;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Character/Jump")]
public class Jump : CharacterState {
    public float JumpBufferDuration => jumpBufferDuration;
    [SerializeField, Range(0f, 1f)] private float jumpBufferDuration = .2f;
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 4;
    [SerializeField, Range(0f, 1f)] private float airSmoothTime = .5f;
    [SerializeField, Range(0f, 1f)] private float airControlSmoothTime = .25f;
    [SerializeField, Range(-30, 0f)] private float gravity = -9.81f;
    [SerializeField, Range(1f, 20f)] private float fallGravityMultiplier = 10f;
    [SerializeField, Range(-2f, 0f)] private float minApexVelocityThreshold = -.62f;
    [SerializeField, Range(0f, 2f)] private float maxApexVelocityThreshold = .2f;
    private Vector3 verticalVelocity;
    private float gravityMultiplier = 1f;
    private bool jumpReleased;

    private void OnValidate() {
        if (characterSm != null && characterSm.JumpBufferDuration != jumpBufferDuration)
            setJumpBufferDuration?.Invoke(jumpBufferDuration);
    }

    public override void Enter() {
        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        smoothInput.x = (characterSm.HorizontalVelocity.x / characterSm.MaxVelocity) * Mathf.Abs(input.Axis.x);
        smoothInput.y = (characterSm.HorizontalVelocity.z / characterSm.MaxVelocity) * Mathf.Abs(input.Axis.y);
        xCurrentVelocity = smoothInput.x;
        yCurrentVelocity = smoothInput.y;
        jumpReleased = false;
    }

    public override void Update() {
        SetJumpApexGravityMultiplier();
        setVerticalVelocity?.Invoke(GetVerticalVelocity());
        setHorizontalVelocity?.Invoke(GetHorizontalVelocity());
        characterController.Move(Time.deltaTime * (characterSm.VerticalVelocity + characterSm.HorizontalVelocity));
        
        if (verticalVelocity.y < 0f && characterController.isGrounded)
            characterSm.TransitionTo<Grounded>();
    }

    private Vector3 GetVerticalVelocity() {
        if (input.JumpReleased)
            jumpReleased = true;
        //if (_input.JumpPressed)
        //    jumpBufferTime = Time.time + jumpBuffering;
        //if (_characterController.isGrounded && characterSm.VerticalVelocity.y < 0f)// && slopeSlideVelocity.magnitude == 0f)
        //    verticalVelocity = 0f;
            //_setVerticalVelocity?.Invoke(Vector3.zero);
            
        //SetJumpApexGravityScale();

        verticalVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
        verticalVelocity.y = Mathf.Max(-Mathf.Abs(characterSm.TerminalVelocity), verticalVelocity.y);
        return verticalVelocity;
        /*
        if (JumpBuffer() || (_input.JumpPressed && CoyoteTime())) {
            //platform = null;
            float verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _setVerticalVelocity?.Invoke(Vector3.up * verticalVelocity);
            jumpReleased = false;
        }
        */

        //bool JumpBuffer() { return Time.time <= jumpBufferTime; }
        //bool CoyoteTime() { return Time.time <= coyoteTimeLast && characterVelocity.y < 0f; }
    }

    
    private void SetJumpApexGravityMultiplier() {
        gravityMultiplier = !jumpReleased && (input.JumpHold && verticalVelocity.y > minApexVelocityThreshold) 
            ? 1f : fallGravityMultiplier;
        gravityMultiplier = !jumpReleased && input.JumpHold && 
                       (verticalVelocity.y > minApexVelocityThreshold && verticalVelocity.y < maxApexVelocityThreshold) 
            ? .25f : gravityMultiplier; // TODO .25f Don't hardcode!
    }
    
    
    private float xCurrentVelocity;
    private float yCurrentVelocity;
    private Vector2 smoothInput;
    private Vector3 GetHorizontalVelocity() {
        float currSmoothX = input.Axis.x == 0 ? airSmoothTime : airControlSmoothTime;
        float currSmoothY = input.Axis.y == 0 ? airSmoothTime : airControlSmoothTime;
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.Axis.x, ref xCurrentVelocity, currSmoothX);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.Axis.y, ref yCurrentVelocity, currSmoothY);
        Vector3 horizontalVelocity;
        horizontalVelocity.x = Mathf.Abs(smoothInput.x) > .1f ? smoothInput.x * characterSm.MaxVelocity : 0f;
        horizontalVelocity.y = 0f;
        horizontalVelocity.z = Mathf.Abs(smoothInput.y) > .1f ? smoothInput.y * characterSm.MaxVelocity : 0f;
        return horizontalVelocity;
    }
    
    public override void LateUpdate() {
    }

    public override void FixedUpdate() {
    }

    public override void Exit() {
    }
}
