using System.Collections.Generic;
using UnityEngine;
using AI.Actors.StateQueueActors;
using Graphs.JointedGraph;
using PrefabScripts;
using Generators;
using Input;
using Extensions.Unity;

namespace Scenes
{
    // This contains the hard coded scene logic.
    /// <summary>
    /// Initializes the scene for the chapter 3 demo.
    /// </summary>
    public sealed class Chapter3Base_Bootstrapper : MonoBehaviour
    {
        #region Fields
        private LinearTransformJointedGraph graph;
        private HexGateSceneInstance[,] hexGates;
        private List<GraphNavigatorActor> actors;
        #endregion
        #region Inspector Fields
        [Tooltip("The hex pattern generator that will create the gates.")]
        [SerializeField] private HexPatternGenerator generator = null;
        [Tooltip("The input broadcaster for setting the nav target.")]
        [SerializeField] private ColliderClickBroadcaster clickBroadcaster = null;
        [Tooltip("The object that moves to the latest nav target.")]
        [SerializeField] private Transform targetIndicator = null;
        [Header("Actor Spawning")]
        [Range(0f, 1f)][Tooltip("The chance an actor will spawn at each tile.")]
        [SerializeField] private float chanceToSpawnActor = 0.1f;
        [Tooltip("The actor template, must contain a GraphNavigatorActor.")]
        [SerializeField] private GameObject actorTemplate = null;
        [Header("Gate Spawning")]
        [Range(0f, 1f)][Tooltip("The chance that any gate will be locked on generation.")]
        [SerializeField] private float chanceToLockGate = 0.1f;
        #endregion

        private void Start()
        {
            // Listen to right click broadcaster.
            clickBroadcaster.ColliderRightClicked += OnColliderRightClicked;
            // Generate the hexagonal regions for the maze.
            generator.Generate();
            GameObject[] hexRegions = generator.LastGeneratedObjects;
            // Initialize the graph and collections for the actors
            // and gate instance in the maze.
            graph = new LinearTransformJointedGraph();
            actors = new List<GraphNavigatorActor>();
            hexGates = new HexGateSceneInstance[generator.TilesX, generator.TilesZ];

            // First pass; set up all instance on the hex maze.
            for (int z = 0; z < generator.TilesZ; z++)
            {
                for (int x = 0; x < generator.TilesX; x++)
                {
                    // Retrieve the hex gate controller.
                    HexGateSceneInstance gate = hexGates[x, z] =
                        hexRegions[z * generator.TilesX + x]
                            .transform.GetComponent<HexGateSceneInstance>();
                    // Randomly set the locked state for each
                    // of the gates on this hex.
                    gate.RightJoint.isLocked = (Random.value < chanceToLockGate);
                    gate.TopRightJoint.isLocked = (Random.value < chanceToLockGate);
                    gate.TopLeftJoint.isLocked = (Random.value < chanceToLockGate);
                    gate.Refresh();
                    // Add the gate transform as a node in the graph.
                    graph.AddNode(gate.transform);
                    // Listen for future changes in gate state,
                    // notifying that the graph structure has changed.
                    // TODO this is hacky and poor encapsulation.
                    gate.StateChanged += graph.InvokeGraphChanged;
                    // Spawn an actor on this hex tile?
                    if (Random.value < chanceToSpawnActor)
                    {
                        // Instantiate the actor template
                        // and place them with a random rotation.
                        GameObject newActor = Instantiate(actorTemplate);
                        newActor.transform.position = gate.transform.position;
                        newActor.transform.SetEulerAngleY(Random.value * 360f);
                        // Link up the navigator to our graph.
                        GraphNavigatorActor navigator = newActor.GetComponent<GraphNavigatorActor>();
                        navigator.Graph = graph;
                        actors.Add(navigator);
                    }
                }
            }

            // Second pass; link up all the nodes in the graph.
            // TODO this is kind of a pain due to the hex layout,
            // maybe some better way to abstract this.
            for (int z = 0; z < generator.TilesZ; z++)
            {
                for (int x = 0; x < generator.TilesX; x++)
                {
                    HexGateSceneInstance gate = hexGates[x, z];
                    int thisHexIndex = z * generator.TilesX + x;
                    int otherHexIndex = 0;
                    gate.RightJoint.start = gate.transform;
                    gate.TopRightJoint.start = gate.transform;
                    gate.TopLeftJoint.start = gate.transform;
                    if (x < generator.TilesX - 1)
                    {
                        otherHexIndex = thisHexIndex + 1;
                        graph.LinkNodes(thisHexIndex, otherHexIndex, gate.RightJoint);
                        graph.LinkNodes(otherHexIndex, thisHexIndex, gate.RightJoint);
                        gate.RightJoint.end = graph[otherHexIndex];
                    }
                    if (z < generator.TilesZ - 1)
                    {
                        if (z % 2 == 0)
                        {
                            if (x > 0)
                            {
                                otherHexIndex = thisHexIndex + generator.TilesX - 1;
                                graph.LinkNodes(thisHexIndex, otherHexIndex, gate.TopLeftJoint);
                                graph.LinkNodes(otherHexIndex, thisHexIndex, gate.TopLeftJoint);
                                gate.TopLeftJoint.end = graph[otherHexIndex];
                            }
                            otherHexIndex = thisHexIndex + generator.TilesX;
                            graph.LinkNodes(thisHexIndex, otherHexIndex, gate.TopRightJoint);
                            graph.LinkNodes(otherHexIndex, thisHexIndex, gate.TopRightJoint);
                            gate.TopRightJoint.end = graph[otherHexIndex];
                        }
                        else
                        {
                            if (x < generator.TilesX - 1)
                            {
                                otherHexIndex = thisHexIndex + generator.TilesX + 1;
                                graph.LinkNodes(thisHexIndex, otherHexIndex, gate.TopRightJoint);
                                graph.LinkNodes(otherHexIndex, thisHexIndex, gate.TopRightJoint);
                                gate.TopRightJoint.end = graph[otherHexIndex];
                            }
                            otherHexIndex = thisHexIndex + generator.TilesX;
                            graph.LinkNodes(thisHexIndex, otherHexIndex, gate.TopLeftJoint);
                            graph.LinkNodes(otherHexIndex, thisHexIndex, gate.TopLeftJoint);
                            gate.TopLeftJoint.end = graph[otherHexIndex];
                        }
                    }
                }
            }
        }

        #region Set Target Click Listener
        private void OnColliderRightClicked(RaycastHit hitInfo)
        {
            // Find the node on the graph closest to the
            // clicked hit location.
            int node = graph.FindNearestNode(hitInfo.transform.position);
            // Move the indicator to that spot.
            targetIndicator.transform.position =
                graph[node].position + Vector3.up * 0.05f;
            // Update the actors.
            foreach (GraphNavigatorActor actor in actors)
                actor.Destination = hitInfo.transform.position;
        }
        #endregion
    }
}
