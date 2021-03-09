using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using AI_PROG_SP21.NavMesh;

namespace AI_PROG_SP21.AI.HungryGuardActors
{
    /// <summary>
    /// A pawn that uses GuardActor finite state machine.
    /// </summary>
    public sealed class HungryGuardPawn : MonoBehaviour
    {
        #region Fields
        private GuardActorFSM machine;
        #endregion
        #region Inspector Fields
        [Header("Behaviour Drivers")]
        [Tooltip("The nav wrapper for the attached navmesh.")]
        [SerializeField] private NavMeshWrapper navTracker = default;
        [Tooltip("The transforms that should be chased.")]
        [SerializeField] private Transform[] suspiciousTransforms = default;
        [Tooltip("The patrol points that the guard moves between.")]
        [SerializeField] private Transform[] patrolPoints = default;
        [Header("Behaviour Parameters")]
        [Tooltip("The starting base speed of the actor.")]
        [SerializeField] private float startSpeed = 1f;
        [Tooltip("The max speed of the actor.")]
        [SerializeField] private float maxSpeed = 2f;
        [Tooltip("The speed increase per second.")]
        [SerializeField] private float speedBuildPerSecond = 0.05f;
        [Header("State Indication")]
        [Tooltip("The mesh that displays the state icon.")]
        [SerializeField] private MeshRenderer stateIndicator = default;
        [Tooltip("The material for the patrolling state.")]
        [SerializeField] private Material patrollingStateMaterial = default;
        [Tooltip("The material for the investigating state.")]
        [SerializeField] private Material investigatingStateMaterial = default;
        [Tooltip("The material for the chasing state.")]
        [SerializeField] private Material chasingStateMaterial = default;
        #endregion
        #region State Machine Accessors
        /// <summary>
        /// The current state of the pawn.
        /// </summary>
        public GuardActorFSM.State State => machine.CurrentState;
        /// <summary>
        /// The state machine attached to this guard.
        /// </summary>
        public GuardActorFSM Machine => machine;
        #endregion
        #region Initialize and Bind Input
        private void Start()
        {
            // Create the new state machine.
            machine = GuardActorFSM.MakeStateMachine(navTracker, transform, suspiciousTransforms);
            machine.PatrolPoints = patrolPoints;
            machine.BaseSpeed = startSpeed;
            // Listen for changes in the state machine.
            machine.StateChanged += OnStateChanged;
            machine.CaughtTransform = () => StartCoroutine(StageLost());
            // Set the state to patrolling.
            machine.CurrentState = GuardActorFSM.State.Patrolling;
        }
        private IEnumerator StageLost()
        {
            // TODO this is hard coded. Should be done elsewhere.
            foreach (Transform transform in suspiciousTransforms)
                transform.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        private void OnDestroy()
        {
            // Clear up bindings.
            machine.StateChanged -= OnStateChanged;
            navTracker.OnDestinationReached = default;
        }
        #endregion
        #region State Changed Listener
        private void OnStateChanged(object machine, GuardActorFSM.State state)
        {
            // Change the rendered state.
            switch (state)
            {
                case GuardActorFSM.State.Patrolling:
                    stateIndicator.material = patrollingStateMaterial;
                    break;
                case GuardActorFSM.State.Investigating:
                    stateIndicator.material = investigatingStateMaterial;
                    break;
                case GuardActorFSM.State.Chasing:
                    stateIndicator.material = chasingStateMaterial;
                    break;
            }
        }
        #endregion
        #region Base Tick Method
        private void FixedUpdate()
        {
            // Make the guard increase speed over time.
            machine.BaseSpeed =
                Mathf.Clamp(startSpeed + speedBuildPerSecond * Time.fixedTime,
                startSpeed, maxSpeed);
            // Invoke the state machine to update.
            machine.Tick(Time.fixedDeltaTime);
        }
        #endregion
    }
}
