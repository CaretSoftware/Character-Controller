using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OldController {
	public class WallJumpState : BaseState {
		private bool _falling = false;

		private const string State = "WallJumpState";
		public override void Enter() {
			
			_falling = false;
			Vector3 verticalVelocity = Vector3.ProjectOnPlane(Character._velocity, Vector3.up);

			RaycastHit rightHit = RayCast(Character, Character.transform.right);
			RaycastHit leftHit = RayCast(Character, -Character.transform.right);

			Vector3 redirectedVelocity = Character._velocity;
			Vector3 wallProjectionVector;
			if (rightHit.collider) {
				wallProjectionVector = Vector3.ProjectOnPlane(verticalVelocity, rightHit.normal);
				redirectedVelocity = RedirectVelocity(wallProjectionVector, rightHit.normal);
			}
			else if (leftHit.collider) {
				wallProjectionVector = Vector3.ProjectOnPlane(verticalVelocity, leftHit.normal);
				redirectedVelocity = RedirectVelocity(wallProjectionVector, leftHit.normal);
			}

			redirectedVelocity += redirectedVelocity.normalized * 1.0f;
			redirectedVelocity.y = Character._jumpForce;;
			Character._velocity = redirectedVelocity;
		}

		private Vector3 RedirectVelocity(Vector3 velocity, Vector3 normal) {

			Vector3 direction = velocity.normalized;
			float magnitude = velocity.magnitude;

			return magnitude * Vector3.Slerp(direction, normal, .5f);
		}

		public override void Update() {
            
			Character.AirControl();

			float gravityMovement = -Character._defaultGravity * 2.0f * Time.deltaTime;
			Character._velocity.y += gravityMovement;
			
			Character.ApplyAirFriction();
			
			if (!Character.HoldingJump || Character._velocity.y < float.Epsilon)
				_falling = true;

			if (_falling)
				stateMachine.TransitionTo<AirState>();

			// if (WallRunState.Requirement(Player))
			// 	stateMachine.TransitionTo<WallRunState>();

			if (Character.Grounded && Character._velocity.y < float.Epsilon)
				stateMachine.TransitionTo<MoveState>();
		}

		private static RaycastHit RayCast(OldCharacterController character, Vector3 direction) {
			Ray ray = new Ray( character._point2Transform.position/*Player.transform.position + Player._point2*/, direction);
			Physics.Raycast(ray, out var hit, character._colliderRadius + .5f, character._collisionMask);
			return hit;
		}

		public override void Exit() { }
	}
}