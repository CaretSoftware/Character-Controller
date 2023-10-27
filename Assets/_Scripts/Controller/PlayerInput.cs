using UnityEngine;

namespace Controller {
    [SelectionBase]
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

        public float JumpPressedLast { get; private set; }

        public bool JumpReleased { get; set; } = true;
        public bool JumpHold { get; set; }
        public Vector2 Axis { get; set; }
        private bool locked;

        private void Awake() {
            AchievementManager.onLevelFinishedDelegate += LockControls;
        }

        private void OnDestroy() {
            AchievementManager.onLevelFinishedDelegate -= LockControls;
        }

        private void LockControls() => locked = true;

        private void Update() {
            if (locked) return;
            JumpPressed = Input.GetButtonDown("Jump");
            JumpReleased = Input.GetButtonUp("Jump");
            JumpHold = Input.GetButton("Jump");
            Vector2 axis = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
            if (axis.magnitude > 1f)
                axis.Normalize();
            Axis = axis;
        }
    }
}
