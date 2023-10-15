using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CameraBounds : MonoBehaviour {
    public delegate void SetCameraBounds(Vector3 min, Vector3 max);
    public static SetCameraBounds setCameraBounds;

    [SerializeField] private bool zLimit;
    
    private Vector3 min;
    private Vector3 max;
    private Transform playerTransform;
    
    private void Start() {
        playerTransform = FindObjectOfType<PlayerInput>().transform;
        Transform myTransform = transform;
        Vector3 localScale = myTransform.localScale * .5f;
        Vector3 pos = myTransform.position;
        min = pos - localScale + Vector3.one * 2f;
        max = pos + localScale - Vector3.one * 2f;;

        if (!zLimit) {
            min.z = Mathf.NegativeInfinity + 10f;
            max.z = Mathf.Infinity - 10f;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform != playerTransform) return;
        setCameraBounds?.Invoke(min, max);
    }

    //private void OnTriggerExit(Collider other) {
    //    if (other.transform != playerTransform) return;
    //    setCameraBounds?.Invoke(
    //        min - Vector3.one * 5f,
    //        max + Vector3.one * 5f
    //        //new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity), 
    //        //new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity)
    //        );
    //}
}
