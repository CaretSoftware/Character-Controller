using System.Diagnostics.CodeAnalysis;
using System;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Falling")]
public class Falling : CharacterState {
    [SerializeField] private float coyoteTime = .2f;
    [SerializeField] private float terminalVelocity = -50f; // Serialized variable used in editor script
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
    }

    public override void Update() {
        timeFalling += Time.deltaTime;
        // TODO set Vertical Velocity in characterSM
        // TODO limit falling speed by terminal velocity
        Vector3 verticalVelocity = characterMovement.VerticalVelocity;
        verticalVelocity.y += Time.deltaTime * -9.81f;
        verticalVelocity.y = Mathf.Max(-Mathf.Abs(terminalVelocity), verticalVelocity.y);
        
        setVerticalVelocity?.Invoke(verticalVelocity);
        //setHorizontalVelocity?.Invoke(verticalVelocity);
        characterController.Move(Time.deltaTime * (characterMovement.VerticalVelocity + characterMovement.HorizontalVelocity));

        if (coyoteTime < timeFalling && input.JumpPressed) {
            Debug.Log("Coyote Time");
            characterMovement.TransitionTo<Jump>();
        }
        
        if (characterController.isGrounded)
            characterMovement.TransitionTo<Grounded>();
    }

    public override void LateUpdate() {
    }

    public override void FixedUpdate() {
    }

    public override void Exit() {
    }
}
