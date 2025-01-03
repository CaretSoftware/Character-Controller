﻿#if UNITY_EDITOR
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
	private Vector3 _smoothObstructionOffset;
	//private Vector3 _cameraPos;
	private Vector2 _mouseDeltaMovement;
	private Vector2 stickInput;
	private float _rotationX;
	private float _rotationY;
	private Camera _cam;
	private Quaternion currentCameraRotation;

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
	private Collider thisCollider;
	private void SetTarget(Transform target) {
		this.target = target;
		characterController = target.GetComponent<CharacterController>();
	}

	private void Awake() {
		//_cameraPos = 
			centerPoint =
					transform.position;
		
		_cam = Camera.main;
		inputReader.CameraMoveEvent += InputGamePad;
		inputReader.MouseMoveCameraEvent += InputMouse;
		cameraShake += ShakeCamera;
		CharacterSwapper.swapCameraTarget += SetTarget;
		thisCollider = GetComponent<Collider>();
	}

	private void OnDestroy() {
		inputReader.CameraMoveEvent -= InputGamePad;
		inputReader.MouseMoveCameraEvent -= InputMouse;
		cameraShake -= ShakeCamera;
		CharacterSwapper.swapCameraTarget -= SetTarget;
	}

	private void Update() => AddGamePadInput();

	private void LateUpdate() {
		SetGroundedHeight();
		Lookahead();
		
		CameraShake();
		MoveCamera();
		
		currentCameraRotation = Quaternion.Inverse(Quaternion.Euler(0f, _mouseDeltaMovement.x, 0f));
		cameraRotation?.Invoke(currentCameraRotation);
	}

	private void AddGamePadInput() {
		if (target == fixedViewCharacter) return;
		_mouseDeltaMovement.x += stickInput.x * stickSensitivity;
		_mouseDeltaMovement.y -= stickInput.y * stickSensitivity;
		ClampCameraAngle();
	}

	private void InputGamePad(Vector2 input) {
		if (target == fixedViewCharacter) return;
		stickInput = input;
	}

	private void InputMouse(Vector2 input) {
		if (target == fixedViewCharacter) return;
		const float inputConstant = 0.01f;
		_mouseDeltaMovement.x += inputConstant * mouseSensitivityX * input.x;
		_mouseDeltaMovement.y -= inputConstant * mouseSensitivityY * input.y;
		ClampCameraAngle();
	}
	
	private void ClampCameraAngle() => 
		_mouseDeltaMovement.y = Mathf.Clamp(_mouseDeltaMovement.y, 
			clampLookupMax - LookOffset, clampLookupMin - LookOffset);

	[SerializeField] private float lookUpRotationOffset = -19.2f;  
	private float minLimitLookUp = -1f;  
	private float maxLimitLookUp = 1f;
	private RaycastHit lineOfSightHit;
	private RaycastHit minHeightHit;
	private Vector3 currentCameraCenter;
	Collider[] colliders = new Collider[10];
	[SerializeField] private Transform fixedViewCharacter;
	[SerializeField] private float fixedViewSpeed = 10f;
	[SerializeField] private float fixedViewAngle = 45f;
	private bool done;
	private void MoveCamera() {
		if (target == fixedViewCharacter)
			FixedView();
		else
			done = false;

		Quaternion rot = _camera.rotation = Quaternion.Euler(_mouseDeltaMovement.y, _mouseDeltaMovement.x, 0.0f);
		_cam.cullingMask = _firstPerson ? ~playerLayer : -1;	// Don't render the player if First Person
		Vector3 targetPosition = target.position;
		if (_firstPerson) {
			_camera.position = targetPosition + _headHeight * Vector3.up;
			return;
		}
		
		int collisionCount;
		int exit = 0;
		if (!obstructionCollision)
			do {
				collisionCount = Physics.OverlapSphereNonAlloc(centerPoint, _cameraCollisionRadius, colliders, _collisionMask,
					QueryTriggerInteraction.Ignore);
				for (int i = 0; i < collisionCount; i++) {
					if (Physics.ComputePenetration(
						    thisCollider,
						    centerPoint,
						    Quaternion.identity, 
						    colliders[i], 
						    colliders[i].gameObject.transform.position, 
						    colliders[i].gameObject.transform.rotation,
						    out var direction,
						    out var distance)) {

						Vector3 separationVector = direction * distance;
						centerPoint += separationVector + separationVector.normalized * .01f;
					}
				}
			} while (collisionCount > 0 && ++exit <= 10);
		
		origin = centerPoint + _headHeight * Vector3.up;
		cameraDirection = rot * _camera3rdPersonOffset;

		Physics.SphereCast(origin, // Dolly camera towards player to avoid obstruction
				_cameraCollisionRadius, 
				cameraDirection.normalized, 
				out _hit, 
				cameraDirection.magnitude, 
				_collisionMask);

		Vector3 losOrigin = targetPosition + _headHeight * Vector3.up;
		Vector3 lineOfSightDirection = _camera.position - losOrigin;
		Physics.SphereCast(losOrigin, // Center camera on player if about to loose line of sight
				_cameraCollisionRadius, 
				lineOfSightDirection.normalized, 
				out lineOfSightHit, 
				lineOfSightDirection.magnitude, 
				_collisionMask);

		//centerPointCollision = centerPointHit.collider;
		obstructionCollision = lineOfSightHit.collider || _hit.collider;

		// set target offset depending if hit
		_offsetLength = _hit.collider ? _camera3rdPersonOffset.normalized * _hit.distance : _camera3rdPersonOffset;
		
		// speed up smoothing if collision
		float _smoothDollyTime = _hit.collider ? smoothDampMinVal : smoothDampMaxVal;
		
		// interpolate towards final position
		_smoothObstructionOffset = Vector3.SmoothDamp(_smoothObstructionOffset, _offsetLength, ref _smoothDampCurrentVelocity, _smoothDollyTime);
		
		float upLook = Vector3.Dot(Vector3.up, cameraDirection.normalized);
		float a = Mathf.InverseLerp(minLimitLookUp, maxLimitLookUp, upLook);
		Quaternion lookUpRotation = Quaternion.Lerp(quaternion.Euler(new float3(lookUpRotationOffset, 0f, 0f)), Quaternion.identity, a); 
		
		_camera.SetPositionAndRotation(
				origin + _camera.rotation * _smoothObstructionOffset + cameraShakeOffset,
				cameraShakeRotation * rot * lookUpRotation);
	}

	private void FixedView() {
		if (done) return;
			
		float movementSpeed = fixedViewSpeed * Time.deltaTime;
		float negativeModulo = NegativeModulo(_mouseDeltaMovement.x, 360f);
			
		if (_mouseDeltaMovement.y < fixedViewAngle)
			_mouseDeltaMovement.y += Mathf.Min((180f + fixedViewAngle) - (180f + _mouseDeltaMovement.y), movementSpeed);
			
		if (negativeModulo <= movementSpeed) {
			_mouseDeltaMovement.x = 0f;
			if (_mouseDeltaMovement.y >= fixedViewAngle - 0.001f)
				done = true;
			return;
		}

		float mov = NegativeModulo(_mouseDeltaMovement.x, 360f) < 180 ? -movementSpeed: movementSpeed;
		_mouseDeltaMovement.x += mov;
		
		float NegativeModulo(float x, float mod) => (x % mod + mod) % mod;
	}
	
	private void ShakeCamera(float magnitude) => trauma += magnitude;

	[Space, Header("Camera Shake")]
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
		if (characterController == null || characterController.isGrounded || obstructionCollision)
			yTargetHeight = playerFeet;
		else
			yTargetHeight = Mathf.Min(playerFeet, yTargetHeight);
		currentYHeight = Mathf.SmoothDamp(currentYHeight, yTargetHeight, ref currentVelocityVertical, smoothTimeVertical);
	}

	[SerializeField] private CharacterController characterController;
	private bool lookahead;
	private bool obstructionCollision;
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

	private Vector3 nonCharControllerPos; 
	private Vector3 nonCharControllerVel; 
	private void Lookahead() {
		Vector3 newPos = centerPoint;
		Vector3 targetPos = target.position;
		targetPos.y = currentYHeight;
		Vector3 localPoint = (targetPos - centerPoint);

		if (characterController == null) {
			nonCharControllerVel = (target.position - nonCharControllerPos) * (1f / Time.deltaTime);
			nonCharControllerPos = target.position;
		}
		
		Vector3 charVel = characterController != null 
				? Vector3.ProjectOnPlane(characterController.velocity, Vector3.up) 
				: nonCharControllerVel;
		
		Vector3 eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
		Quaternion camRot = Quaternion.Euler(eulerAngles);
		float velocityFraction = Mathf.InverseLerp(minCharacterVelocity, maxCharacterVelocity, charVel.magnitude);

		Vector3 inverseTransformPoint = InverseTransformPoint(Vector3.zero, camRot, Vector3.one,localPoint);

		if (velocityFraction == 0)
			lookahead = false;
		if (Mathf.Abs(inverseTransformPoint.x) > lookAheadBound.x * .5f ||
		    Mathf.Abs(inverseTransformPoint.y) > lookAheadBound.y * .5f ||
		    Mathf.Abs(inverseTransformPoint.z) > lookAheadBound.z * .5f)
			lookahead = true;

		if (lookahead || obstructionCollision) {
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
		
		Gizmos.color = obstructionCollision ? Color.red : Color.white;
		Gizmos.DrawWireSphere(_camera.position, _cameraCollisionRadius);
		
		Gizmos.DrawWireSphere(origin, _cameraCollisionRadius);
		
		DrawLookahead();
		
		Gizmos.color = Color.white;
		Gizmos.matrix = Matrix4x4.TRS(_camera.position, _camera.rotation, Vector3.one);
		Gizmos.DrawFrustum(Vector3.zero, mainCamera.fieldOfView, 12.0f, .3f, mainCamera.aspect);
	}
	
	private void DrawLookahead() {
		if (!Application.isPlaying)
			centerPoint = transform.position - _camera3rdPersonOffset;

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