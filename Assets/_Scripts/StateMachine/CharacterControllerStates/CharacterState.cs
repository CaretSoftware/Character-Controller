using System;
using Controller;
using UnityEngine;

public class CharacterState : State {
    protected Action<Vector3> setHorizontalVelocity;
    protected Action<Vector3> setVerticalVelocity;
    protected Action<float> setMaxSpeed;
    protected Action<float> setTerminalVelocity;
    protected Action<float> setJumpBufferDuration;
    
    protected CharacterSM characterSm;
    protected CharacterController characterController;
    protected Transform myTransform;
    protected Animator animator;
    protected IInput input;
    
    public void Init(
            CharacterSM characterSm, 
            CharacterController characterController,
            Transform characterTransform,
            Animator animator,
            IInput input, 
            Action<Vector3> setHorizontalVelocity,
            Action<Vector3> setVerticalVelocity,
            Action<float> setMaxSpeed,
            Action<float> setTerminalVelocity,
            Action<float> setJumpBufferDuration) {
        
        this.characterSm = characterSm;
        this.characterController = characterController;
        this.myTransform = characterTransform;
        this.animator = animator;
        this.input = input;
        this.setHorizontalVelocity = setHorizontalVelocity;
        this.setVerticalVelocity = setVerticalVelocity;
        this.setMaxSpeed = setMaxSpeed;
        this.setTerminalVelocity = setTerminalVelocity;
        this.setJumpBufferDuration = setJumpBufferDuration;
    }
}
