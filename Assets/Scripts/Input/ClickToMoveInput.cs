using System;
using UnityEngine;
using UnityEngine.AI;

namespace Character {
    public class ClickToMoveInput : MonoBehaviour, IInput {
        public bool JumpPressed { get; set; }
        public float JumpPressedLast { get; }
        public bool JumpReleased { get; set; }
        public bool JumpHold { get; set; }
        public bool FirePressed { get; set; }
        public bool FireReleased { get; set; }
        public bool InteractHeld { get; set; }
        public bool PausePressed { get; set; }
        public Vector2 Axis { get; set; }

        [SerializeField] private float arrivalDistance = .5f;

        private NavMeshPath navMeshPath;
        private Transform _transform;
        private Camera _cam;
        private int currentPathCorner;

        private void Awake() {
            _cam = Camera.main;
            _transform = transform;
            navMeshPath = new NavMeshPath();
        }

        private void Update() {
            GetMouseClickPosition();

            MoveAlongPath();
            // Move();
        }
        
        private void GetMouseClickPosition() {
            if (!Input.GetMouseButtonDown(0)) return;
            
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit)) {
                debugPosition = hit.point;
                currentPathCorner = 0;

                if (!NavMesh.CalculatePath(transform.position, hit.point + Vector3.up, NavMesh.AllAreas, navMeshPath))
                    Debug.LogWarning("No Path"); 
            }
        }

        private void MoveAlongPath() {
            Axis = Vector2.zero;
            if (currentPathCorner >= navMeshPath.corners.Length) return;

            Vector3 targetPos = navMeshPath.corners[currentPathCorner];
            Vector3 position = _transform.position;
            Vector3 direction = targetPos - position;

            if (direction.magnitude < arrivalDistance)
                currentPathCorner++;

            direction.Normalize();
            Axis = new Vector2(direction.x, direction.z);
        }
        
        private Vector3 debugPosition;
        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(debugPosition, Vector3.one);
        }
    }
}
