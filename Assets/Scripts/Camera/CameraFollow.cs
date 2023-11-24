using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private bool debugView = true;

    [Header("References")]
    private Transform target;
    [SerializeField] private CharacterController characterController;
    
    [Header("Customizations")] [Space]
    [SerializeField] private Vector3 cameraOffset;
    
    [FormerlySerializedAs("lookAhead")]
    [Header("Lookahead")]
    [SerializeField] private Vector3 lookAheadBound;
    [SerializeField] private float lookAheadHeightOffset;
    [SerializeField, Range(0.01f, 1f)] private float smoothTimeLookahead = .36f;
    [SerializeField, Range(0f, 5f)] private float maxLookaheadLength = 2f;
    [SerializeField, Range(0f, 10f)] private float minCharacterVelocity = 1f;
    [SerializeField, Range(0f, 10f)] private float maxCharacterVelocity = 3f;
    
    [Header("Vertical Follow")]
    [SerializeField, Range(0.01f, 1f)] private float smoothTimeVertical = .1f;
    [SerializeField] private float minYClampPosition = -5f;
    
    // Lookahead
    private Vector3 centerPoint;
    private Vector3 currentLookahead; 
    private Vector3 targetLookahead;
    private Vector3 currentVelocityLookahead;
    private bool lookahead;
    // Vertical
    private float yTargetPos;
    private float currentYPos;
    private float currentVelocityVertical;
    
    private Vector3 cameraPosition;
    private Vector3 wallPush;
    private Vector3 lastPos;
    private bool locked;
    
    private void Awake() {
        target = characterController.transform;
        yTargetPos = target.position.y;
    }
    
    public void SetTarget(Transform target) => this.target = target;
    
    private void Start() => transform.position = target.position - cameraOffset;

    private void LateUpdate() {
        centerPoint = transform.position - cameraOffset;
        SetYTargetPos();
        Lookahead();
        if (locked) return;
        ClampPlayerWithinBoundingBox();
        Move();
    }

    private void Move() => transform.position = 
        new Vector3(cameraPosition.x, 
                    Mathf.Clamp(cameraPosition.y, minYClampPosition, float.MaxValue), 
                    cameraPosition.z) 
            + cameraOffset;

    private void SetYTargetPos() {
        float playerFeet = target.position.y;
        if (characterController.isGrounded)
            yTargetPos = playerFeet;
        else
            yTargetPos = Mathf.Min(playerFeet, yTargetPos);
        currentYPos = Mathf.SmoothDamp(currentYPos, yTargetPos, ref currentVelocityVertical, smoothTimeVertical);
    }

    private void Lookahead() {
        Vector3 charVel = Vector3.ProjectOnPlane(characterController.velocity, Vector3.up);
        float velocityFraction = Mathf.InverseLerp(minCharacterVelocity, maxCharacterVelocity, charVel.magnitude);
        Vector3 newPos = cameraPosition;
        Vector3 targetPos = target.position;
        targetPos.y = currentYPos;
        float rightBound = centerPoint.x + lookAheadBound.x * .5f;
        float leftBound = centerPoint.x - lookAheadBound.x * .5f;
        float forwardBound = centerPoint.z + lookAheadBound.z * .5f;
        float backBound = centerPoint.z - lookAheadBound.z * .5f;
        float upwardBound = centerPoint.y + lookAheadHeightOffset + lookAheadBound.y * .5f;
        float downwardBound = centerPoint.y + lookAheadHeightOffset - lookAheadBound.y * .5f;
            
        if (velocityFraction == 0) 
            lookahead = false;
        else if (targetPos.x > rightBound || targetPos.x < leftBound ||
                 targetPos.z > forwardBound || targetPos.z < backBound
                 //|| targetPos.y > upwardBound
                 || targetPos.y < downwardBound)
            lookahead = true;

        if (lookahead) {
            Vector3 lookaheadDirection = Vector3.ProjectOnPlane(charVel, Vector3.up);
            lookaheadDirection = Vector3.ClampMagnitude(lookaheadDirection, maxLookaheadLength);
            newPos = targetPos + lookaheadDirection;
            newPos.y = currentYPos;
        }
        cameraPosition = Vector3.SmoothDamp(cameraPosition, newPos, ref currentVelocityLookahead,
            smoothTimeLookahead);
    }

    private void ClampPlayerWithinBoundingBox() {
        Vector3 targetPos = target.position;
        
        float rightBound = centerPoint.x + lookAheadBound.x * .5f;
        float leftBound = centerPoint.x - lookAheadBound.x * .5f;
        float forwardBound = centerPoint.z + lookAheadBound.z * .5f;
        float backBound = centerPoint.z - lookAheadBound.z * .5f;
        float upwardBound = centerPoint.y + lookAheadHeightOffset + lookAheadBound.y * .5f;
        float downwardBound = centerPoint.y + lookAheadHeightOffset - lookAheadBound.y * .5f;
        
        if (targetPos.x > rightBound) cameraPosition += Vector3.right * (targetPos.x - rightBound);
        if (targetPos.x < leftBound) cameraPosition += Vector3.left * (leftBound - targetPos.x);
        if (targetPos.z > forwardBound) cameraPosition += Vector3.forward * (targetPos.z - forwardBound);
        if (targetPos.z < backBound) cameraPosition += Vector3.back * (backBound - targetPos.z);
        if (targetPos.y > upwardBound) cameraPosition += Vector3.up * (targetPos.y - upwardBound);
        if (targetPos.y < downwardBound) cameraPosition += Vector3.down * (downwardBound - targetPos.y);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (debugView)
            DrawLookahead();
    }

    private void DrawLookahead() {
        if (!Application.isPlaying)
            centerPoint = transform.position - cameraOffset;
        Handles.color = Color.black;
        Handles.zTest = CompareFunction.LessEqual;

        Handles.DrawWireCube(centerPoint + Vector3.up * lookAheadHeightOffset, lookAheadBound);
        
        Handles.zTest = CompareFunction.Always;

        float y = yTargetPos;
        Vector3[] dottedLines = new Vector3[] {
            new Vector3(centerPoint.x + lookAheadBound.x * .5f, y, centerPoint.z + lookAheadBound.z * .5f),
            new Vector3(centerPoint.x + lookAheadBound.x * .5f, y, centerPoint.z - lookAheadBound.z * .5f),
            new Vector3(centerPoint.x + lookAheadBound.x * .5f, y, centerPoint.z - lookAheadBound.z * .5f),
            new Vector3(centerPoint.x - lookAheadBound.x * .5f, y, centerPoint.z - lookAheadBound.z * .5f),
            new Vector3(centerPoint.x - lookAheadBound.x * .5f, y, centerPoint.z - lookAheadBound.z * .5f),
            new Vector3(centerPoint.x - lookAheadBound.x * .5f, y, centerPoint.z + lookAheadBound.z * .5f),
            new Vector3(centerPoint.x - lookAheadBound.x * .5f, y, centerPoint.z + lookAheadBound.z * .5f),
            new Vector3(centerPoint.x + lookAheadBound.x * .5f, y, centerPoint.z + lookAheadBound.z * .5f),
        };
        
        Handles.DrawDottedLines(dottedLines, 7f);   // Ground height
    }
#endif    
}
