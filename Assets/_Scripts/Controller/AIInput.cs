using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Controller {
    public class AIInput : MonoBehaviour, IInput {
        public delegate void StuckDelegate(AIInput ai, Vector3 position);
        public static StuckDelegate stuck;
        public static List<AIInput> aiCongaLine = new List<AIInput>();

        public int myIndexInCongaLine {
            get;
            private set;
        }
        public bool JumpPressed { get; set; }
        public bool JumpHold { get; set; }  // TODO AI not holding button TODO
        public Vector2 Axis { get; set; }
        public Transform ChaseTransform {
            get => chaseTransform;
            set => chaseTransform = value;
        }
        public bool Chasing {
            get => chasing;
            set {
                if (value) AddAICatToCongaLine(this);
                chasing = value;
            }
        }

        private NavMeshAgent agent;
        private float updatePathTime;
        private bool chaseBuffer;
        private Vector2 smoothDirection;

        [SerializeField] private bool chasing;
        [Space]
        [SerializeField] private LayerMask levelGeometry; 
        private Transform myTransform;
        [Header("Jump")]
        [SerializeField] private Vector3 jumpHeightRayPosition;
        [SerializeField] private Vector3 jumpTooHighRayHeight;
        [SerializeField] private Vector3 jumpGapRayPosition;
        [SerializeField] private float jumpDistance = 1.5f;
        [SerializeField] private float tooHighJumpRayDistance = .95f;
        [Header("Chase")]
        [SerializeField] private Transform chaseTransform;
        [SerializeField, Range(0f, 2f)] private float catchDistance = 1f;
        [SerializeField, Range(0f, 1f)] private float chaseBufferDistance = .1f;
        [SerializeField, Range(1f / 60f, 1f)] private float updatePathInterval = .2f;
        [Header("Agent Movement")]
        [SerializeField, Range(0f, 0.3f)] private float smoothing = .05f;
        [SerializeField, Range(0f, 2f)] private float agentMaxDistance = .1f;
    
        private void OnValidate() { // Unity method that gets called when a value of this script is changed in the inspector
            if (chasing && chaseTransform == null) {
                Chasing = true; // Sets chase target automatically
            }
        }

        private void Awake() {
            stuck += AIStuck;
            myTransform = transform;
            aiCongaLine.Clear();    // Clear static list when reloading scenes, else they persist to the next scene after they are destroyed and point to null

            agent = GetComponent<NavMeshAgent>();
            agent.autoBraking = true;       // keeps agent from overshooting target
            agent.updatePosition = false;   // CharacterController handles transforms movement
            agent.updateRotation = false;   // CharacterController handles transforms rotation
        }

        private void OnDestroy() => stuck -= AIStuck;

        private float stuckTime;
        private float currentStuckTime;
        [SerializeField] private float stuckResolveTime = 2f;
        private void AIStuck(AIInput ai, Vector3 position) {
            if (ai == this) return;
            Vector3 myPos = myTransform.position;
            Vector3 nextTargetDirection = myPos - position;
            float distance = nextTargetDirection.magnitude;
        
            if (distance > 3f) return;
        
            stuckTime = Time.time + stuckResolveTime;
            nextTargetDirection.Normalize();
            nextTargetDirection = myPos + nextTargetDirection * 2f;
            agent.destination = nextTargetDirection;
        }

        private bool AddAICatToCongaLine(AIInput cat) {
            if (cat != null && !aiCongaLine.Contains(cat)) {
                aiCongaLine.Add(cat);
                myIndexInCongaLine = aiCongaLine.Count - 1;
                return GetChaseTargetInCongaLine(myIndexInCongaLine);      // else chase the last AICat
            }

            return false;
        }

        private bool HasChaseTarget() {
            if (chaseTransform != null) return true;            // next one ahead of us is not dead, success!

            if (GetChaseTargetInCongaLine(myIndexInCongaLine)) return true;

            PlayerInput player = FindObjectOfType<PlayerInput>();
            if (player == null) return false;
        
            chaseTransform = player.transform;                  // chase the player instead

            catchDistance = 0f;                                 // catch up to player
        
            return false;                                       // player was dead too
        }

        private bool GetChaseTargetInCongaLine(int myIndexInCongaLine) {
            for (int i = myIndexInCongaLine - 1; i >= 0; i--) { // look ahead in the conga line from our position
                if (aiCongaLine[i] == null) continue;           // next one was dead too, look further ahead

                chaseTransform = aiCongaLine[i].transform;      // found a live one!
                return true;                                    // success!
            }

            return false;
        }
    
        private void OnControllerColliderHit(ControllerColliderHit hit) {
        }
    
        private void FixedUpdate() {
            Move();
        }

        private Vector3 lastPos;
        private void Move() {
            // nothing to chase, stand still
            if (!Chasing || !HasChaseTarget()) return;

            Vector3 targetPosition = ChaseTransform.position;
            Vector3 transformPosition = myTransform.position;

            bool someoneStuck = (Time.time < stuckTime);
        
            // Update path?
            if (Time.time >= updatePathTime) {
                updatePathTime = Time.time + updatePathInterval;
                if (!someoneStuck)
                    agent.destination = targetPosition;
            }

            Vector3 direction = 
                Vector3.ProjectOnPlane(agent.nextPosition - transformPosition, Vector3.up);

            // Move navMeshAgent
            float agentYPos = agent.nextPosition.y;
            Vector3 agentPosition = transformPosition + direction.normalized  
                * Mathf.Min(agentMaxDistance, direction.magnitude);                 // Limit navMesh Agent distance
            agentPosition.y = agentYPos;                                                    // Keep agent on floor
            agent.nextPosition = agentPosition;
        
            // Stop? / Move?
            Vector3 targetVector =  Vector3.ProjectOnPlane(targetPosition - transformPosition, Vector3.up);
            float distanceToTarget = targetVector.magnitude;
            bool shouldMove = distanceToTarget > (chaseBuffer ? catchDistance + chaseBufferDistance : catchDistance);
            chaseBuffer = !shouldMove;  // if we arrived (e.g. should not move), we should use chaseBuffer next frame

            // Stuck?
            bool isStuck = lastPos == transformPosition;
            bool hasBeenStuck = currentStuckTime > 2f;
            if (shouldMove && isStuck) {
                currentStuckTime += Time.fixedDeltaTime;
            }
            if (hasBeenStuck) {
                currentStuckTime = 0f;
                stuck?.Invoke(this, transformPosition);
            }
            if (!isStuck)
                currentStuckTime = 0f;
            lastPos = transformPosition;

            // Smoothing
            float smooth = Mathf.Min(1f, Time.deltaTime / smoothing);
            Vector2 deltaDirection = new Vector2(direction.x, direction.z);
            smoothDirection = Vector2.Lerp(smoothDirection, deltaDirection, smooth);
        
            // Normalize
            if (smoothDirection.magnitude != 0f)
                smoothDirection.Normalize();
        
            Axis = shouldMove || someoneStuck ? smoothDirection : Vector2.zero;
        
            JumpPressed = ShouldJumpObstacle(distanceToTarget) || ShouldJumpGap(distanceToTarget);
        }

        private readonly RaycastHit[] results = new RaycastHit[1];
        private bool ShouldJumpObstacle(float distanceToTarget) {
            if (distanceToTarget < jumpDistance) return false; // We're probably on top of chaseTransforms head, stops us from bouncing around on top of it

            Vector3 pos = myTransform.TransformPoint(jumpHeightRayPosition);
            Vector3 fwd = myTransform.forward;

            Ray ray = new Ray(pos, fwd);
            Ray rayTooHigh = new Ray(pos + jumpTooHighRayHeight, fwd);

            Debug.DrawRay(pos, fwd, Color.red, Time.fixedDeltaTime);
            Debug.DrawRay(pos + Vector3.up, fwd, Color.red, Time.fixedDeltaTime);
        
            int hits = Physics.RaycastNonAlloc(ray, results, jumpDistance, levelGeometry, QueryTriggerInteraction.Ignore);
            int tooHighHits = Physics.RaycastNonAlloc(rayTooHigh, results, tooHighJumpRayDistance, levelGeometry, QueryTriggerInteraction.Ignore);
        
            return hits > 0 && tooHighHits == 0;    // hit obstacle but it's not too high for us to jump over
        }
    
        private bool ShouldJumpGap(float distanceToTarget) {
            if (distanceToTarget < catchDistance) return false; // We're probably on top of chaseTransforms head, stops us from bouncing around on top of it
        
            Vector3 pos = myTransform.TransformPoint(jumpGapRayPosition);
            Ray ray = new Ray(pos, Vector3.down);

            int hits = Physics.RaycastNonAlloc(ray, results, jumpDistance, levelGeometry, QueryTriggerInteraction.Ignore);
            return hits == 0;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
        
            if (agent != null) {
                Gizmos.DrawWireSphere(agent.nextPosition, .5f);
                Gizmos.color = Color.red.WithAlpha(.5f);
                Gizmos.DrawWireSphere(agent.destination, .5f);
            }
        
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
        
            // jump height
            // Gizmos.DrawWireSphere(jumpHeightRayPosition, .15f);
            // Gizmos.DrawLine(jumpHeightRayPosition, jumpHeightRayPosition + Vector3.forward * jumpDistance);
        
            // jump gap
            // Gizmos.DrawWireSphere(jumpGapRayPosition, .15f);
            // Gizmos.DrawLine(jumpGapRayPosition, jumpGapRayPosition + Vector3.down * jumpDepth);
        
            Gizmos.color = Color.red;

            // jump tooHigh
            // Gizmos.DrawWireSphere(jumpTooHighRayHeight, .15f);
            // Gizmos.DrawLine(jumpTooHighRayHeight, jumpTooHighRayHeight + Vector3.forward * tooHighJumpRayDistance);
        }
    }
}
