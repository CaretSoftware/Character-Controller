using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Character {
    [RequireComponent(typeof(CharacterController))]
    public class ControllerPhysicsInteractions : MonoBehaviour {
    
        private CharacterController characterController;
        [SerializeField, Range(0f, 20f)] private float pushPower = 1f;
        [SerializeField, Range(0f, 180f)] private float angle = 45f;
    
        private void Awake() => characterController = GetComponent<CharacterController>();

        private void OnControllerColliderHit(ControllerColliderHit hit) {

            Rigidbody body = hit.collider.attachedRigidbody;

            if (body == null || body.isKinematic)
                return;

            float hitAngle = Vector3.Angle(Vector3.down, hit.moveDirection);

            angleHitColor = hitAngle < angle ? Color.red : Color.blue;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(characterController.velocity, Vector3.up);
            float characterVelocity = Mathf.Max(lateralVelocity.magnitude, .5f);

            if (hitAngle < angle) return;
        
            body.AddForceAtPosition(pushDir * pushPower * characterVelocity, transform.position, ForceMode.Force);
        }

        private Color angleHitColor = Color.blue;
    
#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Handles.color = angleHitColor;
            Handles.zTest = CompareFunction.LessEqual;
            DrawAngleCone();
            Handles.color = angleHitColor * new Color(1f, 1f, 1f, .25f);
            Handles.zTest = CompareFunction.Greater;
            DrawAngleCone();
        }

        private void DrawAngleCone() {
            const float coneHeight = 1f;
        
            Vector3 tipPosition = transform.position;
            Vector3 basePosition = tipPosition - Vector3.up * coneHeight;

            float halfAngle = angle * 0.5f;
            float angleRad = halfAngle * Mathf.Deg2Rad;
            float baseRadius = Mathf.Tan(angleRad) * coneHeight;
        
            Handles.DrawWireDisc(basePosition, Vector3.up, baseRadius);

            Vector3 rgtBasePos = basePosition + new Vector3(XPos(0.0f), 0, ZPos(0.0f));
            Vector3 fwdBasePos = basePosition + new Vector3(XPos(0.5f), 0, ZPos(0.5f));
            Vector3 lftBasePos = basePosition + new Vector3(XPos(1.0f), 0, ZPos(1.0f));
            Vector3 bckBasePos = basePosition + new Vector3(XPos(1.5f), 0, ZPos(1.5f));
        
            Vector3 directionToRightBase = rgtBasePos - tipPosition;
            Vector3 directionToForwardBase = fwdBasePos - tipPosition;

            Handles.DrawWireArc(tipPosition, Vector3.back, directionToRightBase, angle, directionToRightBase.magnitude);
            Handles.DrawWireArc(tipPosition, Vector3.right, directionToForwardBase, angle, directionToForwardBase.magnitude);

            Handles.DrawLine(tipPosition, tipPosition + Vector3.down * directionToForwardBase.magnitude);
        
            Handles.DrawLine(tipPosition, fwdBasePos);
            Handles.DrawLine(tipPosition, rgtBasePos);
            Handles.DrawLine(tipPosition, lftBasePos);
            Handles.DrawLine(tipPosition, bckBasePos);

            float XPos(float rad) => Mathf.Cos(Mathf.PI * rad) * baseRadius;
            float ZPos(float rad) => Mathf.Sin(Mathf.PI * rad) * baseRadius;
        }
#endif
    }
}
