using System;
using UnityEngine;
using UnityEngine.AI;
using EngineInterop;
using AI.Actors.StateQueueActors;
using NavMesh;
using Distributions;
using Services;
using Extensions.CSharp;

namespace AI.FastFoodActors
{
    /// <summary>
    /// Implements a fast food customer behaviour.
    /// </summary>
    public sealed class CustomerActor : StateQueueActor<CustomerActor.State>
    {
        #region State Definition
        /// <summary>
        /// The data that defines a customer's state.
        /// </summary>
        public struct State
        {
            /// <summary>
            /// The behaviour pattern for this state.
            /// </summary>
            public Behaviour behaviour;
            /// <summary>
            /// The location associated with this state.
            /// </summary>
            public Vector3 location;
        }
        /// <summary>
        /// The behaviour pattern of the customer.
        /// </summary>
        public enum Behaviour : byte
        {
            WalkingToRegister,
            WalkingToSeat,
            WalkingToExit,
            RequestingOrder,
            WaitingForFood,
            EatingFood
        }
        #endregion
        #region Events
        /// <summary>
        /// Called when this customer leaves the map.
        /// </summary>
        public event Action<CustomerActor> ExitedMap;
        #endregion
        #region Fields
        private UpdateContext updateContext;
        #endregion
        #region Inspector Fields
        [Header("Navigation Components")]
        [Tooltip("The nav mesh agent component.")]
        [SerializeField] private NavMeshAgent agent = default;
        [Tooltip("Tracks the nav mesh agent's completion state.")]
        [SerializeField] private NavMeshUpdateWrapper agentTracker = default;
        [Header("Navigation Parameters")]
        [Tooltip("The proximity tolerance to the register.")]
        [SerializeField] private float spacingFromRegister = 1f;
        [Tooltip("The proximity tolerance to the seat.")]
        [SerializeField] private float spacingFromSeat = 1f;
        [Tooltip("The proximity tolerance to the exit.")]
        [SerializeField] private float spacingFromExit = 1f;
        [Tooltip("The proximity in which the customer can grab the food from the chef.")]
        [SerializeField] private float foodGrabDistance = 1f;
        [Header("Behaviour Parameters")]
        [Tooltip("The range of durations that an order can take to make.")]
        [SerializeField] private FloatDistribution orderTime = default;
        [Tooltip("The range of durations that eating can take.")]
        [SerializeField] private FloatDistribution eatTime = default;
        private void OnValidate()
        {
            // Force these value to be non-negative.
            spacingFromRegister.LowerLimit(0f);
            spacingFromSeat.LowerLimit(0f);
            spacingFromExit.LowerLimit(0f);
            foodGrabDistance.LowerLimit(0f);
            // TODO limit method on distributions.
        }
        #endregion
        #region Intialization + Destruction
        protected override void Awake()
        {
            base.Awake();
            // Grab the update context so we can
            // put stuff on the Update() and FixedUpdate() loops.
            updateContext = ServiceManager.RetrieveService<UnityUpdateContext>();
        }
        private void OnDestroy()
        {
            // Clear any state in progress.
            updateContext.Draw -= WaitingForFoodUpdate;
            updateContext.Draw -= RequestOrderUpdate;
            updateContext.Draw -= EatingFoodUpdate;
        }
        #endregion
        #region Enter State
        protected override void EnterState(State state)
        {
            switch (state.behaviour)
            {
                case Behaviour.WalkingToRegister: WalkingToRegisterEnter(state); break;
                case Behaviour.WalkingToSeat: WalkingToSeatEnter(state); break;
                case Behaviour.WalkingToExit: WalkingToExitEnter(state); break;
                case Behaviour.RequestingOrder: RequestOrderEnter(); break;
                case Behaviour.WaitingForFood: WaitingForFoodEnter(); break;
                case Behaviour.EatingFood: EatingFoodEnter(); break;
                default: throw new NotImplementedException();
            }
        }
        #endregion
        // TODO I like the SoC here, but each state should be able to encapsulate
        // its own local data. Maybe use inner classes that conform to an interface?
        // TODO employee should remember customer, this is implemented backwards.
        private EmployeeActor cook;
        #region State: Walking to Register
        private void WalkingToRegisterEnter(State state)
        {
            // Set the agent to walk to the register.
            agent.SetDestination(state.location);
            agentTracker.tolerance = spacingFromRegister;
            agentTracker.OnDestinationReached = WalkingToRegisterExit;
            // Start watching this agent.
            agentTracker.MarkTraveling();
        }
        private void WalkingToRegisterExit()
        {
            OnStateExited();
        }
        #endregion
        #region State: Walking to Seat
        private void WalkingToSeatEnter(State state)
        {
            // Set the agent to walk to its seat.
            agent.SetDestination(state.location);
            agentTracker.tolerance = spacingFromSeat;
            agentTracker.OnDestinationReached = WalkingToSeatExit;
            // Start watching this agent.
            agentTracker.MarkTraveling();
        }
        private void WalkingToSeatExit()
        {
            OnStateExited();
        }
        #endregion
        #region State: Walking to Exit
        private void WalkingToExitEnter(State state)
        {
            // Set the agent to walk to its seat.
            agent.SetDestination(state.location);
            agentTracker.tolerance = spacingFromExit;
            agentTracker.OnDestinationReached = WalkingToExitExit;
            // Start watching this agent.
            agentTracker.MarkTraveling();
        }
        private void WalkingToExitExit()
        {
            // Notify any listeners that this actor has left.
            ExitedMap?.Invoke(this);
            Destroy(gameObject);
        }
        #endregion
        #region State: Requesting Order
        private float orderTimeElapsed;
        private float currentOrderTime;
        private void RequestOrderEnter()
        {
            orderTimeElapsed = 0f;
            currentOrderTime = orderTime.Next();
            updateContext.Draw += RequestOrderUpdate;
        }
        private void RequestOrderUpdate(float deltaTime)
        {
            // TODO add head motion look at menu.
            // Has the order time elapsed?
            orderTimeElapsed += deltaTime;
            if (orderTimeElapsed > currentOrderTime)
            {
                // Grab the nearest employee and make
                // them cook the food for this customer.
                cook = EmployeeActor.FindClosest(transform.position);
                cook.EnqueueStateInterrupt(new EmployeeActor.State
                {
                    behaviour = EmployeeActor.Behaviour.MakingFood
                });
                cook.EnqueueState(new EmployeeActor.State
                {
                    behaviour = EmployeeActor.Behaviour.DeliveringFood,
                    customer = this
                });
                // Order request completed.
                RequestOrderExit();
            }
        }
        private void RequestOrderExit()
        {
            updateContext.Draw -= RequestOrderUpdate;
            OnStateExited();
        }
        #endregion
        #region State: Waiting for Food
        private void WaitingForFoodEnter()
        {
            updateContext.Draw += WaitingForFoodUpdate;
        }
        private void WaitingForFoodUpdate(float deltaTime)
        {
            // TODO add some interesting head movement here.
            // Has the food arrived?
            if (Vector3.SqrMagnitude(transform.position - cook.transform.position) < foodGrabDistance)
                WaitingForFoodExit();
        }
        private void WaitingForFoodExit()
        {
            updateContext.Draw -= WaitingForFoodUpdate;
            OnStateExited();
        }
        #endregion
        #region State: Eating Food
        private float eatingTimeElapsed;
        private float currentEatingTime;
        private void EatingFoodEnter()
        {
            eatingTimeElapsed = 0f;
            currentEatingTime = eatTime.Next();
            updateContext.Draw += EatingFoodUpdate;
        }
        private void EatingFoodUpdate(float deltaTime)
        {
            // TODO add head motion for eating.
            // Has the eating time elapsed?
            eatingTimeElapsed += deltaTime;
            if (eatingTimeElapsed > currentEatingTime)
                EatingFoodExit();
        }
        private void EatingFoodExit()
        {
            updateContext.Draw -= EatingFoodUpdate;
            OnStateExited();
        }
        #endregion
    }
}
