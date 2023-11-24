using UnityEngine;

namespace OldController {
	public class InactiveState : BaseState {

		private const string State = "InactiveState";
		private Vector3 previousVelocity;
		
		public override void Enter() {
			previousVelocity = owner._velocity;
			owner._velocity = Vector3.zero;
		}

		public override void Update() {
			if (owner.CharacterActive)
				stateMachine.TransitionTo<MoveState>();
		}

		public override void Exit() {
			owner._velocity = previousVelocity;
		}
	}
}