using UnityEngine;

[CreateAssetMenu(menuName = "States/Tank/Movement")]
public class TankMovement : CharacterState {
    private static readonly int TankEntered = Animator.StringToHash("TankEntered");
    private static readonly int Fire = Animator.StringToHash("Fire");
    
    [SerializeField] private float forwardVelocity = 2f;
    [SerializeField] private float rotationVelocity = 120f;
    [SerializeField] private float smoothAcceleration = .42f;
    [SerializeField] private float smoothDeceleration = .14f;
    [SerializeField] private float smoothRotationAcceleration = .5f;
    [SerializeField] private float reloadTime = 2f;
    
    private float dampedVelocity;
    private float dampedRotationVelocity;
    private float currentVelocity;
    private float currentRotationVelocity;
    private bool reloaded = true;
    private float lastShotTime;
    private float nextBoredTime;
    private float boredDuration = 10f;

    public override void Enter() {
        animator.SetBool(TankEntered, true);
    }
    
    private void OnValidate() {
        foreach (CharacterState characterState in instanceCopies) {
            TankMovement copy = Cast<TankMovement>(characterState);
            if (copy == null) continue;
            
            if (copy.forwardVelocity != forwardVelocity)
                copy.forwardVelocity = forwardVelocity;
            if (copy.rotationVelocity != rotationVelocity)
                copy.rotationVelocity = rotationVelocity;
            if (copy.smoothAcceleration != smoothAcceleration)
                copy.smoothAcceleration = smoothAcceleration;
            if (copy.smoothDeceleration != smoothDeceleration)
                copy.smoothDeceleration = smoothDeceleration;
            if (copy.smoothRotationAcceleration != smoothRotationAcceleration)
                copy.smoothRotationAcceleration = smoothRotationAcceleration;
            if (copy.reloadTime != reloadTime)
                copy.reloadTime = reloadTime;
        }
    }

    public override void Update() {
        FireCannon();
        
        SoundHorn();
        
        Movement();

        if (nextBoredTime < Time.time) {
            nextBoredTime = Time.time + boredDuration;
            animator.SetTrigger(Bored);
        }
        
        if (!movementStateMachine.CharacterActive)
            movementStateMachine.TransitionTo<Inactive>();
    }

    private void FireCannon() {
        if (input.FireReleased) {
            reloaded = true;
        }
        if ((input.FirePressed || Input.GetMouseButtonDown(0)) && reloaded && Time.time >= lastShotTime) {
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

    private void Movement() {
        Vector2 inputAxis = input.Axis;
        if (inputAxis == Vector2.zero) {
            characterController.Move(Time.deltaTime * 9.81f * Vector3.down);
            return;
        } 
        
        float inputMagnitude = inputAxis.magnitude;
        
        Move(inputMagnitude, inputAxis);
        Rotate(inputMagnitude, inputAxis);
    }

    private void Move(float inputMagnitude, Vector2 inputAxis) {
        Vector3 forward = myTransform.forward;
        Vector2 tankForward = new Vector2(forward.x, forward.z).normalized;
        
        float dotProduct = Vector2.Dot(inputAxis, tankForward);

        if (dotProduct >= 0f)
            dampedVelocity = Mathf.SmoothDamp(dampedVelocity, inputMagnitude * forwardVelocity, 
                ref currentVelocity, smoothAcceleration);
        else
            dampedVelocity = Mathf.SmoothDamp(dampedVelocity, 0f, ref currentVelocity, smoothDeceleration);
        
        characterController.Move(Time.deltaTime * dampedVelocity * forward + Time.deltaTime * 9.81f * Vector3.down);
    }
    
    private void Rotate(float inputMagnitude, Vector2 inputAxis) {
        Vector3 right = myTransform.right;
        Vector2 tankRight = new Vector2(right.x, right.z).normalized;
        
        if (inputMagnitude >= .1f) {
            dampedRotationVelocity =
                Mathf.SmoothDamp(dampedRotationVelocity, rotationVelocity, ref currentRotationVelocity, 
                    smoothRotationAcceleration);
        } else {
            dampedRotationVelocity =
                Mathf.SmoothDamp(dampedRotationVelocity, 0f, ref currentRotationVelocity, 
                    smoothRotationAcceleration);
        }
        
        float rotation = Vector2.Dot(inputAxis, tankRight);
        if (Mathf.Abs(rotation) < Time.deltaTime) return;
        
        float rotationDirection = Mathf.Sign(rotation);
        myTransform.Rotate(Vector3.up, rotationDirection * inputMagnitude * dampedRotationVelocity * Time.deltaTime);
    }

    public override void Exit() {
        animator.SetBool(TankEntered, false);
    }
}
