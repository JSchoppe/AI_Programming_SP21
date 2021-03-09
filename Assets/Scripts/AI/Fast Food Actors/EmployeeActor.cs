using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using AI_PROG_SP21.EngineInterop;
using AI_PROG_SP21.AI.Actors.StateQueueActors;
using AI_PROG_SP21.NavMesh;
using AI_PROG_SP21.Extensions.CSharp;
using AI_PROG_SP21.Distributions;
using AI_PROG_SP21.Services;

namespace AI_PROG_SP21.AI.FastFoodActors
{
    /// <summary>
    /// Implements a fast food employee behaviour.
    /// </summary>
    public sealed class EmployeeActor : StateQueueActor<EmployeeActor.State>
    {
        #region State Definition
        /// <summary>
        /// The data that defines an employee's state.
        /// </summary>
        public sealed class State
        {
            /// <summary>
            /// The behaviour pattern for this state.
            /// </summary>
            public Behaviour behaviour;
            /// <summary>
            /// The location associated with this state.
            /// </summary>
            public Vector3 location;
            /// <summary>
            /// The customer associated with this state.
            /// </summary>
            public CustomerActor customer;
        }
        /// <summary>
        /// The behaviour pattern of the employee.
        /// </summary>
        public enum Behaviour : byte
        {
            InspectingLocation,
            MakingFood,
            DeliveringFood
        }
        #endregion
        #region Fields
        private UpdateContext updateContext;
        #endregion
        // TODO this is set manually as a hotfix.
        // Make a better manager class for this.
        public RestaurantLocations keyLocations;
        #region Inspector Fields
        [Header("Navigation Components")]
        [Tooltip("The nav mesh agent component.")]
        [SerializeField] private NavMeshAgent agent = default;
        [Tooltip("Tracks the nav mesh agent's completion state.")]
        [SerializeField] private NavMeshWrapper agentTracker = default;
        [Header("Navigation Parameters")]
        [Tooltip("The proximity tolerance for general location inspection.")]
        [SerializeField] private float spacingFromInspection = 1f;
        [Tooltip("The proximity tolerance for cooking stations.")]
        [SerializeField] private float spacingFromCookStation = 1f;
        [Tooltip("The proximity tolerance for delivering food.")]
        [SerializeField] private float spacingFromDelivery = 1f;
        [Header("Behaviour Parameters")]
        [Tooltip("The range of durations that it can take to inspect somewhere.")]
        [SerializeField] private FloatDistribution inspectionTime = default;
        [Tooltip("The range of stations it may take to make an order.")]
        [SerializeField] private IntDistribution stationCount = default;
        [Tooltip("The range of durations that it can take at each station.")]
        [SerializeField] private FloatDistribution stationPrepTime = default;
        private void OnValidate()
        {
            // Prevent invalid values from inspector.
            spacingFromInspection.LowerLimit(0f);
            spacingFromCookStation.LowerLimit(0f);
            spacingFromDelivery.LowerLimit(0f);
            // TODO implement OnValidate for distributions.
        }
        #endregion
        #region Initialization + Destruction
        private void Start()
        {
            // Retrieve the update context so we can hook
            // onto the Update() and FixedUpdate() loops.
            updateContext = ServiceManager.RetrieveService<UnityUpdateContext>();
            // Add this employee to the list of all employees.
            // TODO this should be made into a manager class.
            if (allEmployees == null)
                allEmployees = new List<EmployeeActor>();
            allEmployees.Add(this);
            // By default set the actor to roam and inspect the resaurant.
            EnqueueState(new State
            {
                behaviour = Behaviour.InspectingLocation,
                location = keyLocations.InspectionLocations.RandomElement()
            });
        }
        private void OnDestroy()
        {
            // Clean up any state in progress.
            updateContext.Draw -= InspectingLocationLookUpdate;
            updateContext.Draw -= MakingFoodStationUpdate;
            // TODO replace with manager class.
            allEmployees.Remove(this);
        }
        #endregion
        #region Find Closest Utility Method
        // TODO closest actor should be abstracted up since
        // it is a very useful method for any type of actor.
        // TODO replace with manager instead of static accessor.
        private static List<EmployeeActor> allEmployees;
        /// <summary>
        /// Finds the nearest employee actor to a location.
        /// </summary>
        /// <param name="toTarget">The target location to search in.</param>
        /// <returns>The closest employee actor.</returns>
        public static EmployeeActor FindClosest(Vector3 toTarget)
        {
            // Compare squared distance to find the closest
            // actor to the given location.
            float closestSqrDistance = float.MaxValue;
            EmployeeActor closestActor = default;
            foreach (EmployeeActor employee in allEmployees)
            {
                float sqrDistance = Vector3.SqrMagnitude(toTarget - employee.transform.position);
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestActor = employee;
                }
            }
            return closestActor;
        }
        #endregion
        #region On States Exhausted (Default Behaviour)
        protected override void OnStatesExhausted()
        {
            // If this employee has nothing to do,
            // just look busy by going somewhere.
            EnqueueState(new State
            {
                behaviour = Behaviour.InspectingLocation,
                location = keyLocations.InspectionLocations.RandomElement()
            });
        }
        #endregion
        #region Enter State
        protected override void EnterState(State state)
        {
            switch (state.behaviour)
            {
                case Behaviour.InspectingLocation: InspectingLocationEnter(state); break;
                case Behaviour.MakingFood: MakingFoodEnter(); break;
                case Behaviour.DeliveringFood: DeliveringFoodEnter(state); break;
                default: throw new NotImplementedException();
            }
        }
        #endregion
        #region State: Inspecting Location
        private void InspectingLocationEnter(State state)
        {
            // Move to the location to inspect.
            agent.destination = state.location;
            agentTracker.tolerance = spacingFromInspection;
            agentTracker.OnDestinationReached = InspectingLocationLook;
            agentTracker.MarkTraveling();
        }
        private float inspectionTimeElapsed;
        private float currentInspectionTime;
        private void InspectingLocationLook()
        {
            // Spend some time at this location.
            currentInspectionTime = inspectionTime.Next();
            inspectionTimeElapsed = 0f;
            updateContext.Draw += InspectingLocationLookUpdate;
        }
        private void InspectingLocationLookUpdate(float deltaTime)
        {
            // TODO add head look animation.
            inspectionTimeElapsed += deltaTime;
            // Check if the inspection has elapsed.
            if (inspectionTimeElapsed > currentInspectionTime)
                InspectingLocationExit();
        }
        private void InspectingLocationExit()
        {
            updateContext.Draw -= InspectingLocationLookUpdate;
            OnStateExited();
        }
        #endregion
        #region State: Making Food
        private int stationsVisited;
        private int currentStationsNeeded;
        private void MakingFoodEnter()
        {
            // Generate the parameters for this meal.
            stationsVisited = 0;
            currentStationsNeeded = stationCount.Next();
            agentTracker.tolerance = spacingFromCookStation;
            // Go to the first prep station.
            agent.destination = keyLocations.FoodPrepLocations.RandomElement();
            agentTracker.OnDestinationReached = MakingFoodArrivedAtStation;
            agentTracker.MarkTraveling();
        }
        private float prepTimeElapsed;
        private float currentPrepTime;
        private void MakingFoodArrivedAtStation()
        {
            // Spend some time at this station.
            stationsVisited++;
            prepTimeElapsed = 0f;
            currentPrepTime = stationPrepTime.Next();
            updateContext.Draw += MakingFoodStationUpdate;
        }
        private void MakingFoodStationUpdate(float deltaTime)
        {
            // TODO add prepping animation here.
            // Has the time elapsed?
            prepTimeElapsed += deltaTime;
            if (prepTimeElapsed > currentPrepTime)
            {
                // Are there more stations to visit?
                if (stationsVisited < currentStationsNeeded)
                {
                    // Go to the next station.
                    updateContext.Draw -= MakingFoodStationUpdate;
                    agent.destination = keyLocations.FoodPrepLocations.RandomElement();
                    agentTracker.OnDestinationReached = MakingFoodArrivedAtStation;
                    agentTracker.MarkTraveling();
                }
                else
                    MakingFoodExit();
            }
        }
        private void MakingFoodExit()
        {
            updateContext.Draw -= MakingFoodStationUpdate;
            OnStateExited();
        }
        #endregion
        #region State: Delivering Food
        private void DeliveringFoodEnter(State state)
        {
            // Deliver the food to the location of the
            // customer actor.
            agent.destination = state.customer.transform.position;
            agentTracker.tolerance = spacingFromDelivery;
            agentTracker.OnDestinationReached = DeliveringFoodExit;
            agentTracker.MarkTraveling();
        }
        private void DeliveringFoodExit()
        {
            OnStateExited();
        }
        #endregion
    }
}
