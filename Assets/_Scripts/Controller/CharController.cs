using UnityEngine;

namespace Controller {
    [RequireComponent(typeof(CharacterController))]
    public class CharController : MonoBehaviour {
        private const float UngroundedDelay = 0.05f;
        private static readonly int MoveZ = Animator.StringToHash("MoveZ");
        private static readonly int IsJumping = Animator.StringToHash("Jump");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        [SerializeField] private Animator animator;
        [SerializeField] private LayerMask platformLayer;
        [Header("Controller Settings")]
        [SerializeField, Range(0f, 10f)] private float playerSpeed = 4.0f;
        [SerializeField, Range(0f, 10f)] private float jumpHeight = 1.5f;
        [SerializeField, Range(-100f, 0f)] private float gravity = -19.62f;
        [Header("Apex Modifiers")] 
        [SerializeField, Range(-2f, 0f)] private float minApexVelocityThreshold = -.5f;
        [SerializeField, Range(0f, 2f)] private float maxApexVelocityThreshold = .5f;
        [Header("Variable Jump Height")]
        [SerializeField, Range(0f, 10f)] private float fallingGravityScale = 10f;
        [Header("Jump Grace Times")]
        [SerializeField, Range(0f, 1f)] private float coyoteTime = .2f;
        [SerializeField, Range(0f, 1f)] private float jumpBuffering = .2f;
        
        [SerializeField] public Outfit[] outfits;
        
        private CharacterController characterController;
        private IInput input;
        private Vector3 characterVelocity;
        private Vector3 horizontalVelocity;
        private Vector3 slopeSlideVelocity;
        private float verticalVelocity;
        private float coyoteTimeTime;
        private float jumpBufferTime;
        private float gravityScale = 1f;
        private float groundedTime;
        private bool grounded;
        private bool animationGrounded;
        private Transform platform;
        private Vector3 lastPlatformPosition;
        private Vector3 platformVelocity;
        private Ray ray;
        private float radius;
        
        private void Awake() {
            characterController = GetComponent<CharacterController>();
            input = GetComponent<IInput>();
            radius = characterController.radius;
        }


        private void Update() {
            grounded = characterController.isGrounded;

            SetHorizontalVelocity();
            SetVerticalVelocity();
            SetSlopeVelocity();
            SetForwardDirection();
            SetCharacterVelocity();

            if (slopeSlideVelocity.magnitude < .2f) {
                AdjustVelocityToSlope(ref characterVelocity);
                characterController.Move(characterVelocity * Time.deltaTime + platformVelocity);
            } else {
                slopeSlideVelocity.y = verticalVelocity;
                characterController.Move(slopeSlideVelocity * Time.deltaTime);
            }
            
            platformVelocity = Vector3.zero;
            SetAnimationsParameters();
        }
        
        private void SetHorizontalVelocity() {
            horizontalVelocity.x = input.Axis.x * playerSpeed;
            horizontalVelocity.z = input.Axis.y * playerSpeed;
        }

        private void AdjustVelocityToSlope(ref Vector3 velocity) {
            
            if (grounded && Physics.Raycast(ray, out RaycastHit hitInfo, 1.75f)) {
                Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                velocity = slopeRotation * velocity;
                velocity.y = velocity.y < 0 ? velocity.y : 0f;
            }
        }

        private void SetForwardDirection() { 
            // TODO Smooth forward direction
            if (horizontalVelocity != Vector3.zero)
                transform.forward = horizontalVelocity;
        }
        
        private void SetVerticalVelocity() {
            if (grounded)
                coyoteTimeTime = Time.time + coyoteTime;
            if (input.JumpPressed)
                jumpBufferTime = Time.time + jumpBuffering;
            if (grounded && verticalVelocity < 0f && slopeSlideVelocity.magnitude == 0f)
                verticalVelocity = 0f;
            
            gravityScale = !grounded && (input.JumpHold && verticalVelocity > minApexVelocityThreshold) ? 1f : fallingGravityScale;
            gravityScale = !grounded && input.JumpHold && 
                           (verticalVelocity > minApexVelocityThreshold && verticalVelocity < maxApexVelocityThreshold) 
                    ? .25f : gravityScale;
            
            verticalVelocity += gravity * gravityScale * Time.deltaTime;

            if ((JumpBuffer() && grounded) || (input.JumpPressed && (grounded || CoyoteTime()))) {
                platform = null;
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            bool JumpBuffer() {
                return Time.time <= jumpBufferTime;
            }
            
            bool CoyoteTime() {
                return Time.time <= coyoteTimeTime;
            }
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
            animator.SetBool(IsMoving, Mathf.Abs(Vector3.ProjectOnPlane(horizontalVelocity, Vector3.up).magnitude) > 0);
            animator.SetFloat(MoveZ, horizontalVelocity.magnitude);
        }
        
        private void OnControllerColliderHit(ControllerColliderHit hit) {
            PlatformCheck(hit);
        }
        
        private void SetSlopeVelocity() {
            ray.origin = transform.position + 1.01f * radius * Vector3.up;
            ray.direction = Vector3.down;

            Debug.DrawRay(transform.position + Vector3.up, Vector3.down * 1.75f, Color.green);
            if (Physics.SphereCast(ray, characterController.radius, out RaycastHit sphereHitInfo, 1.02f * radius)) {
                float rayAngle = float.MaxValue;
                if (Physics.Raycast(ray, out RaycastHit rayHitInfo, 1.75f)) {
                    rayAngle = Vector3.Angle(rayHitInfo.normal, Vector3.up);
                }
                float angle = Mathf.Min(Vector3.Angle(sphereHitInfo.normal, Vector3.up), rayAngle);
                
                if (angle >= characterController.slopeLimit) {
                    slopeSlideVelocity =
                        Vector3.ProjectOnPlane(new Vector3(0f, verticalVelocity, 0f), sphereHitInfo.normal);
                    return;
                }
            }

            float slopeSlideMagnitude = Vector3.ProjectOnPlane(slopeSlideVelocity, Vector3.up).magnitude;
            if (slopeSlideMagnitude == 0f)
                return;

            slopeSlideVelocity -= slopeSlideVelocity * (Time.deltaTime * 3f);   // TODO Smoothdamp
            if (slopeSlideMagnitude > 5f)
                return;
            slopeSlideVelocity = Vector3.zero;
        }

        private void PlatformCheck(ControllerColliderHit hit) {
            ray.origin = transform.position + Vector3.up * radius;
            ray.direction = Vector3.down;
            if (!Physics.SphereCast(ray, characterController.radius, .5f, platformLayer))
                platform = null;
            if (((1 << hit.gameObject.layer) & platformLayer.value) == 0 
                || platform == hit.transform
                || (characterController.collisionFlags & CollisionFlags.CollidedBelow) == 0) return;

            platform = hit.transform;
            lastPlatformPosition = platform.position;
            
            Debug.Log($"collision {(hit.gameObject.layer)} {platformLayer.value} {((1 << hit.gameObject.layer) & platformLayer.value)}");
        }
        
        private void SetPlatformVelocity() {
            if (platform == null) return;
            
            Vector3 currPosition = platform.position;
            platformVelocity = currPosition - lastPlatformPosition;
            lastPlatformPosition = currPosition;
        }
    }
}
