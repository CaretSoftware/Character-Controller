using UnityEngine;
using Character;
    public interface IInput {
        public bool JumpPressed { get; set; }
        public float JumpPressedLast { get; }
        public bool JumpReleased { get; set; }
        public bool JumpHold { get; set; }
        public bool FirePressed { get; set; }
        public bool FireReleased { get; set; }
        public bool InteractHeld { get; set; }
        public bool PausePressed { get; set; }
        public Vector2 Axis { get; set; }
    }
