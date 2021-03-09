using UnityEngine;
using System.Collections;

using AI_PROG_SP21.NavMesh;

namespace AI_PROG_SP21.AI.HungryGuardActors
{
    /// <summary>
    /// A pawn that uses DefenderActor finite state machine.
    /// </summary>
    public sealed class DefenderPawn : MonoBehaviour
    {
        #region Fields
        private DefenderActorFSM machine;
        #endregion
        #region Inspector Fields
        [Header("Behaviour Drivers")]
        [Tooltip("The nav wrapper for the attached navmesh.")]
        [SerializeField] private NavMeshWrapper navWrapper = default;
        [Tooltip("The transform of the actor to defend.")]
        [SerializeField] private Transform actorToGuard = default;
        [Tooltip("The actors that will try to attack the actor.")]
        [SerializeField] private HungryGuardPawn[] aggressors = default;
        #endregion
        #region Initialize and Bind Input
        private void Start()
        {
            // Create the state machine and pass it the
            // aggressor pawns.
            machine = DefenderActorFSM.MakeStateMachine(navWrapper, transform, actorToGuard);
            StartCoroutine(LateInitialize());
        }
        private IEnumerator LateInitialize()
        {
            // TODO this is a hotfix (script execution order breaks this).
            yield return null;
            machine.OpponentPawns = aggressors;
            machine.CurrentState = DefenderActorFSM.State.Following;
        }
        private void OnDestroy()
        {
            // Clear up bindings.
            navWrapper.OnDestinationReached = default;
        }
        #endregion
        #region Base Tick Method
        private void FixedUpdate()
        {
            // Invoke the state machine to update.
            machine.Tick(Time.fixedDeltaTime);
        }
        #endregion
    }
}
