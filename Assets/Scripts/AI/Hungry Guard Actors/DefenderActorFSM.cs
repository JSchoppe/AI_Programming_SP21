using System;
using System.Collections.Generic;
using UnityEngine;

using AI_PROG_SP21.AI.Actors.FiniteStateMachines;
using AI_PROG_SP21.NavMesh;

namespace AI_PROG_SP21.AI.HungryGuardActors
{
    /// <summary>
    /// A state machine that blocks an actor from other actors.
    /// </summary>
    public sealed class DefenderActorFSM : StateMachine<DefenderActorFSM.State>
    {
        #region State Definition
        /// <summary>
        /// The states for the defender actor.
        /// </summary>
        public enum State : byte
        {
            /// <summary>
            /// The defender stands in place.
            /// </summary>
            Idle,
            /// <summary>
            /// The defender follows the actor that it is defending.
            /// </summary>
            Following,
            /// <summary>
            /// The defender blocks other actors from reaching the player.
            /// </summary>
            Blocking
        }
        #endregion
        #region State Shared Fields
        private readonly NavMeshWrapper navWrapper;
        private readonly Transform actorTransform;
        private readonly Transform actorToDefend;
        private readonly float baseRotationSpeed;
        private HungryGuardPawn[] opponentPawns;
        private Transform aggressorTransform;
        private GuardActorFSM aggressorActor;
        private Action<float> stateTick;
        private Action<object, GuardActorFSM.State> opponentStateChanged;
        #endregion
        #region Constructor + Factory
        private DefenderActorFSM(Dictionary<State, IStateMachineState> states, State defaultState,
            NavMeshWrapper navWrapper, Transform actorTransform, Transform actorToDefend)
            : base(states, defaultState)
        {
            this.navWrapper = navWrapper;
            this.actorTransform = actorTransform;
            this.actorToDefend = actorToDefend;
            baseRotationSpeed = navWrapper.RotationSpeed;
        }
        /// <summary>
        /// Creates a new state machine with the given nav wrapper and transform.
        /// </summary>
        /// <param name="navWrapper">The wrapper for the nav agent.</param>
        /// <param name="actorTransform">The transform with the nav agent.</param>
        /// <param name="actorToDefend">The transform of the actor to follow and defend.</param>
        /// <returns>The new state machine.</returns>
        public static DefenderActorFSM MakeStateMachine(NavMeshWrapper navWrapper,
            Transform actorTransform, Transform actorToDefend)
        {
            // Create the new instance.
            var states = new Dictionary<State, IStateMachineState>();
            var machine = new DefenderActorFSM(states, State.Following,
                navWrapper, actorTransform, actorToDefend);
            // Initialize all states.
            states.Add(State.Idle, new IdleState());
            states.Add(State.Following, new FollowingState(machine));
            states.Add(State.Blocking, new BlockingState(machine));
            return machine;
        }
        #endregion
        #region Exposed Properties
        /// <summary>
        /// The pawns to defend against.
        /// </summary>
        public HungryGuardPawn[] OpponentPawns
        {
            set
            {
                // If there were previous pawns
                // debind them.
                if (opponentPawns != null)
                    foreach (HungryGuardPawn pawn in opponentPawns)
                        pawn.Machine.StateChanged -= NotifyStateChanged;
                // Bind the new pawns to broadcast their state changes.
                opponentPawns = value;
                foreach (HungryGuardPawn pawn in opponentPawns)
                    pawn.Machine.StateChanged += NotifyStateChanged;
            }
        }
        // TODO this is hacky.
        private void NotifyStateChanged(object guard, GuardActorFSM.State newState)
        {
            opponentStateChanged?.Invoke(guard, newState);
        }
        #endregion
        #region Base Tick Routine
        /// <summary>
        /// Updates this state machine.
        /// </summary>
        /// <param name="deltaTime">The elapsed time from last tick.</param>
        public void Tick(float deltaTime)
        {
            // Trigger the tick in the current state.
            stateTick?.Invoke(deltaTime);
        }
        #endregion
        #region State Base Classes
        /// <summary>
        /// Base class that implements the machine accessor.
        /// </summary>
        private abstract class DefenderActorState
        {
            protected readonly DefenderActorFSM machine;
            public DefenderActorState(DefenderActorFSM machine)
            {
                this.machine = machine;
            }
        }
        #endregion
        #region Idle State Implementation
        /// <summary>
        /// Encapsulates an idle state where the actor does nothing.
        /// </summary>
        private sealed class IdleState : IStateMachineState
        {
            // TODO this is not implemented because it is unused.
            #region State Enter, State Exit
            public void StateEntered()
            {
                
            }
            public void StateExited()
            {
                
            }
            #endregion
        }
        #endregion
        #region Following State Implementation
        /// <summary>
        /// Encapsulates a following state to follow the actor to defend.
        /// </summary>
        private sealed class FollowingState : DefenderActorState, IStateMachineState
        {
            #region Local State Fields
            private float targetDistance;
            private float repositionInterval;
            private float repathTimer;
            #endregion
            #region Constructors
            /// <summary>
            /// Creates a new following state with the given state machine.
            /// </summary>
            /// <param name="machine">The machine that this state will exist in.</param>
            public FollowingState(DefenderActorFSM machine) : base(machine)
            {
                targetDistance = 2f;
                repositionInterval = 0.3f;
            }
            #endregion
            #region Opponent State Change Listener
            private void OnOpponentStateChanged(object guard, GuardActorFSM.State state)
            {
                if (guard is GuardActorFSM guardFSM)
                {
                    // Did a guard just start chasing the player?
                    if (state == GuardActorFSM.State.Chasing)
                    {
                        // Enter the blocking state.
                        machine.aggressorActor = guardFSM;
                        machine.aggressorTransform = guardFSM.GuardTransform;
                        machine.CurrentState = State.Blocking;
                    }
                }
            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                // Bind this state to the machine.
                machine.opponentStateChanged = OnOpponentStateChanged;
                machine.stateTick = Tick;
                // Reset repath timer.
                repathTimer = 0f;
                // Let nav mesh handle the rotation in this state.
                machine.navWrapper.RotationSpeed = machine.baseRotationSpeed;
            }
            public void StateExited()
            {
                // Clear bindings and state.
                machine.opponentStateChanged = default;
                machine.stateTick = default;
                machine.navWrapper.StopTraveling();
            }
            #endregion
            #region Tick Routine
            private void Tick(float deltaTime)
            {
                repathTimer += deltaTime;
                // Has enough time passed to reconsider
                // the pathing options?
                if (repathTimer > repositionInterval)
                {
                    repathTimer = 0f;
                    // Try to find a good place near the defended actor.
                    // Prefer behind the actor.
                    Vector3 target = machine.actorToDefend.position
                        - machine.actorToDefend.forward * targetDistance;
                    if (Physics.Linecast(machine.actorToDefend.position, target))
                    {
                        // If that is blocked prefer to the right or left.
                        target = machine.actorToDefend.position
                            - machine.actorToDefend.right * targetDistance;
                        if (Physics.Linecast(machine.actorToDefend.position, target))
                        {
                            target = machine.actorToDefend.position
                                + machine.actorToDefend.right * targetDistance;
                            if (Physics.Linecast(machine.actorToDefend.position, target))
                            {
                                // If right and left are blocked default to front.
                                target = machine.actorToDefend.position
                                    + machine.actorToDefend.forward * targetDistance;
                            }
                        }
                    }
                    machine.navWrapper.MarkTraveling(target);
                }
            }
            #endregion
        }
        #endregion
        #region Blocking State Implementation
        /// <summary>
        /// Encapsulates a blocking state to get between the player and opponent actor.
        /// </summary>
        private sealed class BlockingState : DefenderActorState, IStateMachineState
        {
            #region Local State Fields
            private float repositionTimer;
            private float repositionInterval;
            #endregion
            #region Constructors
            /// <summary>
            /// Creates a new blocking state with the given state machine.
            /// </summary>
            /// <param name="machine">The state machine that this will exist in.</param>
            public BlockingState(DefenderActorFSM machine) : base(machine)
            {
                // Set default values.
                repositionInterval = 0.25f;
            }
            #endregion
            #region Opponent State Change Listener
            private void OnOpponentStateChanged(object guard, GuardActorFSM.State state)
            {
                if (guard is GuardActorFSM guardFSM)
                {
                    // Did the guard stop chasing the player?
                    if (guard == machine.aggressorActor &&
                        state != GuardActorFSM.State.Chasing)
                    {
                        // Return to following state.
                        machine.CurrentState = State.Following;
                    }
                }
            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                // Bind this state to the machine.
                machine.stateTick = Tick;
                machine.opponentStateChanged = OnOpponentStateChanged;
                // Reset repath timer.
                repositionTimer = 0f;
                // Take over control of the defender rotation.
                machine.navWrapper.RotationSpeed = 0f;
            }
            public void StateExited()
            {
                // Clear bindings and state.
                machine.stateTick = default;
                machine.opponentStateChanged = default;
                machine.navWrapper.StopTraveling();
            }
            #endregion
            #region Tick Routine
            public void Tick(float deltaTime)
            {
                // Look at the aggressor.
                machine.actorTransform.LookAt(machine.aggressorTransform.position);
                // Update the repath timer.
                repositionTimer += deltaTime;
                if (repositionTimer > repositionInterval)
                {
                    repositionTimer = 0f;
                    // Place the blocker between the player and the aggressor.
                    machine.navWrapper.MarkTraveling(machine.actorToDefend.position +
                        (machine.aggressorTransform.position - machine.actorToDefend.position).normalized);
                }
            }
            #endregion
        }
        #endregion
    }
}
