using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AI_PROG_SP21.AI.FastFoodActors;
using AI_PROG_SP21.Distributions;
using AI_PROG_SP21.Extensions.CSharp;

namespace AI_PROG_SP21.Scenes
{
    // TODO much of this code should be abstracted into a manager class.
    // This contains the hard coded scene logic.
    /// <summary>
    /// Initializes the scene for the chapter 4 demo.
    /// </summary>
    public sealed class Chapter4Base_Bootstrapper : MonoBehaviour
    {
        #region Fields
        private int currentCustomerCount;
        private bool[] seatTaken;
        private Dictionary<CustomerActor, int> assignedSeats;
        #endregion
        #region Inspector Fields
        // TODO add tooltips.
        [Header("Scene Context")]
        [SerializeField] private Transform[] seatLocations = default;
        [SerializeField] private Transform[] registerLocations = default;
        [SerializeField] private Transform[] spawnLocations = default;
        [SerializeField] private RestaurantLocations locations = default;
        [Header("Actor Templates")]
        [SerializeField] private GameObject employeeTemplate = default;
        [SerializeField] private GameObject customerTemplate = default;
        [Header("Spawning Parameters")]
        [SerializeField] private int employees = 1;
        [SerializeField] private FloatDistribution spawnEmployeeSeconds = default;
        [SerializeField] private int maxCustomers = 1;
        [SerializeField] private FloatDistribution spawnCustomerSeconds = default;
        #endregion
        #region Initialization
        private void Awake()
        {
            seatTaken = new bool[seatLocations.Length];
            assignedSeats = new Dictionary<CustomerActor, int>();
            // Start spawning the employees.
            StartCoroutine(SpawnEmployees());
        }
        #endregion
        #region Seat Management Methods
        // TODO this should be added directly to
        // the customer AI routine.
        private int GetRandomFreeSeat()
        {
            // Grab every seat that is free
            // into a list.
            List<int> freeSeats = new List<int>();
            for (int i = 0; i < seatTaken.Length; i++)
                if (seatTaken[i] == false)
                    freeSeats.Add(i);
            // Return a random one of those free seats.
            return freeSeats.RandomElement();
        }
        #endregion
        #region Spawn Actors
        private IEnumerator SpawnEmployees()
        {
            for (int i = 0; i < employees; i++)
            {
                // Get a random spawn point and instantiate
                // a new employee actor.
                Transform spawnPoint = spawnLocations.RandomElement();
                EmployeeActor newEmployee = Instantiate(
                    employeeTemplate,
                    spawnPoint.position,
                    spawnPoint.rotation
                    ).GetComponent<EmployeeActor>();
                // TODO having to set this here is a hotfix.
                newEmployee.keyLocations = locations;
                yield return new WaitForSeconds(spawnEmployeeSeconds.Next());
            }
            // Start spawning the customers.
            StartCoroutine(CheckSpawnCustomer());
        }
        private IEnumerator CheckSpawnCustomer()
        {
            // Keep spawning employees (forever).
            while (true)
            {
                yield return new WaitForSeconds(spawnCustomerSeconds.Next());
                // If there is space:
                if (currentCustomerCount < maxCustomers)
                {
                    currentCustomerCount++;
                    // Take a seat for this actor.
                    int seatIndex = GetRandomFreeSeat();
                    seatTaken[seatIndex] = true;
                    // Spawn the customer actor.
                    Transform spawnPoint = spawnLocations.RandomElement();
                    CustomerActor newCustomer = Instantiate(
                        customerTemplate,
                        spawnPoint.position,
                        spawnPoint.rotation
                        ).GetComponent<CustomerActor>();
                    // Assign the actor to its seat.
                    // This is used to free the seat on exit.
                    assignedSeats[newCustomer] = seatIndex;
                    // Enqueue the entire behaviour pattern
                    // for the customer (current implementation does not deviate).
                    newCustomer.EnqueueState(
                        new CustomerActor.State
                        {
                            behaviour = CustomerActor.Behaviour.WalkingToRegister,
                            location = registerLocations.RandomElement().position
                        });
                    newCustomer.EnqueueState(
                        new CustomerActor.State
                        {
                            behaviour = CustomerActor.Behaviour.RequestingOrder
                        });
                    newCustomer.EnqueueState(
                        new CustomerActor.State
                        {
                            behaviour = CustomerActor.Behaviour.WalkingToSeat,
                            location = seatLocations[seatIndex].position
                        });
                    newCustomer.EnqueueState(
                        new CustomerActor.State
                        {
                            behaviour = CustomerActor.Behaviour.WaitingForFood
                        });
                    newCustomer.EnqueueState(
                        new CustomerActor.State
                        {
                            behaviour = CustomerActor.Behaviour.EatingFood
                        });
                    newCustomer.EnqueueState(
                        new CustomerActor.State
                        {
                            behaviour = CustomerActor.Behaviour.WalkingToExit,
                            location = spawnPoint.position
                        });
                    // Listen for the actor to leave.
                    newCustomer.ExitedMap += OnCustomerExitedMap;
                }
            }
        }
        #endregion
        #region Customer Exited Listener
        private void OnCustomerExitedMap(CustomerActor customer)
        {
            currentCustomerCount--;
            // Free the seat that was taken by the actor.
            seatTaken[assignedSeats[customer]] = false;
            assignedSeats.Remove(customer);
        }
        #endregion
    }
}
