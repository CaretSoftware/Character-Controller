using Character;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Character/AccelerationMove")]

public class AccelerationMove : CharacterState {
    [SerializeField] private float turnSpeedModifier = 3f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private float maxVelocity = 3f;

    public override void Enter() { }

    public override void Update() {

        Vector3 horizontal = HandleHorizontalMovement(characterStateMachine.HorizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontal);
        characterController.Move((horizontal + characterStateMachine.VerticalVelocity) * Time.deltaTime);
        
        if (characterStateMachine.HorizontalVelocity.sqrMagnitude <= float.Epsilon)
            characterStateMachine.TransitionTo<Idle>();
    }

    private Vector3 HandleHorizontalMovement(Vector3 velocity) {
        velocity.x = HandleVelocityChange(velocity.x, input.Axis.x); 
        velocity.z = HandleVelocityChange(velocity.z, input.Axis.y);
        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
        return velocity;
        
        float HandleVelocityChange(float velocityComponent, float input) {

            if (Mathf.Abs(input) > float.Epsilon) {
                velocityComponent = Accelerate(velocityComponent, input);
            }
            else
                velocityComponent = Decelerate(velocityComponent);

            return velocityComponent;

            float Accelerate(float vel, float inp) {
                return vel + inp * acceleration * Time.deltaTime *
                    (ChangedDirection(inp, vel) ? turnSpeedModifier : 1.0f);
            }

            float Decelerate(float vel) {
                if (Mathf.Abs(vel) < deceleration * Time.deltaTime) return 0.0f;
                return vel - vel * deceleration * Time.deltaTime;
            }
            bool ChangedDirection(float inp, float vel) => inp > 0.0f && vel < 0.0f || inp < 0.0f && vel > 0.0f;
        }
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() { }
}
