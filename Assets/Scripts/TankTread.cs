using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class TankTread : MonoBehaviour {
    [SerializeField] private SplineContainer track;
    [SerializeField] private GameObject trackLink;
    [SerializeField] private GameObject tread;
    
    [SerializeField] private Transform[] driverWheels;
    [SerializeField] private Transform[] guideWheels;
    [SerializeField] private int numberOfLinks = 27;
    [SerializeField] private float driverWheelsVelocity = .1f;
    [SerializeField] private float guideWheelsVelocity = .1f;
    [SerializeField] private int trackRevolutionsPerRotation = 2;

    private CharacterController characterController;
    private Vector3[] driverWheelsRotations;
    private Vector3[] guideWheelsRotations;
    private List<GameObject> trackLinks = new List<GameObject>();
    private float progress;
    private float rotationalProgress;
    public float chiralityFactor = 1f;
    
    private void Awake() {
        characterController = GetComponentInParent<CharacterController>();
        chiralityFactor = -Mathf.Sign(tread.transform.localPosition.x);
        
        driverWheelsRotations = new Vector3[driverWheels.Length];
        guideWheelsRotations = new Vector3[guideWheels.Length];
        
        for (int i = 0; i < driverWheelsRotations.Length; i++)
            driverWheelsRotations[i] = driverWheels[i].eulerAngles;
        for (int i = 0; i < guideWheelsRotations.Length; i++)
            guideWheelsRotations[i] = guideWheels[i].eulerAngles;
        
        tread.SetActive(false);
        
        Transform treadParent = transform;
        for (int i = 0; i < numberOfLinks; i++) {
            trackLinks.Add(Instantiate(trackLink, treadParent.position, Quaternion.identity,
                treadParent));
        }
    }

    private void Update() {
        Progress();
        RotationProgress();
        RotateWheels(driverWheels, Vector3.right, new Vector3(0f, 0f, -90f), driverWheelsVelocity);
        RotateWheels(guideWheels, Vector3.up, new Vector3(180, 0f, 0f), guideWheelsVelocity);
        MoveAlongSpline();
    }

    private void Progress() {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;
        if (velocity.sqrMagnitude < 1f)
            return;
        
        progress += Time.deltaTime * Vector3.Dot(velocity, transform.forward) * .1f;
        progress = MathfMod(progress, 1f);
        if (progress < 0f)
            progress += 1f;
        debug = progress;
    }

    public float debugRot;
    private void RotationProgress() {
        float yRotation = transform.rotation.eulerAngles.y;
        float t = Mathf.InverseLerp(0f, 360f, yRotation);
        rotationalProgress = Mathf.Lerp(0f, trackRevolutionsPerRotation, t) * chiralityFactor;
        rotationalProgress = MathfMod(rotationalProgress, 1f);
        debugRot = rotationalProgress;
    }

    private void RotateWheels(Transform[] wheels, Vector3 rotationVector, Vector3 rotationOffset, float rotationSpeed) {
        float prog = progress + rotationalProgress;
        prog = MathfMod(prog, 1f);
        if (prog < 0f)
            prog += 1f;
        float t = Mathf.Lerp(0f, 360 * rotationSpeed, prog);
        for (int i = 0; i < wheels.Length; i++) {
            wheels[i].localEulerAngles = (rotationVector * t + rotationOffset);
        }
    }

    [Range(-2f, 2f)] public float debug;
    private void MoveAlongSpline() {
        int count = trackLinks.Count;
        float fraction = 1f / count;
        for (int i = 0; i < count; i++) {
            float localProgress = progress + rotationalProgress + fraction * i;
            if (localProgress < 0f)
                localProgress += 1f;
            localProgress = MathfMod(localProgress, 1f);
            if (i == 0)
                debug = localProgress;
            Transform t = trackLinks[i].transform;
            if (track.Evaluate(localProgress, out float3 position, out float3 forward, out float3 upwards)) {
                t.position = position;
                t.rotation = Quaternion.LookRotation(forward, upwards);
            }
        }
    }
    
    private static float MathfMod(float x, float mod) {
        return x % mod % mod;   // Returns correct remainder for negative numbers
    }
}
