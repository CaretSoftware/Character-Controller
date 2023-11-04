using System;
using UnityEngine;

public class DebugScript : MonoBehaviour {
    public static Vector3 pos;

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(pos, .5f * .75f);
    }
}
