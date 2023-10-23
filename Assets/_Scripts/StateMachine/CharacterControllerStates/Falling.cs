﻿using System;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Character/Falling")]
public class Falling : CharacterState {
    [SerializeField] private float coyoteTime = .2f;
    [SerializeField] private float terminalVelocity = -30f;
    [field: SerializeField] public float TerminalVelocity { get; private set; }

    private float timeFalling;
    
    private void OnValidate() {
        if (characterSm != null && characterSm.MaxVelocity != terminalVelocity) {
            TerminalVelocity = terminalVelocity;
            setTerminalVelocity?.Invoke(terminalVelocity);
        }
    }
    
    public override void Enter() {
        timeFalling = 0f;
    }

    public override void Update() {
        timeFalling += Time.deltaTime;
        // TODO set Vertical Velocity in characterSM
        // TODO limit falling speed by terminal velocity
        Vector3 verticalVelocity = characterSm.VerticalVelocity;
        verticalVelocity.y += Time.deltaTime * -9.81f;
        verticalVelocity.y = Mathf.Max(-Mathf.Abs(terminalVelocity), verticalVelocity.y);
        
        setVerticalVelocity?.Invoke(verticalVelocity);
        //setHorizontalVelocity?.Invoke(verticalVelocity);
        characterController.Move(Time.deltaTime * (characterSm.VerticalVelocity + characterSm.HorizontalVelocity));

        if (coyoteTime < timeFalling && input.JumpPressed) {
            Debug.Log("Coyote Time");
            characterSm.TransitionTo<Jump>();
        }
        
        if (characterController.isGrounded)
            characterSm.TransitionTo<Grounded>();
    }

    public override void LateUpdate() {
    }

    public override void FixedUpdate() {
    }

    public override void Exit() {
    }
}
