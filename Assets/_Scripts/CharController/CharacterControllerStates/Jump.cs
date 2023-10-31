using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Jump")]
public class Jump : CharacterState {
    [SerializeField, Range(0f, 1f)] private float jumpBufferDuration = .2f;
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 4;
    [SerializeField, Range(0f, 1f)] private float airSmoothTime = .5f;
    [SerializeField, Range(0f, 1f)] private float airControlSmoothTime = .25f;
    [SerializeField, Range(-30, 0f)] private float gravity = -9.81f;
    [SerializeField, Range(1f, 20f)] private float fallGravityMultiplier = 10f;
    [SerializeField, Range(-2f, 0f)] private float minApexVelocityThreshold = -.62f;
    [SerializeField, Range(0f, 2f)] private float maxApexVelocityThreshold = .2f;
    [SerializeField, Range(0f, 1f)] private float rotationSmoothTime = .25f;
    
    private Vector3 verticalVelocity;
    private float gravityMultiplier = 1f;
    private bool jumpReleased;
    
    public float JumpBufferDuration => jumpBufferDuration;

    private void OnValidate() {
        graphArray = null; // forces redrawing of graph
        if (characterSm != null && characterSm.JumpBufferDuration != jumpBufferDuration)
            setJumpBufferDuration?.Invoke(jumpBufferDuration);
        
        foreach (CharacterState characterStateCopy in instanceCopies) {
            Jump copy = Cast<Jump>(characterStateCopy);
            if (copy == null) continue;
            if (copy.jumpBufferDuration != jumpBufferDuration)
                copy.jumpBufferDuration = jumpBufferDuration;
            if (copy.jumpHeight != jumpHeight)
                copy.jumpHeight = jumpHeight;
            if (copy.airSmoothTime != airSmoothTime)
                copy.airSmoothTime = airSmoothTime;
            if (copy.airControlSmoothTime != airControlSmoothTime)
                copy.airControlSmoothTime = airControlSmoothTime;
            if (copy.gravity != gravity)
                copy.gravity = gravity;
            if (copy.fallGravityMultiplier != fallGravityMultiplier)
                copy.fallGravityMultiplier = fallGravityMultiplier;
            if (copy.minApexVelocityThreshold != minApexVelocityThreshold)
                copy.minApexVelocityThreshold = minApexVelocityThreshold;
            if (copy.maxApexVelocityThreshold != maxApexVelocityThreshold)
                copy.maxApexVelocityThreshold = maxApexVelocityThreshold;
        }
    }

    public override void Enter() {
        animator.SetTrigger(IsJumping);
        animator.SetBool(IsGrounded, false);

        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        smoothInput.x = (characterSm.HorizontalVelocity.x / characterSm.MaxVelocity) * Mathf.Abs(input.Axis.x);
        smoothInput.y = (characterSm.HorizontalVelocity.z / characterSm.MaxVelocity) * Mathf.Abs(input.Axis.y);
        xCurrentVelocity = smoothInput.x;
        yCurrentVelocity = smoothInput.y;
        jumpReleased = false;
    }

    public override void Update() {
        if (input.JumpReleased)
            jumpReleased = true;
        
        gravityMultiplier = SetJumpApexGravityMultiplier(jumpReleased, input.JumpHold, verticalVelocity);
        verticalVelocity = GetVerticalVelocity(verticalVelocity, gravityMultiplier, Time.deltaTime);
        setVerticalVelocity?.Invoke(verticalVelocity);
        setHorizontalVelocity?.Invoke(GetHorizontalVelocity(ref smoothInput, input.Axis, ref xCurrentVelocity, ref yCurrentVelocity, Time.deltaTime));
        rotateForward?.Invoke(rotationSmoothTime);
        characterController.Move(Time.deltaTime * (characterSm.VerticalVelocity + characterSm.HorizontalVelocity));
        
        if (verticalVelocity.y < 0f && characterController.isGrounded)
            characterSm.TransitionTo<Grounded>();
    }

    private Vector3 GetVerticalVelocity(Vector3 verticalVelocity, float gravityMultiplier, float deltaTime) {
        verticalVelocity.y += gravity * gravityMultiplier * deltaTime;
        float terminalVelocity = characterSm != null ? characterSm.TerminalVelocity : -20f;
        verticalVelocity.y = Mathf.Max(-Mathf.Abs(terminalVelocity), verticalVelocity.y);
        return verticalVelocity;
    }

