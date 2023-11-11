using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class AtomicBomb : MonoBehaviour {
    [SerializeField] private SphereCollider myCollider;
    [SerializeField] private GameObject mushroomCloud;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float spinVelocity = 3f;
    
    private GameObject myMushroomCloud;
    private Quaternion initialRotation;
    private Vector3 initialDirection;
    private Vector3 spin;
    
    private void Awake() {
        initialRotation = transform.rotation;
        this.gameObject.SetActive(false);
    }

    public void Fire(Vector3 position, Vector3 force) {
        transform.position = position;
        gameObject.SetActive(true);
        
        rigidBody.useGravity = true;
        rigidBody.velocity = Vector3.zero;
        rigidBody.AddForce(force, ForceMode.VelocityChange);
        initialDirection = force;

        myCollider.enabled = false;
        Invoke(nameof(EnableCollider), 1f);
    }
    
    private void Update() {
        Vector3 forward = rigidBody.velocity != Vector3.zero ? rigidBody.velocity.normalized : initialDirection;
        spin.z += Time.deltaTime * spinVelocity;
        transform.rotation = Quaternion.LookRotation(forward) * initialRotation * Quaternion.Euler(spin);
    }

    private void EnableCollider() => myCollider.enabled = true;

    private void OnTriggerEnter(Collider other) {
        // Explode
        if (myMushroomCloud == null)
            myMushroomCloud = Instantiate(mushroomCloud, transform.position, Quaternion.identity);
        else
            myMushroomCloud.transform.position = transform.position;
        
        myMushroomCloud.GetComponent<AnimateMushroomCloud>().Explode();
        this.gameObject.SetActive(false);

        rigidBody.useGravity = false;
    }
}
