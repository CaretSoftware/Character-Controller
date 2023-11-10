using System;
using UnityEngine;

public class AtomicBomb : MonoBehaviour {
    [SerializeField] private SphereCollider collider;
    [SerializeField] private GameObject mushroomCloud;
    
    private void Awake() => Invoke(nameof(EnableCollider), 1f);

    private void EnableCollider() => collider.enabled = true;

    private void OnTriggerEnter(Collider other) {
        // Explode
        Instantiate(mushroomCloud, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
