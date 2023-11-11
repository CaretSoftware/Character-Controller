using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TankPhysicsEffector : MonoBehaviour {
    [SerializeField] private Rigidbody turretRigidBody;
    [SerializeField] private Rigidbody[] rigidBodies;
    [SerializeField] private float force = 180f;
    [SerializeField] private float detailsForce;
    private CharacterController characterController;
    private Transform _transform;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        _transform = transform;
    }

    private void FixedUpdate() {
        Vector3 forward = _transform.forward;
        Vector3 characterVelocity = characterController.velocity;
        
        float dotProduct = Vector3.Dot(characterVelocity, forward);
        
        turretRigidBody.AddForce(Time.deltaTime * dotProduct * force * -forward, ForceMode.Acceleration);
        
        for (int i = 0; i < rigidBodies.Length; i++) {
            rigidBodies[i].AddForce(Time.deltaTime * dotProduct * force * -forward, ForceMode.Acceleration);
        }
    }
}
