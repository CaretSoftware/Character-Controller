using UnityEngine;

public class TankFireEvent : MonoBehaviour {
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private Transform turretTransform;
    [SerializeField] private Vector3 barrelPoint;
    [SerializeField] private float recoilForce = 5f;
    [SerializeField] private float projectileForce = 100f;
    [SerializeField] private float projectileUpwardsForce = 20f;
    private int toggle = -1;
    public void AddForce(AnimationEvent e) {
        rigidBody.AddForce(-transform.forward * recoilForce, ForceMode.Impulse);
        
        toggle *= -1;
        Vector3 firePoint = barrelPoint;
        firePoint.y *= toggle;
        Rigidbody projectile = Instantiate(ammoPrefab, turretTransform.TransformPoint(firePoint),
            Quaternion.LookRotation(transform.forward, transform.right)).GetComponent<Rigidbody>();
        
        projectile.AddForce(projectileForce * transform.right + projectileUpwardsForce * Vector3.up, 
                ForceMode.VelocityChange);
    }
}
