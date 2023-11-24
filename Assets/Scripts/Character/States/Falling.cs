using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Character/Falling")]
public class Falling : CharacterState {
    [SerializeField] private float coyoteTime = .2f;
    [SerializeField] private float terminalVelocity = -50f;
    [SerializeField] private float fallingAcceleration = 19.62f;
    [field: SerializeField] public float TerminalVelocity { get; private set; }

    private float timeFalling;
    
    private void OnValidate() {
        
        foreach (CharacterState characterState in instanceCopies) {
            Falling copy = Cast<Falling>(characterState);
            if (copy == null) continue;
            
            if (copy.terminalVelocity != terminalVelocity) {
                copy.terminalVelocity = terminalVelocity;
                copy.setTerminalVelocity?.Invoke(terminalVelocity);
            }
            if (copy.coyoteTime != coyoteTime)
                copy.coyoteTime = coyoteTime;
        }
    }
    
    public override void Enter() {
        animator.SetBool(IsGrounded, false);
        timeFalling = 0f;
        setVerticalVelocity?.Invoke(Vector3.zero);
    }

    public override void Update() {
        timeFalling += Time.deltaTime;
        Vector3 verticalVelocity = movementStateMachine.VerticalVelocity;
        verticalVelocity.y += Time.deltaTime * fallingAcceleration;
        verticalVelocity.y = Mathf.Max(-Mathf.Abs(terminalVelocity), verticalVelocity.y);
        setVerticalVelocity?.Invoke(verticalVelocity);
        
        characterController.Move(Time.deltaTime * (movementStateMachine.VerticalVelocity + movementStateMachine.HorizontalVelocity));

        if (timeFalling < coyoteTime && input.JumpPressed)
            movementStateMachine.TransitionTo<Jump>();
        
        if (characterController.isGrounded)
            movementStateMachine.TransitionTo<Grounded>();
        
        if (!movementStateMachine.CharacterActive)
            movementStateMachine.TransitionTo<Inactive>();
    }

    public override void LateUpdate() { }
    public override void FixedUpdate() { }

    public override void Exit() { }
}
