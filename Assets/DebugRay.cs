using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DebugRay : MonoBehaviour {
    public static Vector3 pos;
    public static float radius;

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos, radius);
    }
}
