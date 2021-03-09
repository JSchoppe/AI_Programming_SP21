using UnityEngine;

namespace AI_PROG_SP21.Graphs.JointedGraph
{
    /// <summary>
    /// Holds data for a lockable joint between transforms.
    /// </summary>
    public sealed class LockableTransformJoint
    {
        #region Fields
        // TODO there should be a base class to abstract isLocked.
        /// <summary>
        /// The current lock state of this joint.
        /// </summary>
        public bool isLocked;
        /// <summary>
        /// The joint start transform.
        /// </summary>
        public Transform start;
        /// <summary>
        /// The joint end transform.
        /// </summary>
        public Transform end;
        #endregion
    }
}
