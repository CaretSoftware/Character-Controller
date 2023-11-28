using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour {
    [SerializeField] private List<Vector3> positions = new List<Vector3>();
    private int currentGoalPosition;
    [SerializeField, Range(0f, 10f)] private float speed = 1f;
    private Rigidbody rb;

    private void Awake() => rb = GetComponent<Rigidbody>();
    private Vector3 pos;
    
    private void Update() => MovePlatformUpdate();

    private void MovePlatformUpdate() {
        Vector3 direction = positions[currentGoalPosition] - transform.position;
        float dist = direction.magnitude;
        if (dist < .2f)
            currentGoalPosition = ++currentGoalPosition % positions.Count;

        pos = (transform.position + Time.deltaTime * speed * direction.normalized);

        transform.position = pos;
        rb.position = pos;
    }
}
