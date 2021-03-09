using UnityEngine;

using AI_PROG_SP21.NavMesh;
using AI_PROG_SP21.Input;

namespace AI_PROG_SP21.AI.HungryGuardActors
{
    /// <summary>
    /// A pawn that uses TargetMoveActor finite state machine.
    /// </summary>
    public sealed class TargetMovePawn : MonoBehaviour
    {
        #region Fields
        private TargetMoveActorFSM machine;
        #endregion
        #region Inspector Fields
        [Tooltip("Relays input information when user clicks a collider.")]
        [SerializeField] ColliderClickBroadcaster inputBroadcaster = default;
        [Tooltip("The navigation wrapper makes interfacing with navmesh easier.")]
        [SerializeField] NavMeshWrapper navWrapper = default;
        #endregion
        #region State Machine Accessor
        /// <summary>
        /// The current state of the pawn.
        /// </summary>
        public TargetMoveActorFSM.State State => machine.CurrentState;
        #endregion
        #region Initialize and Bind Input
        private void Awake()
        {
            machine = TargetMoveActorFSM.MakeStateMachine(navWrapper);
            inputBroadcaster.ColliderClicked += DestinationClickRecieved;
        }
        private void OnDestroy()
        {
            navWrapper.OnDestinationReached = null;
        }
        private void DestinationClickRecieved(RaycastHit hitInfo)
        {
            // When the player clicks somewhere have the pawn go there.
            machine.Target = hitInfo.point;
            machine.CurrentState = TargetMoveActorFSM.State.Walking;
        }
        #endregion
    }
}
