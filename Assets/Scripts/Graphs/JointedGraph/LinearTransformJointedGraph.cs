using System;
using UnityEngine;

namespace AI_PROG_SP21.Graphs.JointedGraph
{
    /// <summary>
    /// Implements a jointed graph using Transforms as the nodes.
    /// </summary>
    public sealed class LinearTransformJointedGraph
        : JointedGraph<Transform, LockableTransformJoint>
    {
        #region Events
        /// <summary>
        /// Called every time the graph structure is updated.
        /// </summary>
        public override event Action GraphChanged;
        #endregion
        // TODO this should not be here; the graph
        // should respond to changes automatically.
        public void InvokeGraphChanged()
        {
            GraphChanged?.Invoke();
        }
        #region Navigation Implementation
        protected override sealed float GetHeuristic(Node start, Node end)
        {
            // Use the manhattan distance.
            return 
                Mathf.Abs(start.value.position.x - end.value.position.x) +
                Mathf.Abs(start.value.position.y - end.value.position.y) +
                Mathf.Abs(start.value.position.z - end.value.position.z);
        }
        protected override sealed bool JointIsOpen(LockableTransformJoint joint)
        {
            return !joint.isLocked;
        }
        protected override sealed float GetTravelCost(LockableTransformJoint joint)
        {
            return Vector3.SqrMagnitude(joint.start.position - joint.end.position);
        }
        #endregion
        #region Nearest Node Utility Method
        /// <summary>
        /// Finds the nearest graph node to a Vector location.
        /// </summary>
        /// <param name="position">The position to search for nodes.</param>
        /// <returns>THe nearest node in the graph.</returns>
        public int FindNearestNode(Vector3 position)
        {
            // Search for the nearest node using square magnitude.
            int nearestIndex = -1;
            float nearestSquaredDistance = float.MaxValue;
            for (int i = 0; i < nodes.Count; i++)
            {
                float squaredDistance =
                    Vector3.SqrMagnitude(position - nodes[i].value.position);
                if (squaredDistance < nearestSquaredDistance)
                {
                    nearestSquaredDistance = squaredDistance;
                    nearestIndex = i;
                }
            }
            return nearestIndex;
        }
        #endregion
#if UNITY_EDITOR
        #region Debug Methods
        /// <summary>
        /// Provides a test method that draws debug lines
        /// along the graph joints, then pauses the editor.
        /// </summary>
        public void DebugDrawJoints()
        {
            foreach (Node nodeStart in nodes)
                foreach (Node nodeEnd in nodeStart.joints.Values)
                    UnityEngine.Debug.DrawLine(
                        nodeStart.value.position,
                        nodeEnd.value.position);
            UnityEngine.Debug.Break();
        }
        #endregion
#endif
    }
}
