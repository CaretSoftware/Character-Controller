using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Grounded")]
public class Grounded : CharacterState {
    
    public override void Enter() {
        setVerticalVelocity?.Invoke(new Vector3(0f, -9.81f, 0f));
    }

    public override void Update() {
        if (characterController.velocity.sqrMagnitude <= float.Epsilon)
            characterSm.TransitionTo<Idle>();
        
        if (input.Axis.sqrMagnitude > float.Epsilon || characterController.velocity.sqrMagnitude > float.Epsilon)
            characterSm.TransitionTo<Move>();
        
        if (!characterController.isGrounded)
            characterSm.TransitionTo<Falling>();

        if (input.JumpPressed || Time.time <= input.JumpPressedLast + characterSm.JumpBufferDuration)
            characterSm.TransitionTo<Jump>();
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() { }
}
