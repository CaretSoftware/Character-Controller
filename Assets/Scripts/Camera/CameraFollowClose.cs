using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowClose : MonoBehaviour {
	
	public delegate void CameraRotation(Quaternion cameraVector);
	public static CameraRotation cameraRotation;
	
	public delegate void CameraShakeEvent(float magnitude);
	public static CameraShakeEvent cameraShake;
	
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
	private Vector2 _mouseDeltaMovement;
	private Vector2 stickInput;
	private float _rotationX;
	private float _rotationY;
	private Camera _cam;
	private Quaternion currentCameraRotation;
	private bool _debugHit;

	[SerializeField, HideInInspector] private float clampLookupMax = 179;
	[SerializeField, HideInInspector] private float clampLookupMin = 12;
	[SerializeField, HideInInspector] private float smoothDampMinVal;
	[SerializeField, HideInInspector] private float smoothDampMaxVal;
	[SerializeField] private InputReader inputReader;
	[SerializeField] private bool _firstPerson;
	[SerializeField] private LayerMask playerLayer;
	[SerializeField] private LayerMask _collisionMask;
	[SerializeField] private Transform target; 
	[SerializeField] private Transform _camera;
	[SerializeField, Range(0f, 3f)] private float mouseSensitivityX = .1f;
	[SerializeField, Range(0f, 3f)] private float mouseSensitivityY = .05f;
	[SerializeField, Range(0f, 3f)] private float stickSensitivity = 1f;
	[SerializeField, Range(0.0f, 2f)] private float _cameraCollisionRadius = .5f;
	[SerializeField, Range(0f, 1f)] private float _smoothCameraPosTime = 0.105f;
	[SerializeField, Range(0.0f, 2f)] private float _headHeight = 1.6f;
	[SerializeField] private Vector3 _camera3rdPersonOffset = new Vector3(.8f, 1f, -5f);
	
	private void SetTarget(Transform target) => this.target = target;

	private void Awake() {
		_cameraPos = transform.position;
		_cam = Camera.main;
		inputReader.CameraMoveEvent += InputGamePad;
		inputReader.MouseMoveCameraEvent += InputMouse;
		cameraShake += ShakeCamera;
		CharacterSwapper.swapCameraTarget += SetTarget;
		//HideCursor();
	}

	//private void HideCursor() {
	//	Cursor.lockState = CursorLockMode.Locked;
	//	Cursor.visible = false;
	//}

	private void OnDestroy() {
		inputReader.CameraMoveEvent -= InputGamePad;
		inputReader.MouseMoveCameraEvent -= InputMouse;
		cameraShake -= ShakeCamera;
		CharacterSwapper.swapCameraTarget -= SetTarget;
	}

	private void Update() => InputGamePad();

	private void LateUpdate() {
		CameraShake();
		MoveCamera();
		currentCameraRotation = Quaternion.Inverse(Quaternion.Euler(0f, _mouseDeltaMovement.x, 0f));
		cameraRotation?.Invoke(currentCameraRotation);
	}

	private void InputGamePad() {
		_mouseDeltaMovement.x += stickInput.x * stickSensitivity;
		_mouseDeltaMovement.y -= stickInput.y * stickSensitivity;
		ClampCameraAngle();
	}

	private void InputGamePad(Vector2 input) {
		//HideCursor();
		stickInput = input;
	}
	
	private void InputMouse(Vector2 input) {
		const float inputConstant = 0.01f;
		_mouseDeltaMovement.x += inputConstant * mouseSensitivityX * input.x;
		_mouseDeltaMovement.y -= inputConstant * mouseSensitivityY * input.y;
		ClampCameraAngle();
	}
	
	private void ClampCameraAngle() => 
		_mouseDeltaMovement.y = Mathf.Clamp(_mouseDeltaMovement.y, clampLookupMax - LookOffset, clampLookupMin - LookOffset);

	private void MoveCamera() {
		Quaternion rot = _camera.rotation = Quaternion.Euler(_mouseDeltaMovement.y, _mouseDeltaMovement.x, 0.0f);
		_cam.cullingMask = _firstPerson ? ~playerLayer : -1;	// Don't render the player if First Person
		
		if (_firstPerson) {	// Lock camera to first person position
			_camera.position = target.position + _headHeight * Vector3.up;
			return;
		}

		// Lateral Smoothing
		_cameraPos = Vector3.SmoothDamp(_cameraPos, target.position, ref _smoothDampCurrentVelocityLateral, _smoothCameraPosTime);
		
		origin = _cameraPos + _headHeight * Vector3.up;
		cameraDirection = rot * _camera3rdPersonOffset;
		Physics.SphereCast(origin, // Collision between intended camera position and player
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

		_camera.SetPositionAndRotation(
				origin + _camera.rotation * _smoothOffset + cameraShakeOffset,
			
				cameraShakeRotation * rot);
		
		_debugHit = _hit.collider;
	}
	
	private void ShakeCamera(float magnitude) => trauma += magnitude;

	[Header("Camera Shake")]
	private float trauma;
	[SerializeField, Range(0f, 10f)] private float shakeSpeed = 1f;
	[SerializeField, Range(0f, 10f)] private float vibrationSpeed = 1f;
	[SerializeField, Range(0f, 10f)] private float cameraShakeFalloffSpeed = 1f;
	[SerializeField] private float rotationFactor = 1f;
	private Quaternion cameraShakeRotation;
	private Vector3 cameraShakeOffset;
	
	private void CameraShake() {
		// perlin within Range(-1, 1)
		float perlinNoiseX = (Mathf.PerlinNoise(0, Time.time * shakeSpeed) - .5f) * 2;
		float perlinNoiseY = (Mathf.PerlinNoise(.5f, Time.time * shakeSpeed) - .5f) * 2;
		// add perlin within Range(-.25, .25)
		perlinNoiseX += (Mathf.PerlinNoise(.25f, Time.time * vibrationSpeed) - .5f) * .5f;
		perlinNoiseY += (Mathf.PerlinNoise(.75f, Time.time * vibrationSpeed) - .5f) * .5f;

		// decrease trauma over time
		trauma = Mathf.Clamp(trauma -= Time.deltaTime * cameraShakeFalloffSpeed, 0.0f, 1.0f);
		
		float easedTrauma = Ease.InQuad(trauma);
		
		Gamepad.current?.SetMotorSpeeds(trauma, easedTrauma);

		cameraShakeOffset = 
			transform.rotation * 
			new Vector3(perlinNoiseX * easedTrauma, perlinNoiseY * easedTrauma, 0.0f);
		
		cameraShakeRotation = 
			Quaternion.Euler(
				perlinNoiseX * easedTrauma * rotationFactor, 
				perlinNoiseY * easedTrauma * rotationFactor, 
				Mathf.Lerp(perlinNoiseX, perlinNoiseY, .5f) * easedTrauma * rotationFactor);
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