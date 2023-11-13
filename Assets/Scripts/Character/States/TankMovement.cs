using UnityEngine;

[CreateAssetMenu(menuName = "States/Tank/Movement")]
public class TankMovement : CharacterState {
    private static readonly int TankEntered = Animator.StringToHash("TankEntered");
    private static readonly int Fire = Animator.StringToHash("Fire");
    
    [SerializeField] private float forwardVelocity = 2f;
    [SerializeField] private float rotationVelocity = 30f;
    [SerializeField] private float smoothAcceleration = .2f;
    [SerializeField] private float smoothDeceleration = .05f;
    [SerializeField] private float smoothRotationAcceleration = .5f;
    [SerializeField] private float reloadTime = 1f;
    
    private float dampedVelocity;
    private float dampedRotationVelocity;
    private float currentVelocity;
    private float currentRotationVelocity;
    private bool reloaded = true;
    private float lastShotTime;

    public override void Enter() {
        animator.SetBool(TankEntered, true);
    }

    public override void Update() {
        FireCannon();
        
        SoundHorn();
        
        Movement();
        
        if (!movementStateMachine.CharacterActive)
            movementStateMachine.TransitionTo<Inactive>();
    }

    private void Movement() {
        Vector2 inputAxis = input.Axis;
        if (inputAxis == Vector2.zero) {
            characterController.Move(Time.deltaTime * 9.81f * Vector3.down);
            return;
        }
        
        Vector3 forward = myTransform.forward;
        Vector3 right = myTransform.right;
        Vector2 tankForward = new Vector2(forward.x, forward.z).normalized;
        Vector2 tankRight = new Vector2(right.x, right.z).normalized;
        float inputMagnitude = inputAxis.magnitude;
        
        float direction = Mathf.Sign(Vector2.Dot(inputAxis, tankRight));

        float dotProduct = Vector2.Dot(inputAxis, tankForward);

        if (dotProduct >= .5f && inputMagnitude > .2f) {
            dampedVelocity = Mathf.SmoothDamp(dampedVelocity, inputMagnitude * forwardVelocity, 
                ref currentVelocity, smoothAcceleration);
        }

        if (dotProduct < .5f || inputMagnitude < .2f) {
            dampedVelocity = Mathf.SmoothDamp(dampedVelocity, 0f, ref currentVelocity, smoothDeceleration);
        }
        
        characterController.Move(Time.deltaTime * dampedVelocity * forward + Time.deltaTime * 9.81f * Vector3.down);
        
        if (inputMagnitude >= .1f && dotProduct < .95f) {
            dampedRotationVelocity =
                    Mathf.SmoothDamp(dampedRotationVelocity, rotationVelocity, ref currentRotationVelocity, 
                            smoothRotationAcceleration);
        } else {
            dampedRotationVelocity =
                    Mathf.SmoothDamp(dampedRotationVelocity, 0f, ref currentRotationVelocity, 
                            smoothRotationAcceleration);
        }
        
        Rotate(direction, inputMagnitude);
    }

    private void FireCannon() {
        if (input.FireReleased) {
            reloaded = true;
        }
        if (input.FirePressed && reloaded && Time.time >= lastShotTime) {
            lastShotTime = Time.time + reloadTime;
            reloaded = false;
            SoundManager.PlaySound(Sound.CannonShot);
            animator.SetTrigger(Fire);
        }
    }

    private void SoundHorn() {
        if (input.JumpPressed)
            SoundManager.PlaySound(Sound.CarHornSmall);
    }
    
    private void Rotate(float direction, float inputMagnitude) =>
        myTransform.Rotate(Vector3.up, direction * inputMagnitude * dampedRotationVelocity * Time.deltaTime);

    public override void Exit() {
        animator.SetBool(TankEntered, false);
    }
}
