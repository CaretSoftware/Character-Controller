#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
#endif
using Unity.Mathematics;
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

	[SerializeField] private float clampLookupMax = 179;
	[SerializeField] private float clampLookupMin = 12;
	[SerializeField] private float smoothDampMinVal;
	[SerializeField] private float smoothDampMaxVal;
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
	}

	private void Start() => centerPoint = transform.position;

	private void OnDestroy() {
		inputReader.CameraMoveEvent -= InputGamePad;
		inputReader.MouseMoveCameraEvent -= InputMouse;
		cameraShake -= ShakeCamera;
		CharacterSwapper.swapCameraTarget -= SetTarget;
	}

	private void Update() => AddGamePadInput();

	private void LateUpdate() {
		CameraShake();
		MoveCamera();
		
		SetGroundedHeight(); // TODO
		Lookahead();
		
		currentCameraRotation = Quaternion.Inverse(Quaternion.Euler(0f, _mouseDeltaMovement.x, 0f));
		cameraRotation?.Invoke(currentCameraRotation);
	}

	private void AddGamePadInput() {
		_mouseDeltaMovement.x += stickInput.x * stickSensitivity;
		_mouseDeltaMovement.y -= stickInput.y * stickSensitivity;
		ClampCameraAngle();
	}

	private void InputGamePad(Vector2 input) => stickInput = input;

	private void InputMouse(Vector2 input) {
		const float inputConstant = 0.01f;
		_mouseDeltaMovement.x += inputConstant * mouseSensitivityX * input.x;
		_mouseDeltaMovement.y -= inputConstant * mouseSensitivityY * input.y;
		ClampCameraAngle();
	}
	
	private void ClampCameraAngle() => 
		_mouseDeltaMovement.y = Mathf.Clamp(_mouseDeltaMovement.y, 
			clampLookupMax - LookOffset, clampLookupMin - LookOffset);

	private Vector3 debug;
	private Quaternion lookUpRotation;  
	[SerializeField] private float lookUpRotationOffset = -20f;  
	[SerializeField] private float minLimitLookUp = -1f;  
	[SerializeField] private float maxLimitLookUp = 1f;  

	private void MoveCamera() {
		Quaternion rot = _camera.rotation = Quaternion.Euler(_mouseDeltaMovement.y, _mouseDeltaMovement.x, 0.0f);
		_cam.cullingMask = _firstPerson ? ~playerLayer : -1;	// Don't render the player if First Person
		Vector3 targetPosition = target.position;
		if (_firstPerson) {
			_camera.position = targetPosition + _headHeight * Vector3.up;
			return;
		}
		
		// Lateral Smoothing
		_cameraPos = Vector3.SmoothDamp(_camera.position, centerPoint, ref _smoothDampCurrentVelocityLateral, _smoothCameraPosTime);

		float distance = Vector3.Distance(_cameraPos, targetPosition);
		float t = Mathf.InverseLerp(1f,-_camera3rdPersonOffset.z, distance);
		// _cameraPos = Vector3.Lerp(targetPosition, _cameraPos, t);
		
		debug = origin = _cameraPos + _headHeight * Vector3.up;
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
		
		// interpolate towards final position
		_smoothOffset = Vector3.SmoothDamp(_smoothOffset, _offsetLength, ref _smoothDampCurrentVelocity, _smoothDollyTime);
		
		float upLook = Vector3.Dot(Vector3.up, cameraDirection.normalized);
		float a = Mathf.InverseLerp(minLimitLookUp, maxLimitLookUp, upLook);
		lookUpRotation = Quaternion.Lerp(quaternion.Euler(new float3(lookUpRotationOffset, 0f, 0f)), Quaternion.identity, a); 

		_camera.SetPositionAndRotation(
				origin + _camera.rotation * _smoothOffset + cameraShakeOffset,
			
				cameraShakeRotation * rot * lookUpRotation);
		
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

	private void SetGroundedHeight() {
		float playerFeet = target.position.y;
		if (characterController.isGrounded)
			yTargetHeight = playerFeet;
		else
			yTargetHeight = Mathf.Min(playerFeet, yTargetHeight);
		currentYHeight = Mathf.SmoothDamp(currentYHeight, yTargetHeight, ref currentVelocityVertical, smoothTimeVertical);
	}

	[SerializeField] private CharacterController characterController;
	private bool lookahead;
	private float yTargetHeight;
	private float currentYHeight;
	// Smoothing
	[SerializeField]private float smoothTimeVertical;
	private float currentVelocityVertical;
	private Vector3 currentVelocityLookahead;
	[SerializeField] private float smoothTimeLookahead;

	private Vector3 centerPoint;
	[SerializeField]private float minCharacterVelocity;
	[SerializeField]private float maxCharacterVelocity;

	[SerializeField] private Vector3 lookAheadBound;
	[SerializeField] private float maxLookaheadLength;

	private void Lookahead() {
		Vector3 newPos = centerPoint;
		Vector3 targetPos = target.position;
		targetPos.y = currentYHeight;
		Vector3 localPoint = (targetPos - centerPoint);
		Vector3 charVel = Vector3.ProjectOnPlane(characterController.velocity, Vector3.up);
		Vector3 eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
		Quaternion camRot = Quaternion.Euler(eulerAngles);
		float velocityFraction = Mathf.InverseLerp(minCharacterVelocity, maxCharacterVelocity, charVel.magnitude);

		Vector3 inverseTransformPoint = InverseTransformPoint(Vector3.zero, camRot, Vector3.one,localPoint);

		if (velocityFraction == 0)
			lookahead = false;
		else if (Mathf.Abs(inverseTransformPoint.x) > lookAheadBound.x * .5f ||
		         Mathf.Abs(inverseTransformPoint.y) > lookAheadBound.y * .5f ||
		         Mathf.Abs(inverseTransformPoint.z) > lookAheadBound.z * .5f)
			lookahead = true;

		if (lookahead) {
			Vector3 lookaheadDirection = Vector3.ProjectOnPlane(charVel, Vector3.up);
			lookaheadDirection = Vector3.ClampMagnitude(lookaheadDirection, maxLookaheadLength);
			newPos = targetPos + lookaheadDirection;
			newPos.y = currentYHeight;
		}
		centerPoint = Vector3.SmoothDamp(centerPoint, newPos, ref currentVelocityLookahead,
			smoothTimeLookahead);

		centerPoint.y = Mathf.Clamp(targetPos.y, centerPoint.y - lookAheadBound.y * .5f, centerPoint.y + lookAheadBound.y * .5f);

		Vector3 InverseTransformPoint(Vector3 transformPos, Quaternion transformRotation, Vector3 transformScale, Vector3 pos) {
			Matrix4x4 matrix = Matrix4x4.TRS(transformPos, transformRotation, transformScale);
			Matrix4x4 inverse = matrix.inverse;
			return inverse.MultiplyPoint3x4(pos);
		}
	}

#if UNITY_EDITOR
	private Camera mainCamera;
	private void OnDrawGizmos() {
		mainCamera ??= Camera.main;
		if (mainCamera == null) return;
		
		Gizmos.color = _debugHit ? Color.red : Color.white;
		Gizmos.DrawWireSphere(_camera.position, _cameraCollisionRadius);
		Gizmos.DrawWireSphere(debug, _cameraCollisionRadius);
		
		DrawLookahead();
		
		Gizmos.color = Color.white;
		Gizmos.matrix = Matrix4x4.TRS(_camera.position, _camera.rotation, Vector3.one);
		Gizmos.DrawFrustum(Vector3.zero, mainCamera.fieldOfView, 12.0f, .3f, mainCamera.aspect);
	}
	
	private void DrawLookahead() {
		if (!Application.isPlaying)
			centerPoint = transform.position - _camera3rdPersonOffset;

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(centerPoint, .2f);
		
		Handles.color = Color.black;
		Handles.zTest = CompareFunction.LessEqual;
		
		Vector3 cameraRot = transform.rotation.eulerAngles;
		cameraRot.x = 0f;
		cameraRot.z = 0f;
		Quaternion camRot = Quaternion.Euler(cameraRot);
		Handles.matrix = Matrix4x4.TRS(centerPoint, camRot, Vector3.one);

		Handles.DrawWireCube(Vector3.zero + lookAheadBound.y * .5f * Vector3.up, lookAheadBound);
        
		Handles.zTest = CompareFunction.Always;
		
		float y = yTargetHeight - centerPoint.y;
		Vector3[] dottedLines = new Vector3[] {
			new Vector3(+ lookAheadBound.x * .5f, y, + lookAheadBound.z * .5f),
			new Vector3(+ lookAheadBound.x * .5f, y, - lookAheadBound.z * .5f),
			new Vector3(+ lookAheadBound.x * .5f, y, - lookAheadBound.z * .5f),
			new Vector3(- lookAheadBound.x * .5f, y, - lookAheadBound.z * .5f),
			new Vector3(- lookAheadBound.x * .5f, y, - lookAheadBound.z * .5f),
			new Vector3(- lookAheadBound.x * .5f, y, + lookAheadBound.z * .5f),
			new Vector3(- lookAheadBound.x * .5f, y, + lookAheadBound.z * .5f),
			new Vector3(+ lookAheadBound.x * .5f, y, + lookAheadBound.z * .5f),
		};
        
		Handles.DrawDottedLines(dottedLines, 7f);   // Draw Grounded Height
	}
#endif
}