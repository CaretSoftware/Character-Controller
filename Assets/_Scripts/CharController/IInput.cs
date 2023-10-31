using UnityEngine;

namespace Controller {
    public interface IInput {
        public bool JumpPressed { get; set; }
        public float JumpPressedLast { get; }
        public bool JumpReleased { get; set; }
        public bool JumpHold { get; set; }
        public Vector2 Axis { get; set; }
    }
}
