using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.Mathematics;
using UnityEngine;

public class TankAlignToSurface : MonoBehaviour {
    [SerializeField] private Vector3[] raycastPoints = new Vector3[4];
    [SerializeField] private List<Vector3> raycastHits = new List<Vector3>();
    [SerializeField] private Transform tankTransform;
    [SerializeField] private Transform tankParentTransform;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float radius = 1.18f;
    [SerializeField] private float smoothTime = .5f;
    private Vector3 normal;
    private Vector3 currentVelocity;

    private void Start() {
        //tankTransform.parent = null;
    }

    public void Update() {
        RayCastToSurface(out Vector3 surfaceNormal);

        normal = Vector3.Slerp(normal, surfaceNormal, smoothTime * Time.deltaTime);
        Debug.DrawRay(transform.position, normal, Color.blue, Time.deltaTime * 2f, false);
        tankTransform.position = tankParentTransform.position;
        var fwd = Vector3.ProjectOnPlane(tankParentTransform.forward, normal);
        //tankTransform.rotation = quaternion.Euler(-90f, 0f, 0f) * Quaternion.LookRotation(fwd, normal);
    }

    private bool RayCastToSurface(out Vector3 surfaceNormal) {
        raycastHits.Clear();
        for (int i = 0; i < raycastPoints.Length; i++) {
            raycastHits.Add(RayHit(tankTransform.TransformPoint(raycastPoints[i])));
        }
        if (raycastHits.Count < 4) {
            surfaceNormal = normal;
            return false;
        }
        
        raycastHits.Sort((Vector3 a, Vector3 b) => {
            if (a.y == b.y) return 0;
            return a.y > b.y ? -1 : 1;
        });
        raycastHits.RemoveAt(raycastHits.Count - 1);

        // Calculate vectors lying on the plane
        Vector3 U = raycastHits[1] - raycastHits[0];
        Vector3 V = raycastHits[2] - raycastHits[0];

        // Calculate the surface normal
        surfaceNormal = Vector3.Cross(V, U);
        surfaceNormal.Normalize();
        
        return true;
    }

    private Vector3 RayHit(Vector3 position) {
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 3f, layerMask)) {
            return hit.point;
        }

        
        return Vector3.zero;
    }
    
    private void OnDrawGizmos() {
        //Gizmos.color = Color.green;
        //foreach (var vector in raycastPoints) {
        //    Gizmos.DrawWireSphere(tankTransform.TransformPoint(vector), 1.18f);
        //}

        Gizmos.color = Color.red;
        foreach (var vector in raycastHits) {
            Gizmos.DrawWireSphere(vector + radius * Vector3.up, radius);
        }
    }
}
