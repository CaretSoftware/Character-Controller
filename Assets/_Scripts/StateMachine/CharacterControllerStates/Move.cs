using System;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Character/Move")]
public class Move : Grounded {
    [HideInInspector] public float CharacterMaxSpeed => characterMaxSpeed;
    [SerializeField] private float characterMaxSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float groundSmoothTime = .1f;
    private Ray ray;
    
    public override void Enter() { }

    private void OnValidate() {
        if (characterSm != null && characterSm.MaxVelocity != characterMaxSpeed)
            setMaxSpeed?.Invoke(characterMaxSpeed);
    }

    public override void Update() {
        Vector3 horizontalVelocity = SetHorizontalVelocity(characterSm.HorizontalVelocity);
        AdjustVelocityToSlope(ref horizontalVelocity);
        setHorizontalVelocity?.Invoke(horizontalVelocity);
        characterController.Move((horizontalVelocity + characterSm.VerticalVelocity) * Time.deltaTime);
        base.Update();
    }

    private float xCurrentVelocity;
    private float yCurrentVelocity;
    private Vector2 smoothInput;
    private Vector3 SetHorizontalVelocity(Vector3 horizontalVelocity) {
        float currSmoothX = input.Axis.x != 0 ? 0f : groundSmoothTime;
        float currSmoothY = input.Axis.y != 0 ? 0f : groundSmoothTime;
        smoothInput.x = Mathf.SmoothDamp(smoothInput.x, input.Axis.x, ref xCurrentVelocity, currSmoothX);
        smoothInput.y = Mathf.SmoothDamp(smoothInput.y, input.Axis.y, ref yCurrentVelocity, currSmoothY);
        
        horizontalVelocity.x = Mathf.Abs(smoothInput.x) > .1f ? smoothInput.x * characterMaxSpeed : 0f;
        horizontalVelocity.z = Mathf.Abs(smoothInput.y) > .1f ? smoothInput.y * characterMaxSpeed : 0f;
        return horizontalVelocity;
    }
    
    private Vector3 horizontal;
    private RaycastHit slopeHitInfo;
    private Quaternion slopeRotation;
    private void AdjustVelocityToSlope(ref Vector3 velocity) {
        horizontal = velocity;
        horizontal.y = 0f;
        ray.origin = myTransform.position + characterController.radius * Vector3.up;
        ray.direction = Vector3.down;

        float maxDistance = (characterController.radius + characterController.skinWidth) / Mathf.Cos(characterController.slopeLimit * Mathf.Deg2Rad);
        
        if (Physics.Raycast(ray, out slopeHitInfo, maxDistance)){
            slopeRotation = Quaternion.FromToRotation(Vector3.up, slopeHitInfo.normal);
            velocity = slopeRotation * horizontal;
        }
        Debug.DrawRay(myTransform.position + Vector3.up * characterController.radius, Vector3.down * maxDistance);
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() { }
}
