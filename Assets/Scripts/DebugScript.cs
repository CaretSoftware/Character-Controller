using System;
using UnityEngine;

public class DebugScript : MonoBehaviour {
    public static float debug;
    public float val;

    private void Update() {
        val = debug;
    }

    private void OnDrawGizmos() {
        
        //Gizmos.DrawSphere(pos, .5f * .75f);
    }
}
