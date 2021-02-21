using UnityEngine;
using EngineInterop;
using Graphs.JointedGraph;
using Distributions;
using Services;
using Extensions.CSharp;

namespace AI.Actors.StateQueueActors
{
    /// <summary>
    /// An actor that is able to traverse to targets in a graph of Transforms.
    /// </summary>
    public sealed class GraphNavigatorActor : StateQueueActor<Vector3>
    {
        #region Fields
        private Vector3 nodeTarget;
        private Vector3 destination;
        // These are generated using a distribution.
        private float turnSpeed, walkSpeed, waypointTolerance;
        // Required references for functionality.
        private LinearTransformJointedGraph graph;
        private UpdateContext updateContext;
        #endregion
        #region Inspector Fields
        [Header("Movement Parameters")]
        [Tooltip("The walk speed in meters per second.")]
        [SerializeField] private FloatDistribution walkSpeedDistribution = default;
        [Tooltip("The turn speed in radians per second.")]
        [SerializeField] private FloatDistribution turnSpeedDistribution = default;
        [Tooltip("The radius in which a waypoint is marked as hit.")]
        [SerializeField] private FloatDistribution waypointToleranceDistribution = default;
        #endregion
        #region Initialization + Destruction
        protected override void Awake()
        {
            base.Awake();
            // Generate some atributes for this actor.
            turnSpeed = turnSpeedDistribution.Next();
            walkSpeed = walkSpeedDistribution.Next();
            waypointTolerance = waypointToleranceDistribution.Next();
            // Grab the update singleton.
            updateContext = ServiceManager.RetrieveService<UnityUpdateContext>();
        }
        private void OnDestroy()
        {
            // Clean up links to the update context instance.
            updateContext.Draw -= MoveUpdate;
        }
        #endregion
        #region Properties
        /// <summary>
        /// The graph that this actor moves relative to.
        /// </summary>
        public LinearTransformJointedGraph Graph
        {
            set
            {
                // Unbind events from previous listener.
                if (graph != null)
                    graph.GraphChanged -= RefreshRouting;
                // Bind to listen for updates.
                graph = value;
                graph.GraphChanged += RefreshRouting;
            }
        }
        /// <summary>
        /// Notifies this actor of a new target it should be
        /// moving along the graph towards.
        /// </summary>
        public Vector3 Destination
        {
            get => destination;
            set
            {
                destination = value;
                RefreshRouting();
            }
        }
        #endregion
        #region Navigation
        private void RefreshRouting()
        {
            // Clear any previous route data.
            ClearState();
            if (graph != null)
            {
                int start = graph.FindNearestNode(transform.position);
                int end = graph.FindNearestNode(destination);
                // If there is a path queue the node locations
                // as state into the queue.
                if (graph.TryNavigate(start, end, out Transform[] path))
                    foreach (Vector3 location in path.ArrayInto((Transform t) => t.position))
                        EnqueueState(location);
            }
        }
        #endregion
        #region Enter State Handler
        protected override void EnterState(Vector3 state)
        {
            nodeTarget = state;
            EnterMove();
        }
        #endregion
        #region Movement Behaviour
        private void EnterMove()
        {
            updateContext.Draw += MoveUpdate;
        }
        private void MoveUpdate(float deltaTime)
        {
            Vector3 desiredDirection = nodeTarget - transform.position;
            // Turn the actor towards their target.
            transform.forward = Vector3.RotateTowards(transform.forward, desiredDirection, turnSpeed * deltaTime, 0f);
            // Make sure we are not stepping over the target.
            if (walkSpeed * walkSpeed * deltaTime * deltaTime + waypointTolerance
                < desiredDirection.sqrMagnitude)
            {
                float walkDistance = walkSpeed * deltaTime;
                transform.position += transform.forward * walkDistance;
            }
            else
                ExitMove();
        }
        private void ExitMove()
        {
            updateContext.Draw -= MoveUpdate;
            OnStateExited();
        }
        #endregion
    }
}
