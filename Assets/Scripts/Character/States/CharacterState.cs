using System;
using System.Collections.Generic;
using Character;
using UnityEngine;

public class CharacterState : State {
    public List<CharacterState> instanceCopies = new List<CharacterState>();
    
    protected static readonly int MoveZ = Animator.StringToHash("ForwardVelocity");
    protected static readonly int IsJumping = Animator.StringToHash("Jump");
    protected static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    protected static readonly int Bored = Animator.StringToHash("Bored");
    protected static readonly int Sliding = Animator.StringToHash("Sliding");

    protected Action<Vector3> setHorizontalVelocity;
    protected Action<Vector3> setVerticalVelocity;
    protected Action<float> setMaxSpeed;
    protected Action<float> setTerminalVelocity;
    protected Action<float> setJumpBufferDuration;
    protected Action<float> rotateForward;
    
    protected MovementStateMachine movementStateMachine;
    protected CharacterController characterController;
    protected Transform myTransform;
    protected Animator animator;
    protected IInput input;

    public void Init(
            MovementStateMachine movementStateMachine, 
            CharacterController characterController,
            Transform characterTransform,
            Animator animator,
            IInput input, 
            Action<Vector3> setHorizontalVelocity,
            Action<Vector3> setVerticalVelocity,
            Action<float> setMaxSpeed,
            Action<float> setTerminalVelocity,
            Action<float> setJumpBufferDuration,
            Action<float> rotateForward) {
        
        this.movementStateMachine = movementStateMachine;
        this.characterController = characterController;
        this.myTransform = characterTransform;
        this.animator = animator;
        this.input = input;
        this.setHorizontalVelocity = setHorizontalVelocity;
        this.setVerticalVelocity = setVerticalVelocity;
        this.setMaxSpeed = setMaxSpeed;
        this.setTerminalVelocity = setTerminalVelocity;
        this.setJumpBufferDuration = setJumpBufferDuration;
        this.rotateForward = rotateForward;
    }

    private void OnDisable() => instanceCopies.Clear();

    public CharacterState Copy() {
        CharacterState instance = Instantiate(this);
        instanceCopies.Add(instance);
        return instance;
    }

    protected static T Cast<T>(CharacterState state) where T : CharacterState => state as T;
}
