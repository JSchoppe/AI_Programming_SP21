using System;
using System.Collections.Generic;

namespace AI_PROG_SP21.AI.Actors.FiniteStateMachines
{
    /// <summary>
    /// Base class for finite state machines.
    /// </summary>
    /// <typeparam name="TStateKey">The key used to identify the state.</typeparam>
    public abstract class StateMachine<TStateKey>
    {
        #region State Fields
        private readonly Dictionary<TStateKey, IStateMachineState> states;
        private TStateKey currentState, defaultState;
        #endregion
        #region Constructors
        /// <summary>
        /// Creates a new state machine with the given states and default state.
        /// </summary>
        /// <param name="states">The states accessible to this machine.</param>
        /// <param name="defaultState">The default state for this machine.</param>
        public StateMachine(Dictionary<TStateKey, IStateMachineState> states, TStateKey defaultState)
        {
            this.states = states;
            this.defaultState = defaultState;
        }
        #endregion
        #region State Events
        /// <summary>
        /// Called every time this machine changes state.
        /// Sends the new state and the machine.
        /// </summary>
        public event Action<StateMachine<TStateKey>, TStateKey> StateChanged;
        #endregion
        #region State Properties
        /// <summary>
        /// The state that the machine is currently in.
        /// </summary>
        public TStateKey CurrentState
        {
            get => currentState;
            set
            {
                // Is this a new state?
                if (!currentState.Equals(value))
                {
                    // Exit current state
                    states[currentState].StateExited();
                    currentState = value;
                    // Enter new state.
                    states[currentState].StateEntered();
                    // Notify listeners.
                    StateChanged?.Invoke(this, currentState);
                }
            }
        }
        /// <summary>
        /// The default state for the machine.
        /// </summary>
        public TStateKey DefaultState
        {
            get => defaultState;
            set
            {
                // Is this a new default state?
                if(!defaultState.Equals(value))
                {
                    // Were we in the default state?
                    if (currentState.Equals(defaultState))
                    {
                        // Change to the new default state.
                        states[currentState].StateExited();
                        states[value].StateEntered();
                    }
                    defaultState = value;
                }
            }
        }
        #endregion
        #region Protected State Accessor
        /// <summary>
        /// Retrieves a state with the given state key.
        /// </summary>
        /// <param name="key">The state key.</param>
        /// <returns>The IStateMachine state object.</returns>
        protected IStateMachineState this[TStateKey key] => states[key];
        #endregion
    }
}
