using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Character {
    [RequireComponent(typeof(NavMeshAgent))]
    public class ClickToMoveInput : MonoBehaviour, IInput {
        public bool JumpPressed { get; set; }
        public float JumpPressedLast { get; }
        public bool JumpReleased { get; set; }
        public bool JumpHold { get; set; }
        public Vector2 Axis { get; set; }

        [SerializeField, Range(0f, 0.3f)] private float smoothing = .05f;
        private NavMeshAgent _agent;
        private Vector3 _agentDestination;
        private Transform _transform;
        private Camera _cam;
        private float _agentMaxDistance = .1f;
        private Vector2 smoothDirection;
        private bool arrived;

        public void SetDestination(Vector3 destination) => _agent.destination = destination;
        
        private void Awake() {
            _cam = Camera.main;
            _agent = GetComponent<NavMeshAgent>();
            _agent.updatePosition = false;   // CharacterController handles transforms movement
            _agent.updateRotation = false;   // CharacterController handles transforms rotation
            _agent.autoBraking = true;       // keeps agent from overshooting target
            _transform = transform;
        }
        
        private void Update() {
            GetMouseClickPosition();
            
            Move();
        }

        private void GetMouseClickPosition() {
            if (!Input.GetMouseButtonDown(0)) return;
            
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit)) {
                arrived = false;
                SetDestination(hit.point);
            }
        }

        [SerializeField] private float arrivalDistance = .5f;
        private void Move() {
            if (arrived) return;
            
            Vector3 position = _transform.position;
            Vector3 distance = _agent.destination - position;
            if (distance.magnitude < arrivalDistance) { // Arrived
                arrived = true;
                Axis = Vector2.zero;
                smoothDirection = Vector2.zero;
                return;
            }
            
            Vector3 targetPosition = _agent.nextPosition;
            Vector3 direction = targetPosition - position;
            direction.y = 0f;
            direction.Normalize();
            
            // Move NavMeshAgent
            float agentYPos = _agent.nextPosition.y;
            Vector3 agentPosition = position + direction.normalized * 
                    Mathf.Min(_agentMaxDistance, direction.magnitude);      // Limit navMesh Agent distance from transform
            agentPosition.y = agentYPos;                                        // Keep agent on floor
            _agent.nextPosition = agentPosition;
            
            // Smoothing
            float smooth = Mathf.Min(1f, Time.deltaTime / smoothing);
            Vector2 deltaDirection = new Vector2(direction.x, direction.z);
            smoothDirection = Vector2.Lerp(smoothDirection, deltaDirection, smooth);
            
            // Normalize
            if (smoothDirection.magnitude != 0f)
                smoothDirection.Normalize();
            
            Axis = smoothDirection;
        }
    }
}
