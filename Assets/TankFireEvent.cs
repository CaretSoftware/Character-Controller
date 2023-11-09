using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankFireEvent : MonoBehaviour {
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float recoilForce = 5f;
    
    public void AddForce(AnimationEvent e) => rigidBody.AddForce(-transform.forward * recoilForce, ForceMode.Impulse);
}
