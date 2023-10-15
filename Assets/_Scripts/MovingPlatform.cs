using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour {
    [SerializeField] private List<Vector3> positions = new List<Vector3>();
    private Rigidbody rb;
    private int currentGoalPosition;
    [SerializeField, Range(0f, 10f)] private float speed = 1f;
    
    private void Awake() => rb = GetComponent<Rigidbody>();

    private void FixedUpdate() {
        Vector3 direction = positions[currentGoalPosition] - transform.position;
        float dist = direction.magnitude;
        if (dist < .1f)
            currentGoalPosition = ++currentGoalPosition % positions.Count;

        rb.velocity = direction.normalized * (speed);
    }
}
