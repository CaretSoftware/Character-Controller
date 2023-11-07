using System;
using UnityEngine;

public class DebugScript : MonoBehaviour {
    public static float debug;
    public float val;
    public static Vector3[] debugPoints;
    public Vector3[] points;
    
    private void Update() {
        val = debug;
        points ??= debugPoints;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        foreach (var vector in debugPoints) {
            Gizmos.DrawWireSphere(vector, .5f);
        }
        //Gizmos.DrawSphere(pos, .5f * .75f);
    }
}
