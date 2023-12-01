using UnityEngine;

namespace Character {
    [RequireComponent(typeof(CharacterController))]
    public class CharController : MonoBehaviour {
        private const float UngroundedDelay = 0.05f;
        private static readonly int MoveZ = Animator.StringToHash("ForwardVelocity");
        private static readonly int IsJumping = Animator.StringToHash("Jump");
        //private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        [SerializeField] private Animator animator;
        [SerializeField] private LayerMask platformLayer;
        [Header("Controller Settings")]
        [SerializeField, Range(0f, 10f)] private float playerSpeed = 7.0f;
        [SerializeField, Range(0f, 10f)] private float jumpHeight = 5f;
        [SerializeField, Range(-100f, 0f)] private float gravity = -19.62f;
        [SerializeField, Range(0f, 10f)] private float slopeSlideLimit = 5f;

        [Header("Apex Modifiers")] 
        [SerializeField, Range(-2f, 0f)] private float minApexVelocityThreshold = -.62f;
        [SerializeField, Range(0f, 2f)] private float maxApexVelocityThreshold = .2f;
        [Header("Variable Jump Height")]
        [SerializeField, Range(0f, 10f)] private float fallingGravityScale = 10f;
        [Header("Jump Grace Times")]
        [SerializeField, Range(0f, 1f)] private float coyoteTime = .2f;
        [SerializeField, Range(0f, 1f)] private float jumpBuffering = .2f;
        [Header("Turning Smoothing")]
        [SerializeField, Range(0f, .2f)] private float rotationSmoothTime = .1f;
        [SerializeField, Range(0f, 1f)] private float groundSmoothTime = .1f;
        [SerializeField, Range(0f, 1f)] private float airSmoothTime = 1f;
        [Header("Outfit Selector")]
        [SerializeField] public Outfit[] outfits;
        
        private CharacterController characterController;
        private IInput input;
        private Vector3 characterVelocity;
        private Vector3 horizontalVelocity;
        private Vector3 slopeSlideVelocity;
        private float verticalVelocity;
        private float coyoteTimeLast;
        private float jumpBufferTime;
        private float gravityScale = 1f;
        private float groundedTime;
        private bool grounded;
        private bool animationGrounded;
        private bool jumpReleased;
        private Transform platform;
        private Vector3 lastPlatformPosition;
        private Vector3 platformMovement;
        private Ray ray;
        private float radius;
        private Transform _transform;
        
        private void Awake() {
            _transform = transform;
            characterController = GetComponent<CharacterController>();
            input = GetComponent<IInput>();
            radius = characterController.radius;
        }

        private void Update() {
            SetGrounded();
            SetHorizontalVelocity();
            SetVerticalVelocity();
            SetSlopeSlideVelocity();
            SetForwardDirection();
            SetCharacterVelocity();

            //if (!Sliding()) {
            
            AdjustVelocityToSlope(ref characterVelocity);
            characterController.Move((characterVelocity + slopeSlideVelocity + externalForces) * Time.deltaTime + platformMovement);

            externalForces *= 1f - Time.deltaTime;
            if (externalForces.magnitude < 5f) externalForces = Vector3.zero;
            //} 
            //else {
            //    Debug.Log("SLIDING");
            //    slopeSlideVelocity.y = verticalVelocity;
            //    characterController.Move(slopeSlideVelocity * Time.deltaTime);
            //}
            
            platformMovement = Vector3.zero;
            SetAnimationsParameters();

            //bool Sliding() { return slopeSlideVelocity.magnitude > .2f; }
        }

        private void SetGrounded() {
            grounded = characterController.isGrounded;
        }

        private Vector3 vertical;
        private Vector3 horizontal;
        private RaycastHit slopeHitInfo;
        private Quaternion slopeRotation;
        private void AdjustVelocityToSlope(ref Vector3 velocity) {
            horizontal = velocity;
            horizontal.y = 0f;
            vertical.y = velocity.y;
            if (grounded && Physics.Raycast(ray, out slopeHitInfo, 1.75f)) {
                slopeRotation = Quaternion.FromToRotation(Vector3.up, slopeHitInfo.normal);
                velocity = slopeRotation * horizontal;
                velocity.y = Mathf.Min(0f, velocity.y);
                velocity += vertical;
            }
        }
        
        private float xCurrentVelocity;
        private float yCurrentVelocity;
        private Vector2 smoothInput;
        private void SetHorizontalVelocity() {
            float currSmoothX = input.Axis.x != 0 ? 0f : 
                    grounded ? groundSmoothTime : airSmoothTime;
            float currSmoothY = input.Axis.y != 0 ? 0f : 
                    grounded ? groundSmoothTime : airSmoothTime;
            smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.Axis.x, ref xCurrentVelocity, currSmoothX);
            smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.Axis.y, ref yCurrentVelocity, currSmoothY);
            
