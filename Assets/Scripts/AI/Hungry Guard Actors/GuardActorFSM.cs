using System;
using System.Collections.Generic;
using UnityEngine;

using AI_PROG_SP21.AI.Actors.FiniteStateMachines;
using AI_PROG_SP21.NavMesh;
using AI_PROG_SP21.Extensions.CSharp;
using AI_PROG_SP21.Distributions;

namespace AI_PROG_SP21.AI.HungryGuardActors
{
    /// <summary>
    /// A state machine for a chasing guard.
    /// </summary>
    public sealed class GuardActorFSM : StateMachine<GuardActorFSM.State>
    {
        #region State Definition
        /// <summary>
        /// The states for the guard actor.
        /// </summary>
        public enum State : byte
        {
            /// <summary>
            /// Guard is standing in place.
            /// </summary>
            Idle,
            /// <summary>
            /// Guard is patrolling randomly.
            /// </summary>
            Patrolling,
            /// <summary>
            /// Guard is investigating points of interest.
            /// </summary>
            Investigating,
            /// <summary>
            /// Guard is chasing another actor.
            /// </summary>
            Chasing
        }
        #endregion
        #region State Shared Fields
        private readonly Transform guardTransform;
        private readonly Transform[] suspiciousTransforms;
        private readonly Stack<Vector3> suspicionPoints;
        private readonly NavMeshWrapper navWrapper;
        private float baseSpeed;
        private Transform sightedActor;
        private Action<float> stateTick;
        #endregion
        #region State Shared Broadcasters
        private event Action BaseSpeedChanged;
        #endregion
        #region Constructor + Factory
        private GuardActorFSM(Dictionary<State, IStateMachineState> states, State defaultState,
            NavMeshWrapper navWrapper, Transform guardTransform, Transform[] suspiciousTransforms)
            : base(states, defaultState)
        {
            // Expose readonly fields for the states to access.
            this.navWrapper = navWrapper;
            this.guardTransform = guardTransform;
            this.suspiciousTransforms = suspiciousTransforms;
            suspicionPoints = new Stack<Vector3>();
            // Set default values.
            baseSpeed = 1f;
        }
        /// <summary>
        /// Creates a new state machine with the given objects.
        /// </summary>
        /// <param name="navWrapper">Wrapper for the nav mesh agent.</param>
        /// <param name="actorTransform">The transform of the guard actor.</param>
        /// <param name="suspiciousTransforms">The transforms marked to chase.</param>
        /// <returns>The new state machine.</returns>
        public static GuardActorFSM MakeStateMachine(NavMeshWrapper navWrapper, Transform actorTransform, Transform[] suspiciousTransforms)
        {
            // Create the new instance.
            var states = new Dictionary<State, IStateMachineState>();
            GuardActorFSM machine = new GuardActorFSM(states, State.Patrolling,
                navWrapper, actorTransform, suspiciousTransforms);
            // Initialize all states.
            states.Add(State.Idle, new IdleState(machine));
            states.Add(State.Patrolling, new PatrollingState(machine)
            {
                // TODO these should be exposed to be set outside
                // the state machine. Here they are just magic numbers.
                PauseLookBetweenDuration = new FloatDistribution { min = 7.0f, max = 15.0f },
                LookAroundPauseDuration = new FloatDistribution { min = 1.0f, max = 1.7f },
                LookAroundDuration = new FloatDistribution { min = 4.0f, max = 7.0f },
                LookAroundAngle = new FloatDistribution { min = 20.0f, max = 30.0f },
                LookAroundSpeed = new FloatDistribution { min = 1.0f, max = 1.3f }
            });
            states.Add(State.Investigating, new InvestigatingState(machine));
            states.Add(State.Chasing, new ChasingState(machine));
            return machine;
        }
        #endregion
        #region Exposed Events
        /// <summary>
        /// Called when a transform is caught.
        /// </summary>
        public Action CaughtTransform { set; private get; }
        #endregion
        #region Exposed Properties
        /// <summary>
        /// The transform being controlled by this state machine.
        /// </summary>
        public Transform GuardTransform => guardTransform;
        /// <summary>
        /// The patrol points that this guard moves randomly between.
        /// </summary>
        public Transform[] PatrolPoints { get; set; }
        /// <summary>
        /// The base navmesh speed of this actor.
        /// </summary>
        public float BaseSpeed
        {
            get => baseSpeed;
            set
            {
                if (value != baseSpeed)
                {
                    baseSpeed = value;
                    BaseSpeedChanged?.Invoke();
                }
            }
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
        #region Common Check Actor Method
        /// <summary>
        /// Checks for suspicious actors, changes
        /// to chasing state if sighted.
        /// </summary>
        private void CheckSuspiciousActors()
        {
            // Check if each actor is within a cone angle
            // of this actor's forwards transform and
            // that there is an unblocked line of sight.
            foreach (Transform actor in suspiciousTransforms)
            {
                if (Vector3.Angle(guardTransform.forward, actor.position - guardTransform.position) < 30f)
                {
                    if (!Physics.Linecast(guardTransform.position, actor.position))
                    {
                        sightedActor = actor;
                        CurrentState = State.Chasing;
                        break;
                    }
                }
            }
        }
        #endregion
        #region State Base Classes
        /// <summary>
        /// Base class that implements the machine accessor.
        /// </summary>
        private abstract class GuardActorState
        {
            protected readonly GuardActorFSM machine;
            public GuardActorState(GuardActorFSM machine)
            {
                this.machine = machine;
            }
        }
        /// <summary>
        /// Base class that implements speed changed listener.
        /// </summary>
        private abstract class MovementState : GuardActorState
        {
            public MovementState(GuardActorFSM machine) : base(machine)
            {
                machine.BaseSpeedChanged += OnSpeedChanged;
            }
            protected abstract void OnSpeedChanged();
        }
        #endregion
        #region Idle State Implementation
        /// <summary>
        /// Encapsulates an idle state where the guard is not moving.
        /// </summary>
        private sealed class IdleState : GuardActorState, IStateMachineState
        {
            #region Constructors
            /// <summary>
            /// Creates a new idle state with the given state machine.
            /// </summary>
            /// <param name="machine">The holding state machine.</param>
            public IdleState(GuardActorFSM machine) : base(machine)
            {

            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                // Stop any navigation.
                machine.navWrapper.StopTraveling();
            }
            public void StateExited()
            {

            }
            #endregion
        }
        #endregion
        #region Investigating State Implementation
        /// <summary>
        /// Encapsulates an investigating state where the guard is investigating suspicion points.
        /// </summary>
        private sealed class InvestigatingState : MovementState, IStateMachineState
        {
            #region Constructors
            /// <summary>
            /// Creates a new investigating state with the given state machine.
            /// </summary>
            /// <param name="machine">The holding state machine.</param>
            public InvestigatingState(GuardActorFSM machine) : base(machine)
            {
                
            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                // Link up callbacks.
                machine.navWrapper.OnDestinationReached = InvestigationPointReached;
                machine.stateTick = Tick;
                // Refresh the speed of the agent.
                OnSpeedChanged();
                // Pull the latest investigation point.
                InvestigationPointReached();
            }
            public void StateExited()
            {
                // Clear suspicion memory and stop ticking.
                machine.suspicionPoints.Clear();
                machine.stateTick = default;
            }
            #endregion
            #region Speed Changed Listener
            protected override void OnSpeedChanged()
            {
                // If we are in this state react by changing
                // the agent speed.
                if (machine.CurrentState == State.Investigating)
                    machine.navWrapper.Speed = machine.baseSpeed;
            }
            #endregion
            #region Nav Arrived Listener
            private void InvestigationPointReached()
            {
                // Is there another point to investigate?
                // If not then revert to patrolling state.
                if (machine.suspicionPoints.Count > 0)
                    machine.navWrapper.MarkTraveling(machine.suspicionPoints.Pop());
                else
                    machine.CurrentState = State.Patrolling;
            }
            #endregion
            #region Tick Routine
            private void Tick(float deltaTime)
            {
                // Check if there are any suspicious actors
                // in the field of view.
                machine.CheckSuspiciousActors();
            }
            #endregion
        }
        #endregion
        #region Chasing State Implementation
        /// <summary>
        /// Encapsulates a chasing state where the guard is chasing after an actor.
        /// </summary>
        private sealed class ChasingState : MovementState, IStateMachineState
        {
            #region Local State Fields
            private float chasingSpeedCoefficient;
            private float retargetTimer;
            #endregion
            #region Constructors
            /// <summary>
            /// Creates a new investigating state with the given state machine.
            /// </summary>
            /// <param name="machine">The holding state machine.</param>
            public ChasingState(GuardActorFSM machine) : base(machine)
            {
                // Set some default values.
                // TODO specific values should not be set here.
                RetargetInterval = 0.4f;
                chasingSpeedCoefficient = 3f;
            }
            #endregion
            #region Behaviour Properties
            /// <summary>
            /// Multiplied by base speed to determine chasing speed.
            /// </summary>
            public float ChasingSpeedCoefficient
            {
                get => chasingSpeedCoefficient;
                set
                {
                    if (value != chasingSpeedCoefficient)
                    {
                        chasingSpeedCoefficient = value;
                        OnSpeedChanged();
                    }
                }
            }
            /// <summary>
            /// The number of seconds between each event
            /// where repathing and is executed.
            /// </summary>
            public float RetargetInterval { get; set; }
            #endregion
            #region Speed Changed Listener
            protected override void OnSpeedChanged()
            {
                // If we are currently in this state update
                // the nav agent's speed.
                if (machine.CurrentState == State.Chasing)
                    machine.navWrapper.Speed = machine.baseSpeed * chasingSpeedCoefficient;
            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                // Travel towards the sighted actor.
                machine.navWrapper.MarkTraveling(machine.sightedActor.position);
                // Bind to tick and reset timer.
                machine.stateTick = Tick;
                retargetTimer = 0f;
                // Refresh the speed of the agent.
                OnSpeedChanged();
            }
            public void StateExited()
            {
                // Clear navigation and tick state.
                machine.navWrapper.StopTraveling();
                machine.stateTick = default;
            }
            #endregion
            #region Tick Routine
            private void Tick(float deltaTime)
            {
                retargetTimer += deltaTime;
                // Is the player within range? If so notify listeners.
                // TODO magic number 2f should be prop catch distance.
                if (Vector3.SqrMagnitude(machine.guardTransform.position - machine.sightedActor.position) < 2f)
                    machine.CaughtTransform?.Invoke();
                // Has enough time passed to recalculate routing?
                if (retargetTimer > RetargetInterval)
                {
                    retargetTimer = 0f;
                    // Is the target still visible?
                    if (Physics.Linecast(machine.guardTransform.position, machine.sightedActor.position))
                    {
                        // If blocked change to investigate the last sighted point.
                        machine.suspicionPoints.Push(machine.sightedActor.position);
                        machine.CurrentState = State.Investigating;
                    }
                    else machine.navWrapper.MarkTraveling(machine.sightedActor.position);
                }
            }
            #endregion
        }
        #endregion
        #region Patrolling State Implementation
        /// <summary>
        /// Encapsulates a patrolling state where the guard randomly walks between patrol points.
        /// </summary>
        private sealed class PatrollingState : MovementState, IStateMachineState
        {
            // TODO maybe instead of enum sub state
            // have a state machine instance.
            #region SubState Definition
            private enum LookCycleState : byte
            {
                Walking,
                Pausing,
                Looking
            }
            #endregion
            #region Local State Fields
            private LookCycleState lookCycleState;
            private float lookTimer;
            private float startLookAngle;
            private float lookTime, lookPauseDuration, lookDuration, lookAngle, lookSpeed;
            #endregion
            #region Constructors
            /// <summary>
            /// Creates a new patrolling state with the given state machine.
            /// </summary>
            /// <param name="machine">The holding state machine.</param>
            public PatrollingState(GuardActorFSM machine) : base(machine)
            {
                
            }
            #endregion
            #region Behaviour Properties
            /// <summary>
            /// The seconds between each pause to look around.
            /// </summary>
            public FloatDistribution PauseLookBetweenDuration { get; set; }
            /// <summary>
            /// The seconds before the guard's vision starts panning left to right.
            /// </summary>
            public FloatDistribution LookAroundPauseDuration { get; set; }
            /// <summary>
            /// The seconds that the guard's vision pans left to right.
            /// </summary>
            public FloatDistribution LookAroundDuration { get; set; }
            /// <summary>
            /// The angle magnitude that the guard's vision extends to.
            /// </summary>
            public FloatDistribution LookAroundAngle { get; set; }
            /// <summary>
            /// The speed at which the guard pans its vision.
            /// </summary>
            public FloatDistribution LookAroundSpeed { get; set; }
            #endregion
            #region Speed Changed Listener
            protected override void OnSpeedChanged()
            {
                // If we are in this state update the
                // speed of the agent.
                if (machine.CurrentState == State.Investigating)
                    machine.navWrapper.Speed = machine.baseSpeed;
            }
            #endregion
            #region Nav Arrived Listener
            private void GotoNewWaypoint()
            {
                // Randomly choose a new waypoint to approach.
                machine.navWrapper.OnDestinationReached = GotoNewWaypoint;
                machine.navWrapper.MarkTraveling(machine.PatrolPoints.RandomElement().position);
            }
            #endregion
            #region Generate State Function
            private void ResetLookCycle()
            {
                // Reset look cycle.
                lookCycleState = LookCycleState.Walking;
                lookTimer = 0f;
                // Generate a new set of parameters for
                // the next look pan cycle.
                lookTime = PauseLookBetweenDuration.Next();
                lookPauseDuration = LookAroundPauseDuration.Next();
                lookDuration = LookAroundDuration.Next();
                lookAngle = LookAroundAngle.Next();
                lookSpeed = LookAroundSpeed.Next();
            }
            #endregion
            #region State Enter, State Exit
            public void StateEntered()
            {
                // Bind to tick.
                machine.stateTick = Tick;
                // Initialize state.
                ResetLookCycle();
                OnSpeedChanged();
                // Pick a new waypoint to go to.
                GotoNewWaypoint();
            }
            public void StateExited()
            {
                // Stop traveling and clear state.
                machine.navWrapper.StopTraveling();
                machine.navWrapper.OnDestinationReached = default;
                machine.stateTick = default;
            }
            #endregion
            #region Tick Routine
            // TODO reimplement this as its own state machine.
            // This will make it more reusable.
            private void Tick(float deltaTime)
            {
                // Check for nearby actors.
                machine.CheckSuspiciousActors();
                // Advance substate.
                lookTimer += deltaTime;
                switch (lookCycleState)
                {
                    case LookCycleState.Walking: WalkingTick(); break;
                    case LookCycleState.Pausing: PausingTick(); break;
                    case LookCycleState.Looking: LookingTick(); break;
                    default: throw new NotImplementedException();
                }
                void WalkingTick()
                {
                    if (lookTimer > lookTime)
                    {
                        lookTimer = 0f;
                        // Pause the navmesh.
                        machine.navWrapper.IsPaused = true;
                        lookCycleState = LookCycleState.Pausing;
                    }
                }
                void PausingTick()
                {
                    if (lookTimer > lookPauseDuration)
                    {
                        lookTimer = 0f;
                        // Advance to panning the scene.
                        startLookAngle = machine.guardTransform.eulerAngles.y;
                        lookCycleState = LookCycleState.Looking;
                    }
                }
                void LookingTick()
                {
                    if (lookTimer > lookDuration)
                    {
                        ResetLookCycle();
                        // Resume the navmesh.
                        machine.navWrapper.IsPaused = false;
                    }
                    else
                    {
                        // Pan back and forth on the y euler axis.
                        machine.guardTransform.eulerAngles = Vector3.up *
                            (startLookAngle + Mathf.Sin(lookTimer * lookSpeed) * lookAngle);
                    }
                }
            }
            #endregion
        }
        #endregion
    }
}
