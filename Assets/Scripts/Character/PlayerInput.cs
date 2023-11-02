using System;
using UnityEngine;

namespace Character {
    public class PlayerInput : MonoBehaviour, IInput {
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

        private void Awake() {
            inputReader.MoveEvent += HandleMove;
            inputReader.JumpEvent += HandleJump;
            inputReader.JumpCancelledEvent += HandleCancelledJump;
        }

        private void OnDestroy() {
            inputReader.MoveEvent -= HandleMove;
            inputReader.JumpEvent -= HandleJump;
            inputReader.JumpCancelledEvent -= HandleCancelledJump;
        }

        private void LateUpdate() {
            if (JumpPressed)
                JumpPressed = false;
        }

        [SerializeField] private InputReader inputReader;

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
    }
}
