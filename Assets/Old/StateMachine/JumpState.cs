using UnityEngine;

namespace OldController {
    public class JumpState : BaseState {
        private bool _falling = false;

        private const string State = "JumpState";
        public override void Enter() {
            _falling = false;
            Character._jumpedOnce = true;
            Character.airTime = 0;
            Character._velocity.y = Character._jumpForce;
        }

        public override void Update() {

            Character.AirControl();

            Vector3 gravityMovement = Character._defaultGravity * Time.deltaTime * Vector3.down;

            Character._velocity += gravityMovement;

            Character.ApplyAirFriction();

            if (!Character.HoldingJump || Character._velocity.y < float.Epsilon)
                _falling = true;

            if (_falling)
                stateMachine.TransitionTo<AirState>();

            if (WallRunState.Requirement(Character))
                stateMachine.TransitionTo<WallRunState>();

            if (Character.Grounded && Character._velocity.y < float.Epsilon)
                stateMachine.TransitionTo<MoveState>();
            
            if (!owner.CharacterActive)
                stateMachine.TransitionTo<InactiveState>();
        }

        public override void Exit() { }
    }
}