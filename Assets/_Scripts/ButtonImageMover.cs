using UnityEngine;

public class ButtonImageMover : MonoBehaviour {
    private static bool shown;
    [SerializeField] private KeyCode key;
    [SerializeField] private Transform myTransform;
    private Vector3 pos;
    private bool push;
    private float progress;
    [SerializeField] private float pushSpeed = 5f;
    [SerializeField] private float releaseSpeed = 3f;
    [SerializeField] private float keyStrokeLength = .5f;
    [SerializeField] private Rigidbody rb;
    private float fixedDeltaTime;
    private void Awake() {
        if (shown)
            Destroy(this.gameObject);
        pos = myTransform.position;
        fixedDeltaTime = 1f / Time.fixedDeltaTime;
    }

    private void Start() => shown = true;

    private void Update() {
        push = Input.GetKey(key);
    }

    private void FixedUpdate() {
        float fixedTime = fixedDeltaTime * Time.unscaledDeltaTime;
        progress += push ? fixedTime * pushSpeed : -fixedTime * releaseSpeed;
        progress = Mathf.Clamp01(progress);
        Vector3 newPos = Vector3.Lerp(pos, pos + Vector3.forward * keyStrokeLength, progress);
        rb.MovePosition(newPos);
    }
}
