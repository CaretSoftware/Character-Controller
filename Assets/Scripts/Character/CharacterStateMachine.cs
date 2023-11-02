using System;
using UnityEngine;

namespace Character {
    public class CharacterStateMachine : StateMachine {
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
        private Action<float> rotateForward;

        public Vector3 HorizontalVelocity { get; private set; }
        public Vector3 VerticalVelocity { get; private set; }
        public float MaxVelocity { get; private set; }
        public float TerminalVelocity { get; private set; }
        public float JumpBufferDuration { get; private set; }
        public Vector3 SlopeSlideVelocity { get; private set; }

        protected override void Awake() {
            setJumpBufferDuration += SetJumpBufferDuration;
            setHorizontalVelocity += SetHorizontalVelocity;
            setVerticalVelocity += SetVerticalVelocity;
            setTerminalVelocity += SetTerminalVelocity;
            setMaxVelocity += SetMaxVelocity;
            rotateForward += RotateForward;
            
            _characterController = GetComponent<CharacterController>();
            _characterController.Move(Vector3.down); // Moving sets IsGrounded variable
            _transform = transform;
            _animator = GetComponent<Animator>();
            input = GetComponent<IInput>();
            
            foreach (State state in states) {
                var instance = ((CharacterState)state).Copy();
                _states.Add(instance.GetType(), instance);
                currentState ??= instance;
                
                instance.stateMachine = this;
                
                instance.Init(this, _characterController, transform, _animator, input, setHorizontalVelocity, 
                    setVerticalVelocity, setMaxVelocity, setTerminalVelocity, setJumpBufferDuration, rotateForward);
                
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
            
            stateHistory.Add(currentState);
        }

        protected override void Update() {
            SetSlopeSlideVelocity();
            base.Update();
        }

        private void OnDestroy() {
            setJumpBufferDuration -= SetJumpBufferDuration;
            setHorizontalVelocity -= SetHorizontalVelocity;
            setVerticalVelocity -= SetVerticalVelocity;
            setMaxVelocity -= SetMaxVelocity;
            rotateForward -= RotateForward;
        }

        private void SetJumpBufferDuration(float jumpBufferDuration) => JumpBufferDuration = jumpBufferDuration;

        private void SetVerticalVelocity(Vector3 verticalVelocity) => VerticalVelocity = verticalVelocity;

        private void SetHorizontalVelocity(Vector3 horizontalVelocity) => HorizontalVelocity = horizontalVelocity;
        
        private void SetMaxVelocity(float maxVelocity) => MaxVelocity = maxVelocity;
        
        private void SetTerminalVelocity(float terminalVelocity) => TerminalVelocity = terminalVelocity;

        private Vector3 smoothRotation;
        private Vector3 currentVelocity;
        private void RotateForward(float rotationSmoothTime) {
            Vector3 horizontalVelocity = _characterController.velocity;
            horizontalVelocity.y = 0f;
            
            if (horizontalVelocity != Vector3.zero) {
                Vector3 targetRotation = Quaternion.LookRotation(horizontalVelocity).eulerAngles;

                smoothRotation = new Vector3(
                    Mathf.SmoothDampAngle(smoothRotation.x, targetRotation.x, ref currentVelocity.x, rotationSmoothTime),
                    Mathf.SmoothDampAngle(smoothRotation.y, targetRotation.y, ref currentVelocity.y, rotationSmoothTime),
                    Mathf.SmoothDampAngle(smoothRotation.z, targetRotation.z, ref currentVelocity.z, rotationSmoothTime)
                );
                _transform.eulerAngles = smoothRotation;
            }
        }
        
        private RaycastHit sphereHitInfo;
        private RaycastHit rayHitInfo;
        private float slopeSlideMagnitude;
        private float rayAngle;
        private float angle;
        private Ray ray;
        private void SetSlopeSlideVelocity() {
            SlopeSlideVelocity -= SlopeSlideVelocity * (Time.deltaTime * 3f);   // TODO Smooth damp
            float radius = _characterController.radius;
            Vector3 position = _transform.position;
            
            ray.origin = position + 1.01f * radius * Vector3.up;
            ray.direction = Vector3.down;

            Debug.DrawRay(position + Vector3.up, Vector3.down * 1.75f, Color.green);
                
            if (Physics.SphereCast(ray, _characterController.radius, out sphereHitInfo, 1.02f * radius)) {
                rayAngle = float.MaxValue;
                if (Physics.Raycast(ray, out rayHitInfo, 1.75f)) {
                    rayAngle = Vector3.Angle(rayHitInfo.normal, Vector3.up);
                }
                angle = Mathf.Min(Vector3.Angle(sphereHitInfo.normal, Vector3.up), rayAngle);
                    
                if (angle >= _characterController.slopeLimit) {
                    SlopeSlideVelocity =
                        Vector3.ProjectOnPlane(new Vector3(0f, -Mathf.Abs(VerticalVelocity.y), 0f), sphereHitInfo.normal);
                    return;
                }
            }

            slopeSlideMagnitude = Vector3.ProjectOnPlane(SlopeSlideVelocity, Vector3.up).magnitude;
            if (slopeSlideMagnitude == 0f)
                return;

            if (slopeSlideMagnitude > 5f)                                       // TODO parameter
                return;
            SlopeSlideVelocity = Vector3.zero;
        }
        
    #if UNITY_EDITOR
        private static CharacterStateMachine _characterStateMachine;
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
            _characterStateMachine ??= this;
            if (_characterStateMachine != this) return;
            Rect rect = new Rect {
                xMin = Screen.width - textWidth - padding, 
                yMin = padding, 
                width = textWidth,
                height = padding * 16f,
            };

            UnityEditor.Handles.BeginGUI();

            GUI.Label(rect, stateHistory[^1].GetType().Name, LabelStyle);
            
            GUI.color = new Color(1f, 1f, 1f, .5f);

            for (int i = stateHistory.Count - 2; i >= 0; i--) {
                rect.yMin = padding * (stateHistory.Count - i);
                GUI.Label(rect, stateHistory[i].GetType().Name, LabelStyle);
            }
            
            UnityEditor.Handles.EndGUI();
        }
    #endif
    }
}
