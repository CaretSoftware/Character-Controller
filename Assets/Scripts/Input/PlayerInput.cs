using System;
using UnityEngine;
using Character;

    public class PlayerInput : MonoBehaviour, IInput {
        [SerializeField] private InputReader inputReader;
        
        private bool jumpPressed;
        public bool JumpPressed {
            get => jumpPressed;
            set {
                jumpPressed = value;
                if (jumpPressed)
                    JumpPressedLast = Time.time;
            }
        }

        public float JumpPressedLast { get; private set; } = Mathf.NegativeInfinity;
        public bool JumpReleased { get; set; } = true;
        public bool JumpHold { get; set; }
        public Vector2 Axis { get; set; }
        public bool FirePressed { get; set; }
        public bool FireReleased { get; set; }
        public bool InteractHeld { get; set; }
        public bool PausePressed { get; set; }

        private void Awake() {
            inputReader.MoveEvent += HandleMove;
            inputReader.JumpEvent += HandleJump;
            inputReader.JumpCancelledEvent += HandleCancelledJump;
            inputReader.InteractEvent += HandleInteract;
            inputReader.InteractCanceledEvent += HandleInteractCancelled;
            inputReader.FireEvent += HandleFire;
            inputReader.FireCanceledEvent += HandleCancelledFire;
            inputReader.PauseEvent += HandlePause;
            inputReader.ResumeEvent += HandleResume;
        }

        private void OnDestroy() {
            inputReader.MoveEvent -= HandleMove;
            inputReader.JumpEvent -= HandleJump;
            inputReader.JumpCancelledEvent -= HandleCancelledJump;
            inputReader.InteractEvent -= HandleInteract;
            inputReader.InteractCanceledEvent -= HandleInteractCancelled;
            inputReader.FireEvent -= HandleFire;
            inputReader.FireCanceledEvent -= HandleCancelledFire;
            inputReader.PauseEvent -= HandlePause;
            inputReader.ResumeEvent -= HandleResume;
        }

        private void LateUpdate() {
            if (JumpPressed)
                JumpPressed = false;
        }

        private void HandleMove(Vector2 dir) {
            Axis = dir;
            if (Axis.magnitude > 1f)
                Axis.Normalize();
        }

        private void HandleJump() {
            JumpPressed = true;
            JumpHold = true;
            JumpReleased = false;
        }

        private void HandleCancelledJump() {
            JumpPressed = false;
            JumpHold = false;
            JumpReleased = true;
        }
        
        private void HandleInteract() {
            InteractHeld = true;
        }
        
        private void HandleInteractCancelled() {
            InteractHeld = false;
        }

        private void HandleFire() {
            FirePressed = true;
            FireReleased = false;
        }

        private void HandleCancelledFire() {
            FirePressed = false;
            FireReleased = true;
        }
        
        private void HandlePause() {
            PausePressed = true;
        }

        private void HandleResume() {
            PausePressed = false;
        }
    }
