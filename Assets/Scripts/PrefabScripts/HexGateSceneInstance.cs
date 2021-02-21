using System;
using UnityEngine;
using Input;
using Graphs.JointedGraph;

namespace PrefabScripts
{
    /// <summary>
    /// Holds the state for a patternable hex gate.
    /// </summary>
    public sealed class HexGateSceneInstance : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// Called whenever the gate state changes.
        /// </summary>
        public event Action StateChanged;
        #endregion
        #region Inspector Fields
        [Header("Gate Input")]
        [Tooltip("Provides input to open/close the gates.")]
        [SerializeField] private ColliderClickBroadcaster clickBroadcaster = null;
        [Header("Gate Objects")]
        [Tooltip("The right gate on this hexagon tile.")]
        [SerializeField] private Transform rightGate = null;
        [Tooltip("The upper right gate on this hexagon tile.")]
        [SerializeField] private Transform topRightGate = null;
        [Tooltip("The upper left gate on this hexagon tile.")]
        [SerializeField] private Transform topLeftGate = null;
        [Header("Gate Open Close Parameters")]
        [Tooltip("Material to use when this gate is open.")]
        [SerializeField] private Material gateOpenMaterial = null;
        [Tooltip("Material to use when this gate is closed.")]
        [SerializeField] private Material gateClosedMaterial = null;
        [Tooltip("Local position of the gate when it is closed.")]
        [SerializeField] private Vector3 closedPosition = default;
        [Tooltip("Local position of the gate when it is open.")]
        [SerializeField] private Vector3 openPosition = default;
        #endregion
        #region Initialization
        private void Awake()
        {
            // Initialize joints.
            RightJoint = new LockableTransformJoint();
            TopRightJoint = new LockableTransformJoint();
            TopLeftJoint = new LockableTransformJoint();
            // Subscribe to input.
            clickBroadcaster.ColliderClicked += OnColliderClicked;
        }
        #endregion
        #region Joint Accessors
        /// <summary>
        /// The joint to the right edge of the hexagon.
        /// </summary>
        public LockableTransformJoint RightJoint { get; private set; }
        /// <summary>
        /// The joint to the upper right edge of the hexagon.
        /// </summary>
        public LockableTransformJoint TopRightJoint { get; private set; }
        /// <summary>
        /// The joint to the upper left edge of the hexagon.
        /// </summary>
        public LockableTransformJoint TopLeftJoint { get; private set; }
        #endregion
        #region Visual Refresh Method
        /// <summary>
        /// Refreshes the visual state of the gates.
        /// </summary>
        public void Refresh()
        {
            // Update the visual state for each gate.
            SetGateVisual(rightGate, RightJoint.isLocked);
            SetGateVisual(topRightGate, TopRightJoint.isLocked);
            SetGateVisual(topLeftGate, TopLeftJoint.isLocked);
            void SetGateVisual(Transform gate, bool isClosed)
            {
                if (isClosed)
                {
                    gate.localPosition = closedPosition;
                    gate.GetComponent<MeshRenderer>().material = gateClosedMaterial;
                }
                else
                {
                    gate.localPosition = openPosition;
                    gate.GetComponent<MeshRenderer>().material = gateOpenMaterial;
                }
            }
        }
        #endregion
        #region Collider Click Listener
        private void OnColliderClicked(RaycastHit hitInfo)
        {
            // Check if the hit transform is one of our gates.
            bool hitGate = false;
            if (hitInfo.transform == rightGate)
            {
                RightJoint.isLocked = !RightJoint.isLocked;
                hitGate = true;
            }
            else if (hitInfo.transform == topRightGate)
            {
                TopRightJoint.isLocked = !TopRightJoint.isLocked;
                hitGate = true;
            }
            else if (hitInfo.transform == topLeftGate)
            {
                TopLeftJoint.isLocked = !TopLeftJoint.isLocked;
                hitGate = true;
            }
            // If a gate state has changed, refresh the visual
            // state and broadcast state change.
            if (hitGate)
            {
                Refresh();
                StateChanged?.Invoke();
            }
        }
        #endregion
    }
}