    private float SetJumpApexGravityMultiplier(bool jumpReleased, bool jumpHold, Vector3 verticalVelocity) {
        float gravityMultiplier = !jumpReleased && (jumpHold && verticalVelocity.y > minApexVelocityThreshold) 
            ? 1f : fallGravityMultiplier;
        gravityMultiplier = !jumpReleased && jumpHold && 
                       (verticalVelocity.y > minApexVelocityThreshold && verticalVelocity.y < maxApexVelocityThreshold) 
            ? .25f : gravityMultiplier; // TODO .25f Don't hardcode!
        return gravityMultiplier;
    }

    private float xCurrentVelocity;
    private float yCurrentVelocity;
    private Vector2 smoothInput;
    
    private Vector3 GetHorizontalVelocity(ref Vector2 smoothInput, Vector2 input, ref float xCurrentVelocity, ref float yCurrentVelocity, float deltaTime) {
        float currSmoothX = input.x == 0 ? airSmoothTime : airControlSmoothTime;
        float currSmoothY = input.y == 0 ? airSmoothTime : airControlSmoothTime;
        float terminalVelocity = characterSm != null ? characterSm.TerminalVelocity : -50f;
        float maxVelocity = characterSm != null ? characterSm.MaxVelocity : 5f;
        
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.x, ref xCurrentVelocity, currSmoothX, -terminalVelocity, deltaTime);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.y, ref yCurrentVelocity, currSmoothY, -terminalVelocity, deltaTime);
        Vector3 horizontalVelocity;
        horizontalVelocity.x = smoothInput.x * (Mathf.Abs(smoothInput.x) > .1f ? maxVelocity : 0f);
        horizontalVelocity.y = 0f;
        horizontalVelocity.z = smoothInput.y * (Mathf.Abs(smoothInput.y) > .1f ? maxVelocity : 0f);
        return horizontalVelocity;
    }
    
    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() { }

    private const int GraphResolution = 100;
    [SerializeField, HideInInspector] private Vector2[] graphArray;
    [SerializeField, HideInInspector] private float one;
    public Vector2[] JumpGraph(out float zero, out float one) {
        zero = 0f;
        if (graphArray != null) {
            one = this.one;
            return graphArray;
        }
        
        ResetSimulation();
        Vector3 pos = Vector3.zero;
        float height = -1f;
        float length = -1f;

        float timeStep = 1f / 30f;
        float time = 0f;
        int safety = 0;
        do {
            pos += SimulateTimeStep(timeStep) * timeStep;
            height = Mathf.Max(height, pos.y);
            length = Mathf.Max(length, pos.x);
            time += timeStep;
        } while (pos.y > -2f && ++safety < 2000);

        height += .2f;
        length += 2f;
        ResetSimulation();
        pos = Vector3.zero;
        timeStep = time / GraphResolution;

        Vector2[] points = new Vector2[100];
        points[0] = Vector2.zero;
        for (int i = 1; i < GraphResolution; i++) {
            pos += SimulateTimeStep(timeStep) * timeStep;
            float x = Mathf.InverseLerp(0f, length, pos.x);
            float y = Mathf.InverseLerp(0f, height, pos.y);
            points[i] = new Vector2(Mathf.Lerp(.01f, .99f, x), Mathf.LerpUnclamped(.01f, .99f, y));
        }
        one = Mathf.InverseLerp(0f, height, 1f);
        return points;
    }

    private float simGravityMultiplier = 1;
    private Vector3 simVerticalVelocity;
    private Vector3 simHorizontalVelocity;
    private float simXCurrentVelocity;
    private float simYCurrentVelocity;
    private Vector2 simSmoothInput;
    
    private Vector3 SimulateTimeStep(float timeStep) {
        simGravityMultiplier = SetJumpApexGravityMultiplier(false, true, simVerticalVelocity);
        simVerticalVelocity = GetVerticalVelocity(simVerticalVelocity, simGravityMultiplier, timeStep);
        simHorizontalVelocity = GetHorizontalVelocity(ref simSmoothInput, Vector2.right, ref simXCurrentVelocity, ref simYCurrentVelocity, timeStep);
        return (simVerticalVelocity + simHorizontalVelocity);
    }
    
    private void ResetSimulation() {
        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        simSmoothInput.x = 1f;
        xCurrentVelocity = simSmoothInput.x;

        simVerticalVelocity = Vector3.zero;
        simHorizontalVelocity = Vector3.right;
        simVerticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        simXCurrentVelocity = 0f;
    }
}
