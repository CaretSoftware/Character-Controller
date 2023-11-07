using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Tank/Movement")]
public class TankMovement : CharacterState {
    [SerializeField] private float forwardVelocity = 2f;
    [SerializeField] private float rotationVelocity = 30f;
    
    public override void Enter() {
        base.Enter();
    }

    private float dampedVelocity;
    private float dampedRotationVelocity;
    private float currentVelocity;
    private float currentRotationVelocity;
    [SerializeField] private float smoothAcceleration = .2f;
    [SerializeField] private float smoothDeceleration = .05f;
    [SerializeField] private float smoothRotationAcceleration = .5f;
    
    public override void Update() {
        Vector2 inputAxis = input.Axis;
        if (inputAxis == Vector2.zero) {
            characterController.Move(Vector3.zero);
            return;
        }
        
        Vector3 forward = myTransform.forward;
        Vector3 right = myTransform.right;
        Vector2 tankForward = new Vector2(forward.x, forward.z).normalized;
        Vector2 tankRight = new Vector2(right.x, right.z).normalized;
        float inputMagnitude = inputAxis.magnitude;
        
        float direction = Mathf.Sign(Vector2.Dot(inputAxis, tankRight));

        float dotProduct = Vector2.Dot(inputAxis, tankForward);

        if (dotProduct >= .5f && inputMagnitude > .2f) {
            dampedVelocity = Mathf.SmoothDamp(dampedVelocity, inputMagnitude * forwardVelocity, 
                ref currentVelocity, smoothAcceleration);
        }

        if (dotProduct < .5f || inputMagnitude < .2f) {
            dampedVelocity = Mathf.SmoothDamp(dampedVelocity, 0f, ref currentVelocity, smoothDeceleration);
        }
        
        characterController.Move(Time.deltaTime * dampedVelocity * forward);
        
        if (inputMagnitude >= .1f && dotProduct < .95f) {
            dampedRotationVelocity =
                    Mathf.SmoothDamp(dampedRotationVelocity, rotationVelocity, ref currentRotationVelocity, 
                            smoothRotationAcceleration);
        } else {
            dampedRotationVelocity =
                    Mathf.SmoothDamp(dampedRotationVelocity, 0f, ref currentRotationVelocity, 
                            smoothRotationAcceleration);
        }
        
        //if (dotProduct < .98f)
        Rotate(direction, inputMagnitude);

        DebugScript.debug = dotProduct;
    }

    private void Rotate(float direction, float inputMagnitude) {
        myTransform.Rotate(Vector3.up, direction * inputMagnitude * dampedRotationVelocity * Time.deltaTime);
    }
    
    public override void Exit() {
        base.Exit();
    }
}
