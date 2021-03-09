using System.Collections.Generic;
using UnityEngine;

using AI_PROG_SP21.AI.Actors.FiniteStateMachines;
using AI_PROG_SP21.NavMesh;

namespace AI_PROG_SP21.AI.HungryGuardActors
{
    /// <summary>
    /// A state machine for a click move nav player.
    /// </summary>
    public sealed class TargetMoveActorFSM : StateMachine<TargetMoveActorFSM.State>
    {
        #region State Definition
        /// <summary>
        /// The states for the target move actor.
        /// </summary>
        public enum State : byte
        {
            /// <summary>
            /// Actor is awaiting input.
            /// </summary>
            Idle,
            /// <summary>
            /// Actor is moving towards the set target.
            /// </summary>
            Walking
        }
        #endregion
        #region Constructor + Factory
        private TargetMoveActorFSM(Dictionary<State, IStateMachineState> states, State defaultState)
            : base(states, defaultState)
        {
            
        }
        /// <summary>
        /// Creates a new state machine with the given nav wrapper.
        /// </summary>
        /// <param name="navWrapper">The nav agent wrapper.</param>
        /// <returns>The new state machine.</returns>
        public static TargetMoveActorFSM MakeStateMachine(NavMeshWrapper navWrapper)
        {
            var states = new Dictionary<State, IStateMachineState>();
            var machine = new TargetMoveActorFSM(states, State.Idle);
            // Initialize the states.
            states.Add(State.Walking, new WalkingState(navWrapper, machine));
            states.Add(State.Idle, new IdleState(navWrapper));
            return machine;
        }
        #endregion
        #region State Wrappers
        /// <summary>
        /// The movement target for the machine.
        /// </summary>
        public Vector3 Target
        {
            get => ((WalkingState)this[State.Walking]).Target;
            set { ((WalkingState)this[State.Walking]).Target = value; }
        }
        #endregion
        #region Idle State Implementation
        /// <summary>
        /// Encapsulates an Idle State for a nav agent.
        /// </summary>
        private sealed class IdleState : IStateMachineState
        {
            #region State Reference Fields
            private readonly NavMeshWrapper navWrapper;
            #endregion
            #region Constructors
            /// <summary>
            /// Creates a new idle state using the given nav wrapper.
            /// </summary>
            /// <param name="navWrapper">The wrapper for the nav agent.</param>
            public IdleState(NavMeshWrapper navWrapper)
            {
                this.navWrapper = navWrapper;
            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                // Stop moving if a previous state
                // was traveling.
                navWrapper.StopTraveling();
            }
            public void StateExited()
            {
                
            }
            #endregion
        }
        #endregion
        #region Walking State Implementation
        /// <summary>
        /// Encapsulates a Walking State for a nav agent.
        /// </summary>
        private sealed class WalkingState : IStateMachineState
        {
            #region State Reference Fields
            private readonly NavMeshWrapper navWrapper;
            private readonly TargetMoveActorFSM machine;
            #endregion
            #region State Fields
            private Vector3 target;
            #endregion
            #region Constructors
            /// <summary>
            /// Creates a new walking state with the given nav wrapper
            /// and state machine reference.
            /// </summary>
            /// <param name="navWrapper">The nav agent wrapper.</param>
            /// <param name="machine">The state machine this state exists in.</param>
            public WalkingState(NavMeshWrapper navWrapper, TargetMoveActorFSM machine)
            {
                this.navWrapper = navWrapper;
                this.machine = machine;
            }
            #endregion
            #region Behaviour Properties
            /// <summary>
            /// Sets the target for the walking state.
            /// </summary>
            public Vector3 Target
            {
                get => target;
                set
                {
                    if (value != target)
                    {
                        target = value;
                        if (machine.CurrentState == State.Walking)
                        {
                            // Only mark the agent traveling if
                            // we are currently in this state.
                            navWrapper.MarkTraveling(target);
                            navWrapper.OnDestinationReached = OnFinishedRoute;
                        }
                    }
                }
            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                navWrapper.MarkTraveling(target);
                navWrapper.OnDestinationReached = OnFinishedRoute;
            }
            public void StateExited()
            {
                navWrapper.StopTraveling();
            }
            #endregion
            #region Local Functions
            private void OnFinishedRoute()
            {
                // Return to the default state machine state.
                machine.CurrentState = machine.DefaultState;
            }
            #endregion
        }
        #endregion
    }
}
