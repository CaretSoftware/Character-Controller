using UnityEngine;

namespace OldController {
    public class MoveState : BaseState {

        //private float inAirTime;
        //private float transitionToAirStateTime = .1f;
        
        private const string State = "MoveState";
        public override void Enter() {
            Character._jumpedOnce = false;
            //inAirTime = 0.0f;
        }

        public override void Update() {

            // if (Char._inputMovement.magnitude > float.Epsilon)
            //     Char.Accelerate(Char._inputMovement);
            // else
            //     Char.Decelerate();
            
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
            //     inAirTime += Time.deltaTime;
            // if (inAirTime >= transitionToAirstateTime)
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
                // Vector3.Dot(lowHit.normal, Vector3.up) < .6f &&
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
                    Character._point2Transform.position + stepHeight + maxMagnitude,//direction * Char._colliderRadius,
                    Character._colliderRadius,
                    Vector3.down,
                    out RaycastHit hit, 
                    float.MaxValue, 
                    Character._collisionMask);
                
                Character.transform.position += (stepHeight - hit.distance * Vector3.up) * 1.0f;
            }
        }

        private void ApplyStaticFriction() {

            // if (Char._velocity.magnitude < 
            //     Char.normalForce.magnitude * Char._staticFrictionCoefficient) {
            //     Char._velocity = Vector3.zero;
            // } else {
            //     Char._velocity -= Char._velocity.normalized * Char.normalForce.magnitude *
            //                 Char._kineticFrictionCoefficient;
            // }
            
            if (Vector3.ProjectOnPlane(Character._velocity, Vector3.up).magnitude <
                Character.normalForce.magnitude * Character._staticFrictionCoefficient) {
                
                // float verticalVelocity = Char._velocity.y;
                Character._velocity = Vector3.zero;
                // Char._velocity.y = verticalVelocity;
            }
        }

        private void AddGravityForce() {

            float gravityMovement = -Character._defaultGravity * Time.deltaTime;
            Character._velocity.y += gravityMovement;
        }

        public override void Exit() { }
    }
}