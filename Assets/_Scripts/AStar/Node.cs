using UnityEngine;

public class Node : MonoBehaviour {
    public Node[] neighbours;
    public Vector3 position { get; private set; }

    private void Awake() => position = transform.position;
}
