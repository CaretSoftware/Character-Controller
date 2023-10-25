using System;
using Controller;
using UnityEngine;

public class CharacterSM : StateMachine {
    private CharacterController _characterController;
    private Transform _transform;
    private Animator _animator;
    private IInput input;
    private ControllerVariables controllerVars; // TODO move character parameters to this TODO //
    
    private Action<Vector3> setHorizontalVelocity;
    private Action<Vector3> setVerticalVelocity;
    private Action<float> setMaxVelocity;
    private Action<float> setTerminalVelocity;
    private Action<float> setJumpBufferDuration;

    public Vector3 HorizontalVelocity { get; private set; }
    public Vector3 VerticalVelocity { get; private set; }
    public float MaxVelocity { get; private set; }
    public float TerminalVelocity { get; private set; }
    public float JumpBufferDuration { get; private set; }

    protected override void Awake() {
        setJumpBufferDuration += SetJumpBufferDuration;
        setHorizontalVelocity += SetHorizontalVelocity;
        setVerticalVelocity += SetVerticalVelocity;
        setTerminalVelocity += SetTerminalVelocity;
        setMaxVelocity += SetMaxVelocity;

        _characterController = GetComponent<CharacterController>();
        _transform = transform;
        _animator = GetComponentInChildren<Animator>();
        input = GetComponent<IInput>();
        
        foreach (State state in states) {
            var instance = ((CharacterState)state).Copy();
            _states.Add(instance.GetType(), instance);
            currentState ??= instance;
            
            instance.stateMachine = this;
            
            instance.Init(this, _characterController, transform, _animator, input,
                setHorizontalVelocity, setVerticalVelocity, setMaxVelocity, setTerminalVelocity, setJumpBufferDuration);
            
            if (instance.GetType() == typeof(Jump))
                SetJumpBufferDuration(((Jump)instance).JumpBufferDuration);
            
            if (instance.GetType() == typeof(Falling))
                SetTerminalVelocity(((Falling)instance).TerminalVelocity);
            
            if (instance.GetType() == typeof(Move))
                SetMaxVelocity(((Move)instance).CharacterMaxSpeed);
        }

        queuedState = currentState;
        
        if (currentState != null)
            currentState.Enter();
    }

    private void OnDestroy() {
        setJumpBufferDuration -= SetJumpBufferDuration;
        setHorizontalVelocity -= SetHorizontalVelocity;
        setVerticalVelocity -= SetVerticalVelocity;
        setMaxVelocity -= SetMaxVelocity;
    }

    private void SetJumpBufferDuration(float jumpBufferDuration) => JumpBufferDuration = jumpBufferDuration;

    private void SetVerticalVelocity(Vector3 verticalVelocity) => VerticalVelocity = verticalVelocity;

    private void SetHorizontalVelocity(Vector3 horizontalVelocity) => HorizontalVelocity = horizontalVelocity;
    
    private void SetMaxVelocity(float maxVelocity) => MaxVelocity = maxVelocity;
    
    private void SetTerminalVelocity(float terminalVelocity) => TerminalVelocity = terminalVelocity;
    
#if UNITY_EDITOR
    private readonly int textWidth = 200;
    private readonly int padding = 24;

    private GUIStyle labelStyle;
    private GUIStyle LabelStyle =>
        labelStyle ??= new GUIStyle(GUI.skin.label) {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight,
            wordWrap = false,
            normal = { textColor = Color.white }
        };

    private void OnGUI() {
        Rect rect = new Rect {
            xMin = Screen.width - textWidth - padding, 
            yMin = padding, 
            width = textWidth,
            height = padding * 2f,
        };
        
        Rect next = new Rect {
            xMin = Screen.width - textWidth - padding, 
            yMin = padding * 2f, 
            width = textWidth,
            height = padding * 2f,
        };

        UnityEditor.Handles.BeginGUI();
        GUI.Label(rect, currentState.GetType().Name, LabelStyle);
        GUI.color = new Color(1f, 1f, 1f, .5f);
        if (lastState != null)
            GUI.Label(next, lastState.GetType().Name, LabelStyle);
        UnityEditor.Handles.EndGUI();
    }
#endif
}
