using UnityEngine;

public class CameraFollowClose : MonoBehaviour {
	private const string MouseX = "Mouse X";
	private const string MouseY = "Mouse Y";
	private const float LookOffset = 90;
	
	private RaycastHit _hit;
	private Vector3 _smoothDampCurrentVelocity;
	private Vector3 _smoothDampCurrentVelocityLateral;
	private Vector3 _offsetLength;
	private Vector3 origin;
	private Vector3 _offsetTarget;
	private Vector3 cameraDirection;
	private Vector3 _smoothOffset;
	private Vector3 _cameraPos;
	private Vector2 _mouseMovement;
	private float _rotationX;
	private float _rotationY;
	private Camera _cam;
	
	[HideInInspector]
	public float clampLookupMax = 179;
	[HideInInspector]
	public float clampLookupMin = 12;
	[HideInInspector]
	public float smoothDampMinVal;
	[HideInInspector]
	public float smoothDampMaxVal;
	
	[SerializeField]
	private bool _firstPerson;

	[SerializeField] private Transform followTransform; 
	
	[SerializeField] 
	private LayerMask _collisionMask;
	
	[SerializeField]
	private Transform _camera;
	
	[SerializeField] [Range(1f, 10f)]
	private float mouseSensitivityX = 10f;
	
	[SerializeField] [Range(1f, 10f)]
	private float mouseSensitivityY = 5f;
		
	[SerializeField] [Range(0.0f, 2f)]
	private float _cameraCollisionRadius = .5f;

	[SerializeField] [Range(0.0f, 2f)]
	private float _headHeight = 1.6f;

	[SerializeField] 
	private Vector3 _camera3rdPersonOffset = new Vector3(.8f, 1f, -5f);

	[SerializeField] [Range(0f, 1f)] 
	private float _smoothCameraPosTime = 0.105f;
	
	private void Awake() {
		_cameraPos = transform.position;
		_cam = Camera.main;
	}

	private void Update() => Input();

	private void LateUpdate() => MoveCamera();

	private void Input() {
		_mouseMovement.x += UnityEngine.Input.GetAxisRaw(MouseX) * mouseSensitivityX;
		_mouseMovement.y -= UnityEngine.Input.GetAxisRaw(MouseY) * mouseSensitivityY;
		_mouseMovement.y = Mathf.Clamp(_mouseMovement.y, clampLookupMax - LookOffset, clampLookupMin - LookOffset);
	}

	private bool _debugHit;
	private void MoveCamera() {
		// Rotate Camera
		_camera.rotation = Quaternion.Euler(_mouseMovement.y, _mouseMovement.x, 0.0f);

		_cam.cullingMask = _firstPerson ? ~(1 << 1) : -1;	// Don't render the player if First Person
		
		// Lock camera to first person position
		if (_firstPerson) {
			_camera.position = followTransform.position;
			return;
		}

		// Smooth 
		_cameraPos = Vector3.SmoothDamp(_cameraPos, followTransform.position, ref _smoothDampCurrentVelocityLateral, _smoothCameraPosTime);
		
		origin = _cameraPos + _headHeight * Vector3.up;
		cameraDirection = _camera.rotation * _camera3rdPersonOffset;

		// Collision between intended camera position and player
		Physics.SphereCast(origin, 
			_cameraCollisionRadius, 
			cameraDirection.normalized, 
			out _hit, 
			cameraDirection.magnitude, 
			_collisionMask);
		
		// set target offset depending if hit
		_offsetLength = _hit.collider ? _camera3rdPersonOffset.normalized * _hit.distance : _camera3rdPersonOffset;
		
		// speed up smoothing if collision
		float _smoothDollyTime = _hit.collider ? smoothDampMinVal : smoothDampMaxVal;
		
		// interpolate towards end position
		_smoothOffset = Vector3.SmoothDamp(_smoothOffset, _offsetLength, ref _smoothDampCurrentVelocity, _smoothDollyTime);

		_camera.position = origin + _camera.rotation * _smoothOffset;

		_debugHit = _hit.collider;
	}

	private Camera mainCamera; 
	private void OnDrawGizmos() {
		mainCamera ??= Camera.main;
		Gizmos.color = _debugHit ? Color.red : Color.white;
		Gizmos.DrawWireSphere(_camera.position, _cameraCollisionRadius);
		
		Gizmos.color = Color.white;
		Gizmos.matrix = Matrix4x4.TRS(_camera.position, _camera.rotation, Vector3.one);
		Gizmos.DrawFrustum(Vector3.zero, mainCamera.fieldOfView, 12.0f, .3f, mainCamera.aspect);
	}
}