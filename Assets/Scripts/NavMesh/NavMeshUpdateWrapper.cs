using System;
using UnityEngine;
using EngineInterop;
using UnityEngine.AI;
using Services;

namespace NavMesh
{
    /// <summary>
    /// Wraps nav mesh functionality so it can be used in a callback model.
    /// </summary>
    public sealed class NavMeshUpdateWrapper : MonoBehaviour
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
        #endregion
        #region Observation Update
        private void CheckCompletion(float deltaTime)
        {
            // Has the agent reached the destination?
            if (agent.remainingDistance < tolerance)
            {
                updateContext.FixedStep -= CheckCompletion;
                // Notify the listener.
                OnDestinationReached?.Invoke();
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
