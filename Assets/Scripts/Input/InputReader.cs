using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, InputHandler.ICharacterActionMapActions, InputHandler.IUIActionMapActions {
    private InputHandler gameInput;

    private void OnEnable() {
        if (gameInput == null) {
            gameInput = new InputHandler();
            
            gameInput.CharacterActionMap.SetCallbacks(this);
            gameInput.UIActionMap.SetCallbacks(this);
            
            SetCharacterMovement();
        }
    }

    public void SetUI() {
        gameInput.CharacterActionMap.Disable();
        gameInput.UIActionMap.Enable();
    }
    
    public void SetCharacterMovement() {
        gameInput.CharacterActionMap.Enable();
        gameInput.UIActionMap.Disable();
    }

    //private InputActionMap[] actionMaps;
    //public void SetActionMap(InputActionMap actionMap) {
    //    foreach (var actMap in actionMaps) {
    //        if (actMap == actionMap)
    //            actionMap.Enable();
    //        else
    //            actMap.Disable();
    //    }
    //}
    
    public event Action<Vector2> MoveEvent;

    public event Action JumpEvent;
    public event Action JumpCancelledEvent;
    
    public event Action InteractEvent;
    public event Action InteractCanceledEvent;

    public event Action FireEvent;
    public event Action FireCanceledEvent;

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

    public void OnFire(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed)
            FireEvent?.Invoke();
        
        if (context.phase == InputActionPhase.Canceled)
            FireCanceledEvent?.Invoke();
    }
    
    public void OnInteract(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) 
            InteractEvent?.Invoke();
        
        if (context.phase == InputActionPhase.Canceled)
            InteractCanceledEvent?.Invoke();
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
