using System;
using System.Collections.Generic;
using Controller;
using UnityEngine;

public class CharacterState : State {
    public List<CharacterState> instanceCopies = new List<CharacterState>();

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

    private void OnDisable() => instanceCopies.Clear();

    public virtual CharacterState Copy() {
        CharacterState instance = Instantiate(this);
        instanceCopies.Add(instance);
        return instance;
    }

    protected static T Cast<T>(CharacterState state) where T : CharacterState => state as T;
}
