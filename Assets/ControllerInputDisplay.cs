using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class ControllerInputDisplay : MonoBehaviour {
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform aButton;
    [SerializeField] private Transform rightTrigger;
    [SerializeField] private Transform analogStickLeft;
    [SerializeField] private Transform analogStickRight;
    [SerializeField] private Vector3 triggerRotation;
    [SerializeField] private float analogStickSmoothing = .15f;
    
    private Vector3 controllerForward = new Vector3(.1f, -.1f, 0f);
    private Vector3 aButtonLocalPosition;
    private Quaternion triggerDefaultRotation;
    private Quaternion triggerHeldRotation;

    // Analog sticks
    private const float StickLimit = -.4f;
    private Quaternion analogStickRotation;
    private Vector3 axisSmoothLeft;
    private Vector3 axisSmoothRight;
    private Vector3 currentVelocityLeftStick;
    private Vector3 currentVelocityRightStick;
    private Vector3 axisTargetLeft;
    private Vector3 axisTargetRight;
    
    private void Awake() {
        inputReader.JumpEvent += AButtonHeld;
        inputReader.JumpCancelledEvent += AButtonReleased;
        inputReader.TriggerEvent += TriggerRight;
        inputReader.MoveEvent += LeftAnalogStick;
        inputReader.CameraMoveEvent += RightAnalogStick;
        
        aButtonLocalPosition = aButton.localPosition;
        triggerDefaultRotation = rightTrigger.rotation;
        triggerHeldRotation = triggerDefaultRotation * quaternion.Euler(triggerRotation);
        analogStickRotation = analogStickLeft.rotation;
    }

    private void OnDestroy() {
        inputReader.JumpEvent -= AButtonHeld;
        inputReader.JumpCancelledEvent -= AButtonReleased;
        inputReader.TriggerEvent -= TriggerRight;
        inputReader.MoveEvent -= LeftAnalogStick;
        inputReader.CameraMoveEvent -= RightAnalogStick;
    }

    private void AButtonHeld() => AButton(true);
    
    private void AButtonReleased() => AButton(false);
    
    private void AButton(bool held) =>
            aButton.localPosition = held ? aButtonLocalPosition + controllerForward : aButtonLocalPosition;

    private void TriggerRight(float value) => 
            rightTrigger.rotation = Quaternion.Lerp(triggerDefaultRotation, triggerHeldRotation, value);

    private void Update() {
        axisSmoothLeft = Vector3.SmoothDamp(axisSmoothLeft, axisTargetLeft, ref currentVelocityLeftStick, analogStickSmoothing);
        axisSmoothRight = Vector3.SmoothDamp(axisSmoothRight, axisTargetRight, ref currentVelocityRightStick, analogStickSmoothing);
        analogStickLeft.rotation =
                analogStickRotation * quaternion.Euler(axisSmoothLeft);
        analogStickRight.rotation =
                analogStickRotation * quaternion.Euler(axisSmoothRight);
    }

    private void LeftAnalogStick(Vector2 axis) => axisTargetLeft = new Vector3(axis.x, 0f, axis.y) * StickLimit;

    private void RightAnalogStick(Vector2 axis) => axisTargetRight = new Vector3(axis.x, 0f, axis.y) * StickLimit;
}
