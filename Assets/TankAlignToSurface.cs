using System.Collections.Generic;
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
    
    public void Update() => AlignToSurface();
    
    private void AlignToSurface() {
        RayCastToSurface(out Vector3 surfaceNormal);
        
        Physics.SphereCast(transform.position + Vector3.up * 3.1f, 3f, Vector3.down, 
                out RaycastHit hit, 3f, layerMask);
        normal = Vector3.SmoothDamp(normal, surfaceNormal, ref currentVelocity, smoothTime);
        
        var fwd = Vector3.ProjectOnPlane(tankParentTransform.forward, normal);
        tankTransform.rotation = Quaternion.LookRotation(fwd, normal);
    }
    
    private void RayCastToSurface(out Vector3 surfaceNormal) {
        raycastHits.Clear();
        for (int i = 0; i < raycastPoints.Length; i++)
            raycastHits.Add(RayHit(tankTransform.TransformPoint(raycastPoints[i])));
        
        if (raycastHits.Count < 4)
            surfaceNormal = normal;
        
        raycastHits.Sort((Vector3 a, Vector3 b) => {
            if (a.y == b.y) 
                return 0;
            return a.y > b.y ? -1 : 1;
        });
        
        // Remove lowest point to guarantee a flat plane (triangle)
        raycastHits.RemoveAt(raycastHits.Count - 1);

        // Calculate vectors lying on the plane
        Vector3 U = raycastHits[1] - raycastHits[0];
        Vector3 V = raycastHits[2] - raycastHits[0];
        
        // Calculate the surface normal
        surfaceNormal = Vector3.Cross( U, V);
        surfaceNormal.Normalize();
        
        // Vectors might be clockwise, invert surfaceNormal if it is
        if (surfaceNormal.y < 0f)
            surfaceNormal = -surfaceNormal;
    }
    
    private Vector3 RayHit(Vector3 position) {
        if (Physics.SphereCast(position + (radius + 0.1f) * Vector3.up, radius, Vector3.down, 
                    out RaycastHit hit, 3f, layerMask)) {
            return hit.point;
        }
        
        return Vector3.zero;
    }
    
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        foreach (var vector in raycastPoints)
            Gizmos.DrawWireSphere(tankTransform.TransformPoint(vector), 1.18f);
        
        Gizmos.color = Color.red;
        foreach (var vector in raycastHits)
            Gizmos.DrawWireSphere(vector + radius * Vector3.up, radius);
    }
}
