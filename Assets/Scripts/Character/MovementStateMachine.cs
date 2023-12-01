using System;
using UnityEngine;

namespace Character {
    [SelectionBase]
    public class MovementStateMachine : StateMachine {
        private CharacterController _characterController;
        private Transform _transform;
        private Animator _animator;
        private IInput input;
        private Vector3 smoothRotation;
        private Vector3 currentVelocity;
        private LayerMask platformLayer = 1 << 7;
        private Ray ray;
        private int characterIndex = -1;

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
        public bool CharacterActive { get; private set; }
        public StateSO InitialState { get; private set; }

        [ContextMenu("RESET Character")]
        private void ResetCharacter() {
            transform.position = Vector3.zero;
            _characterController.Move(Vector3.zero);
        }
        
        protected override void Awake() {
            setJumpBufferDuration += SetJumpBufferDuration;
            setHorizontalVelocity += SetHorizontalVelocity;
            setVerticalVelocity += SetVerticalVelocity;
            setTerminalVelocity += SetTerminalVelocity;
            setMaxVelocity += SetMaxVelocity;
            rotateForward += RotateForward;
            CharacterSwapper.cycleCharacter += CharacterSwap;
            characterIndex = CharacterSwapper.GetCharacterIndex(this);
            
            _characterController = GetComponent<CharacterController>();
            _characterController.Move(Vector3.down); // Move() sets IsGrounded variable in CharacterController
            _transform = transform;
            _animator = GetComponentInChildren<Animator>();
            input = GetComponent<IInput>();
            
            foreach (StateSO state in states) {
                var instance = ((CharacterState)state).Copy();
                _states.Add(instance.GetType(), instance);
                currentState ??= InitialState = instance;
                
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
            base.Update();
            ray.origin = _transform.position + Vector3.up * _characterController.radius;
            ray.direction = Vector3.down;
            if (!_characterController.isGrounded) {
                platform = null;
                _transform.parent = null;
            }
        }

        private void OnDestroy() {
            setJumpBufferDuration -= SetJumpBufferDuration;
            setHorizontalVelocity -= SetHorizontalVelocity;
            setVerticalVelocity -= SetVerticalVelocity;
            setMaxVelocity -= SetMaxVelocity;
            rotateForward -= RotateForward;
            CharacterSwapper.cycleCharacter -= CharacterSwap;
        }

        private void SetJumpBufferDuration(float jumpBufferDuration) => JumpBufferDuration = jumpBufferDuration;

        private void SetVerticalVelocity(Vector3 verticalVelocity) => VerticalVelocity = verticalVelocity;

        private void SetHorizontalVelocity(Vector3 horizontalVelocity) => HorizontalVelocity = horizontalVelocity;
        
        private void SetMaxVelocity(float maxVelocity) => MaxVelocity = maxVelocity;
        
        private void SetTerminalVelocity(float terminalVelocity) => TerminalVelocity = terminalVelocity;

        private void CharacterSwap(int index) => CharacterActive = characterIndex == index;
        
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

        private void OnControllerColliderHit(ControllerColliderHit hit) => PlatformCheck(hit);

        private Transform platform;
        private void PlatformCheck(ControllerColliderHit hit) {
            if (_transform == null) return;
            bool hitPlatform = ((1 << hit.gameObject.layer) & platformLayer.value) != 0;
            bool hitIsLastPlatform = platform != null && hit.transform == platform;
            bool platformBelow = _transform.position.y > hit.transform.position.y;
            bool hitIsFromBelow = (_characterController.collisionFlags & CollisionFlags.CollidedBelow) != 0;

            if (!hitPlatform && hitIsFromBelow) {
                platform = null;
                _transform.parent = null;
                return;
            }

            if (!hitPlatform || hitIsLastPlatform || !hitIsFromBelow || !platformBelow)
                return;
            
            platform = hit.transform;

            _transform.parent = platform;
            PlaceCharacterOnPlatform();
            
            void PlaceCharacterOnPlatform() {
                const float platformHeight = 1.25f;
                Vector3 position = _transform.position;
                Vector3 platformPosition = platform.position;
                position.y = platformPosition.y + platformHeight;
                _transform.position = position;
            }
        }

        //private Vector3 lastPlatformPosition;
        //private void SetPlatformVelocity() {
        //    if (platform == null) return;
        //    
        //    Vector3 currPosition = platform.position;
        //    PlatformVelocity = currPosition - lastPlatformPosition;
        //    lastPlatformPosition = currPosition;
        //}
        
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
            if (!CharacterActive) return;
            
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
