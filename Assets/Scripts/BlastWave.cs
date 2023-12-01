using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using UnityEngine;

public class BlastWave : MonoBehaviour {
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float blastForce = 20f;
    [SerializeField] private float cameraShakeTrauma = 3f;
    
    private Rigidbody[] rigidbodies;
    private Transform mainCamera;
    private bool played;
    private float distance;
    private HashSet<Rigidbody> affectedRigidbodies = new HashSet<Rigidbody>();
    
    private void OnEnable() {
        affectedRigidbodies.Clear();
        rigidbodies = FindObjectsOfType<Rigidbody>();
        played = false;
        if (mainCamera == null)
            mainCamera = Camera.main.transform;
        distance = (mainCamera.position - transform.position).magnitude;
    }

    private void Update() {
        Vector3 position = transform.position;
        float currentScale = transform.localScale.x;
        for (int i = 0; i < rigidbodies.Length; i++) {
            Vector3 dir = rigidbodies[i].transform.position - position;

            if (affectedRigidbodies.Contains(rigidbodies[i]) || dir.magnitude > currentScale)
                continue;
            
            rigidbodies[i].AddForce(blastForce * dir);
            affectedRigidbodies.Add(rigidbodies[i]);
        }
        if (!played && currentScale >= distance - 10f) {
            played = true;
            SoundManager.DampenAudioTemporarily(.2f, 1f, Ease.InCirc);
            SoundManager.PlaySound(Sound.BlastWave, .35f, false, true);
            CameraFollowClose.cameraShake?.Invoke(cameraShakeTrauma);
        }
    }
}
