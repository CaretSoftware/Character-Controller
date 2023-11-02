using System;
using Character;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ForceTrigger : MonoBehaviour {
    [SerializeField] private ForceMode forceMode;
    [SerializeField] private Vector3 force;
    [SerializeField] private LayerMask layerMask;
    private Collider[] results;
    private Bounds boxBounds;
    private CapsuleCollider characterCollider;
    public CapsuleCollider lastCollider;
    private void Awake() {
        boxBounds = GetComponent<Renderer>().bounds;
        characterCollider = FindObjectOfType<CharController>().GetComponent<CapsuleCollider>();
    }

    private void Update() {
        Collider[] results = new Collider[5];
        int hitCount = Physics.OverlapBoxNonAlloc(boxBounds.center, boxBounds.extents, results, transform.rotation, layerMask);

        if (hitCount > 0) {
            for (int i = 0; i < hitCount; i++) {
                if (results[i] == characterCollider && results[i] != lastCollider) {
                    CharController charController = characterCollider.transform.GetComponent<CharController>();
                    lastCollider = characterCollider;

                    charController.SetExternalForces(force, forceMode, true);
                    Debug.Log(results[i].gameObject.name);
                    break;
                }

                results[i] = null;
            }
        } else {
            lastCollider = null;
        }
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        CharController charController = hit.transform.GetComponent<CharController>();
        
        charController.SetExternalForces(force, forceMode);
    }

    private void OnDrawGizmos()
    {

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(boxBounds.center, boxBounds.size);
        }
    }
}
