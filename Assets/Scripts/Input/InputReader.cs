using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, InputHandler.ICharacterMovementActions, InputHandler.IUIActions {
    private InputHandler gameInput;

    private void OnEnable() {
        if (gameInput == null) {
            gameInput = new InputHandler();
            
            gameInput.CharacterMovement.SetCallbacks(this);
            gameInput.UI.SetCallbacks(this);
            
            SetCharacterMovement();
        }
    }

    public void SetUI() {
        gameInput.CharacterMovement.Disable();
        gameInput.UI.Enable();
    }
    
    public void SetCharacterMovement() {
        gameInput.CharacterMovement.Enable();
        gameInput.UI.Disable();
    } 
    
    public event Action<Vector2> MoveEvent;

    public event Action JumpEvent;
    public event Action JumpCancelledEvent;

    public event Action PauseEvent;
    public event Action ResumeEvent;
    
    public void OnMove(InputAction.CallbackContext context) {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) 
            JumpEvent?.Invoke();
        
        if (context.phase == InputActionPhase.Canceled)
            JumpCancelledEvent?.Invoke();
    }

    public void OnPause(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            PauseEvent?.Invoke();
            SetUI();
        }
    }

    public void OnResume(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            ResumeEvent?.Invoke();
            SetCharacterMovement();
        }
    }
}
