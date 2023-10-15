using UnityEngine;

namespace Controller {
    [SelectionBase]
    public class PlayerInput : MonoBehaviour, IInput {
        public bool JumpPressed { get; set; }
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
            JumpHold = Input.GetButton("Jump");
            Vector2 axis = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
            if (axis.magnitude > 1f)
                axis.Normalize();
            Axis = axis;
        }
    }
}
