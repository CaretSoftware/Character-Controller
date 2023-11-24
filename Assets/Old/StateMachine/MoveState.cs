using UnityEngine;

namespace OldController {
    public class MoveState : BaseState {
        
        private const string State = "MoveState";
        public override void Enter() => Character._jumpedOnce = false;

        public override void Update() {
            StepUp();
            
            Character.HandleVelocity();

            if (Vector3.Angle(Character.GroundNormal, Vector3.up) < 40)
                ApplyStaticFriction();
            else
                AddGravityForce();

            
            if (Character.Jumped)
                stateMachine.TransitionTo<JumpState>();

            if (!Character.Grounded)
                stateMachine.TransitionTo<AirState>();

            if (!owner.CharacterActive)
                stateMachine.TransitionTo<InactiveState>();
        }

        private void StepUp() {

            Vector3 stepHeight = Vector3.up * .3f;
            Vector3 velocity = Vector3.ProjectOnPlane(Character._velocity, Vector3.up) * Time.deltaTime;
            Vector3 direction = velocity.normalized;
            float maxDistance = velocity.magnitude + Character._skinWidth;
            
            if (Physics.CapsuleCast(
                    Character._point1Transform.position,
                    Character._point2Transform.position, 
                    Character._colliderRadius, 
                    direction, 
                    out RaycastHit lowHit,
                    maxDistance,
                    Character._collisionMask) &&
                Character._velocity.y < float.Epsilon &&
                
                !Physics.CapsuleCast(
                    Character._point1Transform.position + stepHeight,
                    Character._point2Transform.position + stepHeight, 
                    Character._colliderRadius, 
                    direction, 
                    maxDistance + Character._colliderRadius,
                    Character._collisionMask)) {
                
                Vector3 maxMagnitude = Vector3.ClampMagnitude(direction * Character._colliderRadius, Character._velocity.magnitude);
                Physics.CapsuleCast(
                    Character._point1Transform.position + stepHeight + maxMagnitude,
                    Character._point2Transform.position + stepHeight + maxMagnitude,
                    Character._colliderRadius,
                    Vector3.down,
                    out RaycastHit hit, 
                    float.MaxValue, 
                    Character._collisionMask);
                
                Character.transform.position += (stepHeight - hit.distance * Vector3.up) * 1.0f;
            }
        }

        private void ApplyStaticFriction() {
            if (Vector3.ProjectOnPlane(Character._velocity, Vector3.up).magnitude <
                Character.normalForce.magnitude * Character._staticFrictionCoefficient) {
                
                Character._velocity = Vector3.zero;
            }
        }

        private void AddGravityForce() {
            float gravityMovement = -Character._defaultGravity * Time.deltaTime;
            Character._velocity.y += gravityMovement;
        }

        public override void Exit() { }
    }
}