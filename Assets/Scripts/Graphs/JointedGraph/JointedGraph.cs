using System;
using System.Collections.Generic;
using Extensions.CSharp;

namespace Graphs.JointedGraph
{
    /// <summary>
    /// Base class for jointed graph collections.
    /// </summary>
    /// <typeparam name="TValue">The value type at each node.</typeparam>
    /// <typeparam name="TJoint">The joint type between nodes.</typeparam>
    public abstract class JointedGraph<TValue, TJoint>
    {
        #region Events
        /// <summary>
        /// Called every time the graph structure has changed.
        /// </summary>
        public abstract event Action GraphChanged;
        #endregion
        #region Fields
        /// <summary>
        /// The nodes currently in the graph.
        /// </summary>
        protected List<Node> nodes;
        #endregion
        #region Constructor
        /// <summary>
        /// Creates a new empty jointed graph.
        /// </summary>
        public JointedGraph()
        {
            nodes = new List<Node>();
        }
        #endregion
        #region Collection Accessor
        /// <summary>
        /// Retrieves the node value at the given index.
        /// </summary>
        /// <param name="index">The index of the node.</param>
        /// <returns>The vlaue assigned to the node in the graph.</returns>
        public TValue this[int index]
        {
            get
            {
                #region Argument Checking
                if (!nodes.IndexArgsAreInRange(index))
                    throw new ArgumentOutOfRangeException(
                        "index", "The node index is not in this graph.");
                #endregion
                return nodes[index].value;
            }
        }
        #endregion
        #region Node Management Methods
        /// <summary>
        /// Adds a new node to the graph.
        /// </summary>
        /// <param name="nodeValue">The value at the node.</param>
        /// <returns>The index of the new node in the graph.</returns>
        public virtual int AddNode(TValue nodeValue)
        {
            nodes.Add(new Node(nodeValue));
            return nodes.Count - 1;
        }
        /// <summary>
        /// Removes a given node in the graph, recycling its index for any new nodes.
        /// </summary>
        /// <param name="nodeIndex">The index of the node to remove.</param>
        public virtual void RemoveNode(int nodeIndex)
        {
            #region Argument Checking
            if (!nodes.IndexArgsAreInRange(nodeIndex))
                throw new ArgumentOutOfRangeException(
                    "nodeIndex", "The node index is not in this graph.");
            #endregion
            // TODO implement this method.
            // Will require array reorganization that
            // does not interupt (shift) existing nodes.
            throw new NotImplementedException();
        }
        /// <summary>
        /// Links two nodes together with a one way joint.
        /// </summary>
        /// <param name="nodeA">The start node.</param>
        /// <param name="nodeB">The destination node.</param>
        /// <param name="joint">The linking joint.</param>
        public virtual void LinkNodes(int nodeA, int nodeB, TJoint joint)
        {
            #region Argument Checking
            if (!nodes.IndexArgsAreInRange(nodeA))
                throw new ArgumentOutOfRangeException(
                    "nodeA", "The node index is not in this graph.");
            if (!nodes.IndexArgsAreInRange(nodeB))
                throw new ArgumentOutOfRangeException(
                    "nodeB", "The end node index is not in this graph.");
            if (nodeA == nodeB)
                throw new ArgumentException("Node indices must be different.", "nodeB");
            #endregion
            nodes[nodeA].joints.Add(joint, nodes[nodeB]);
        }
        /// <summary>
        /// Remove the directional joint from one node to another.
        /// </summary>
        /// <param name="nodeA">The start node.</param>
        /// <param name="nodeB">The destination node.</param>
        public virtual void UnlinkNodes(int nodeA, int nodeB)
        {
            #region Argument Checking
            if (!nodes.IndexArgsAreInRange(nodeA))
                throw new ArgumentOutOfRangeException(
                    "nodeA", "The node index is not in this graph.");
            if (!nodes.IndexArgsAreInRange(nodeB))
                throw new ArgumentOutOfRangeException(
                    "nodeB", "The end node index is not in this graph.");
            if (nodeA == nodeB)
                throw new ArgumentException("Node indices must be different.", "nodeB");
            #endregion
            // Check each joint in nodeA and collect
            // the joints that link to nodeB.
            List<TJoint> jointsToRemove = new List<TJoint>();
            foreach (KeyValuePair<TJoint, Node> joint in nodes[nodeA].joints)
                if (joint.Value == nodes[nodeB])
                    jointsToRemove.Add(joint.Key);
            // Remove the joints.
            foreach (TJoint joint in jointsToRemove)
                nodes[nodeA].joints.Remove(joint);
        }
        #endregion
        #region Navigation Methods
        /// <summary>
        /// Attempts to find a path through the graph.
        /// </summary>
        /// <param name="startNode">The node to start at.</param>
        /// <param name="endNode">The target destination node.</param>
        /// <param name="path">The path of nodes found, not including the start.</param>
        /// <returns>True if a path was found.</returns>
        public virtual bool TryNavigate(int startNode, int endNode, out TValue[] path)
        {
            #region Argument Checking
            if (!nodes.IndexArgsAreInRange(startNode))
                throw new ArgumentOutOfRangeException(
                    "startNode", "The node index is not in this graph.");
            if (!nodes.IndexArgsAreInRange(endNode))
                throw new ArgumentOutOfRangeException(
                    "endNode", "The end node index is not in this graph.");
            #endregion
            // Handle the case where the navigation is
            // already at the destination.
            if (startNode == endNode)
            {
                path = new TValue[] { this[endNode] };
                return true;
            }
            // Initialize route data for the search.
            Dictionary<Node, RouteData> routeScore =
                new Dictionary<Node, RouteData>();
            foreach (Node node in nodes)
                routeScore.Add(node, new RouteData());
            // Initialize the open nodes collection
            // with the start point of the path.
            List<Node> openNodes = new List<Node>();
            openNodes.Add(nodes[startNode]);
            routeScore[nodes[startNode]].travelCost = 0f;
            routeScore[nodes[startNode]].heuristicCost =
                GetHeuristic(nodes[startNode], nodes[endNode]);
            routeScore[nodes[startNode]].calculated = true;
            // Start iteration through search algorithm.
            while (openNodes.Count > 0)
            {
                // Find the lowest cost of current movement options.
                Node current = openNodes[0];
                for (int i = 1; i < openNodes.Count; i++)
                    if (routeScore[openNodes[i]].totalCost < routeScore[current].totalCost)
                        current = openNodes[i];
                // Return the path if the end has been found.	
                if (current == nodes[endNode])
                {
                    // Unwind the path using the parent at
                    // each node along the route score data.
                    List<TValue> tracedPath = new List<TValue>();
                    while (routeScore[current].parent != null)
                    {
                        tracedPath.Add(current.value);
                        current = routeScore[current].parent;
                    }
                    tracedPath.Reverse();
                    // Return path data and break out.
                    path = tracedPath.ToArray();
                    return true;
                }
                // Check the nodes of the path for
                // movement options.
                foreach (TJoint joint in current.joints.Keys)
                {
                    if (JointIsOpen(joint))
                    {
                        // Get the node at the other end of this joint,
                        // calculate its heuristic if it hasn't been calculated.
                        Node jointEndNode = current.joints[joint];
                        if (!routeScore[jointEndNode].calculated)
                            routeScore[jointEndNode].heuristicCost =
                                GetHeuristic(jointEndNode, nodes[endNode]);
                        // Check to see if this is the fastest we have
                        // ever reached this location in the graph.
                        float newTravelCost = routeScore[current].travelCost + GetTravelCost(joint);
                        if (newTravelCost < routeScore[jointEndNode].travelCost)
                        {
                            // If so update the route data.
                            routeScore[jointEndNode].travelCost = newTravelCost;
                            routeScore[jointEndNode].parent = current;
                            // Make sure this movement option is
                            // in the open nodes collection.
                            if (!openNodes.Contains(jointEndNode))
                                openNodes.Add(jointEndNode);
                        }
                    }
                }
                openNodes.Remove(current);
            }
            // Failed to find a path.	
            path = new TValue[0];
            return false;
        }
        #endregion
        #region Navigation Overridable Methods
        /// <summary>
        /// Gets an estimate for the travel cost to reach the end node.
        /// </summary>
        /// <param name="start">The start node.</param>
        /// <param name="end">The end target node.</param>
        /// <returns>Approximated cost to reach the end node.</returns>
        protected virtual float GetHeuristic(Node start, Node end) => 0f;
        /// <summary>
        /// Checks if a joint can currently be traversed.
        /// </summary>
        /// <param name="joint">The joint to check.</param>
        /// <returns>True if the joint can be traveled along.</returns>
        protected virtual bool JointIsOpen(TJoint joint) => true;
        /// <summary>
        /// Gets the travel cost for moving along a joint.
        /// </summary>
        /// <param name="joint">The joint to move along.</param>
        /// <returns>The relative cost of traveling along this joint.</returns>
        protected virtual float GetTravelCost(TJoint joint) => 1f;
        #endregion
        #region Data Structure Classes
        /// <summary>
        /// Stores the node data for the graph.
        /// </summary>
        protected sealed class Node
        {
            /// <summary>
            /// The value at the node.
            /// </summary>
            public readonly TValue value;
            /// <summary>
            /// The joints that start at this node.
            /// </summary>
            public readonly Dictionary<TJoint, Node> joints;
            /// <summary>
            /// Creates a new node with no joints.
            /// </summary>
            /// <param name="value">The value at the node.</param>
            public Node(TValue value)
            {
                this.value = value;
                joints = new Dictionary<TJoint, Node>();
            }
        }
        // Stores the route data for the 
        // navigation method implementation.
        private sealed class RouteData
        {
            public Node parent;
            public bool calculated;
            public float heuristicCost, travelCost;
            public float totalCost => heuristicCost + travelCost;
            public RouteData()
            {
                calculated = false;
                // TODO these value are so they are easily
                // beaten, but this should be a sentinel value.
                heuristicCost = float.MaxValue / 3f;
                travelCost = float.MaxValue / 3f;
            }
        }
        #endregion
    }
}