            horizontalVelocity.x = Mathf.Abs(smoothInput.x) > .1f ? smoothInput.x * playerSpeed : 0f;
            horizontalVelocity.z = Mathf.Abs(smoothInput.y) > .1f ? smoothInput.y * playerSpeed : 0f;
        }

        private Vector3 currentRotationVelocity;
        private Vector3 newRotation;
        private float xVel;
        private float yVel;
        private float zVel;
        private void SetForwardDirection() {
            if (horizontalVelocity != Vector3.zero) {
                Vector3 targetRotation = Quaternion.LookRotation(horizontalVelocity).eulerAngles;

                newRotation = new Vector3(
                    Mathf.SmoothDampAngle(newRotation.x, targetRotation.x, ref xVel, rotationSmoothTime),
                    Mathf.SmoothDampAngle(newRotation.y, targetRotation.y, ref yVel, rotationSmoothTime),
                    Mathf.SmoothDampAngle(newRotation.z, targetRotation.z, ref zVel, rotationSmoothTime)
                );
                transform.eulerAngles = newRotation;
            }
        }
        
        private void SetVerticalVelocity() {
            if (grounded)
                coyoteTimeLast = Time.time + coyoteTime;
            if (input.JumpReleased)
                jumpReleased = true;
            if (input.JumpPressed)
                jumpBufferTime = Time.time + jumpBuffering;
            if (grounded && verticalVelocity < 0f && slopeSlideVelocity.magnitude == 0f)
                verticalVelocity = 0f;
            
            SetJumpApexGravityScale();

            verticalVelocity += gravity * gravityScale * Time.deltaTime;

            if ((JumpBuffer() && grounded) || (input.JumpPressed && (grounded || CoyoteTime()))) {
                platform = null;
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpReleased = false;
            }

            bool JumpBuffer() { return Time.time <= jumpBufferTime; }
            bool CoyoteTime() { return Time.time <= coyoteTimeLast && characterVelocity.y < 0f; }
        }

        private void SetJumpApexGravityScale() {
            gravityScale = !grounded && !jumpReleased && (input.JumpHold && verticalVelocity > minApexVelocityThreshold) 
                ? 1f : fallingGravityScale;
            gravityScale = !grounded && !jumpReleased && input.JumpHold && 
                           (verticalVelocity > minApexVelocityThreshold && verticalVelocity < maxApexVelocityThreshold) 
                ? .25f : gravityScale;
        }

        private void FixedUpdate() {
            SetPlatformVelocity();
        }

        private void SetCharacterVelocity() {
            characterVelocity.x = horizontalVelocity.x;
            characterVelocity.y = verticalVelocity;
            characterVelocity.z = horizontalVelocity.z;
        }

        private void SetAnimationsParameters() {
            if (grounded) groundedTime = Time.time + UngroundedDelay;
            if (grounded && input.JumpPressed) animator.SetTrigger(IsJumping);
            
            animationGrounded = Time.time < groundedTime;
            animator.SetBool(IsGrounded, animationGrounded);
            //animator.SetBool(IsMoving, Mathf.Abs(Vector3.ProjectOnPlane(horizontalVelocity, Vector3.up).magnitude) > 0);
            animator.SetFloat(MoveZ, horizontalVelocity.magnitude);
        }
        
        private void OnControllerColliderHit(ControllerColliderHit hit) => PlatformCheck(hit);

        private RaycastHit sphereHitInfo;
        private RaycastHit rayHitInfo;
        private Vector3 position;
        private float slopeSlideMagnitude;
        private float rayAngle;
        private float angle;
        private void SetSlopeSlideVelocity() {
            slopeSlideVelocity -= slopeSlideVelocity * (Time.deltaTime * 3f);
            
            position = _transform.position;
            ray.origin = position + 1.01f * radius * Vector3.up;
            ray.direction = Vector3.down;

            Debug.DrawRay(position + Vector3.up, Vector3.down * 1.75f, Color.green);
            
            if (Physics.SphereCast(ray, characterController.radius, out sphereHitInfo, 1.02f * radius)) {
                rayAngle = float.MaxValue;
                if (Physics.Raycast(ray, out rayHitInfo, 1.75f)) {
                    rayAngle = Vector3.Angle(rayHitInfo.normal, Vector3.up);
                }
                angle = Mathf.Min(Vector3.Angle(sphereHitInfo.normal, Vector3.up), rayAngle);
                
                if (angle >= characterController.slopeLimit) {
                    slopeSlideVelocity =
                        Vector3.ProjectOnPlane(new Vector3(0f, -Mathf.Abs(verticalVelocity), 0f), sphereHitInfo.normal);
                    return;
                }
            }

            slopeSlideMagnitude = Vector3.ProjectOnPlane(slopeSlideVelocity, Vector3.up).magnitude;
            
            if (slopeSlideMagnitude == 0f || slopeSlideMagnitude > slopeSlideLimit)
                return;

            slopeSlideVelocity = Vector3.zero;
        }
        
        private void PlatformCheck(ControllerColliderHit hit) {
            ray.origin = _transform.position + Vector3.up * radius;
            ray.direction = Vector3.down;
            
            if (!Physics.SphereCast(ray, characterController.radius, .5f, platformLayer))
                platform = null;

            if (((1 << hit.gameObject.layer) & platformLayer.value) == 0 
                || platform == hit.transform
                || (characterController.collisionFlags & CollisionFlags.CollidedBelow) == 0) return;

            platform = hit.transform;
            lastPlatformPosition = platform.position;
            
            //Debug.Log($"collision {(hit.gameObject.layer)} {platformLayer.value} {((1 << hit.gameObject.layer) & platformLayer.value)}");
        }
        
        private void SetPlatformVelocity() {
            if (platform == null) return;
            
            Vector3 currPosition = platform.position;
            platformMovement = currPosition - lastPlatformPosition;
            lastPlatformPosition = currPosition;
        }

        [SerializeField] private float mass = 1f;
        private Vector3 externalForces;
        public void SetExternalForces(Vector3 velocity, ForceMode forceMode, bool invertOpposingForce = false) {
            float mass = forceMode == ForceMode.VelocityChange ? 1f : this.mass;
            if (invertOpposingForce) {
                verticalVelocity = Mathf.Abs(verticalVelocity);
            }
            verticalVelocity = velocity.y * (1f / mass);
        }
        
        [ContextMenu("RESET")]
        public void ResetPosition() {
            this.enabled = false;
            characterController.enabled = false;
            
            _transform.position = Vector3.zero;
            _transform.rotation = Quaternion.identity;
            
            this.enabled = true;
            characterController.enabled = true;
        }
    }
}
