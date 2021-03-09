using System;
using UnityEngine;
using UnityEngine.AI;

using AI_PROG_SP21.EngineInterop;
using AI_PROG_SP21.Services;

namespace AI_PROG_SP21.NavMesh
{
    /// <summary>
    /// Wraps nav mesh functionality so it can be used in a callback model.
    /// </summary>
    public sealed class NavMeshWrapper : MonoBehaviour
    {
        #region Parameters
        private const float DEFAULT_TOLERANCE = 0.05f;
        #endregion
        #region Callbacks
        /// <summary>
        /// Called back when the destination has been reached.
        /// </summary>
        public Action OnDestinationReached { private get; set; }
        #endregion
        #region Fields
        private UnityUpdateContext updateContext;
        /// <summary>
        /// The remaining distance before destination
        /// reached is invoked.
        /// </summary>
        public float tolerance;
        #endregion
        #region Inspector Fields
        [Tooltip("The nav mesh agent component to watch.")]
        [SerializeField] private NavMeshAgent agent = default;
        #endregion
        #region Enable Update Method
        /// <summary>
        /// Tells this script that the agent is moving and 
        /// to call OnDestinationReached when it reaches
        /// the destination.
        /// </summary>
        public void MarkTraveling()
        {
            // Start watching the agent.
            updateContext.FixedStep += CheckCompletion;
        }
        /// <summary>
        /// Sets the agent's target destination and
        /// call OnDestinationReached when it reaches
        /// the specified destination.
        /// </summary>
        /// <param name="destination">The destination of the agent.</param>
        public void MarkTraveling(Vector3 destination)
        {
            agent.destination = destination;
            // Start watching the agent.
            updateContext.FixedStep += CheckCompletion;
        }
        #endregion
        #region Exit Travel Method
        /// <summary>
        /// Stops the current path travel and resets the agent.
        /// </summary>
        public void StopTraveling()
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        #endregion
        #region Pause Travel Property
        /// <summary>
        /// Can be used to temporarily pause the nav mesh.
        /// </summary>
        public bool IsPaused
        {
            set { agent.isStopped = value; }
        }
        #endregion
        // TODO This wrapper is out of control; really only update needs to be wrapped.
        public float Speed { set { agent.speed = value; } }
        public float RotationSpeed
        {
            get => agent.angularSpeed;
            set { agent.angularSpeed = value; }
        }
        #region Observation Update
        private void CheckCompletion(float deltaTime)
        {
            // TODO this is a hotfix for scene persistence issues.
            // Somewhere else is fucking the de-initialization.
            if (agent == null)
            {
                updateContext.FixedStep -= CheckCompletion;
                return;
            }
            // Don't check if the gameobject is disabled.
            if (agent.gameObject.activeSelf)
            {
                // Has the agent reached the destination?
                if (agent.remainingDistance < tolerance)
                {
                    updateContext.FixedStep -= CheckCompletion;
                    // Notify the listener.
                    OnDestinationReached?.Invoke();
                }
            }
        }
        #endregion
        #region Initialization + Destruction
        private void Awake()
        {
            // Retrieve the update context to hook onto update loop.
            updateContext = ServiceManager.RetrieveService<UnityUpdateContext>();
            tolerance = DEFAULT_TOLERANCE;
        }
        private void OnDestroy()
        {
            // Clean up state.
            updateContext.FixedStep -= CheckCompletion;
        }
        #endregion
    }
}
