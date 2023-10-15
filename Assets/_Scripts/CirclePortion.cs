using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CirclePortion : MonoBehaviour {
    public float radius = 1.0f;         // Radius of the circle
    public float angle = 90.0f;         // Angle in degrees for the portion of the circle
    public int segments = 100;          // Number of line segments to create the circle portion
    public Material lineMaterial;       // Material for the LineRenderer

    private LineRenderer lineRenderer;

    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.positionCount = segments + 1;
        CreateCirclePortion();
    }

    [SerializeField] private float off;
    void CreateCirclePortion() {
        float angleStep = angle / segments;
        Quaternion offset = quaternion.Euler(0f, off * Mathf.Deg2Rad, 0f);
        Vector3 pos = transform.position;
        for (int i = 0; i <= segments; i++) {
            float theta = Mathf.Deg2Rad * (i * angleStep);
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);

            Vector3 position = new Vector3(x, .1f, y);
            
            lineRenderer.SetPosition(i, pos + offset * transform.rotation * position);
        }
    }

    private void OnDestroy() {
        Destroy(lineRenderer);
    }
}
