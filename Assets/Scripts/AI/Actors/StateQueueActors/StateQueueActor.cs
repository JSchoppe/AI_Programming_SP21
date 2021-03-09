using System.Collections.Generic;
using UnityEngine;

namespace AI_PROG_SP21.AI.Actors.StateQueueActors
{
    /// <summary>
    /// Base class for AI Actors that utilize an action queue.
    /// </summary>
    /// <typeparam name="TState">Type that holds the state identifying data. Typically an enum or struct.</typeparam>
    public abstract class StateQueueActor<TState> : MonoBehaviour
    {
        #region Fields
        private Queue<TState[]> stateQueue;
        // Store state for what is currently happening in the subclass.
        private bool isExecuting;
        private int executingIndex;
        private TState[] currentlyExecuting;
        // Used to skip to new state in the queue when requested.
        private bool willInterupt;
        #endregion
        #region Initialization
        protected virtual void Awake()
        {
            stateQueue = new Queue<TState[]>();
            currentlyExecuting = new TState[0];
        }
        #endregion
        #region State Queue Methods
        /// <summary>
        /// Pushes new actor state to the back of the queue.
        /// </summary>
        /// <param name="states">The new state data.</param>
        public virtual void EnqueueState(params TState[] states)
        {
            // Add the new state data and recall advance
            // state in case the actor is currently idle.
            stateQueue.Enqueue(states);
            AdvanceStateFeed();
        }
        /// <summary>
        /// Places new actor state directly after the current
        /// state has completed executing. This overwrites
        /// any other state in the queue.
        /// </summary>
        /// <param name="states">The new state data.</param>
        public virtual void EnqueueStateInterrupt(params TState[] states)
        {
            // Clear all other states and place in new state.
            stateQueue.Clear();
            stateQueue.Enqueue(states);
            // Force any remaining state in the current state array to be ignored.
            willInterupt = true;
            AdvanceStateFeed();
        }
        /// <summary>
        /// Removes all currently queued actions.
        /// </summary>
        public void ClearState()
        {
            stateQueue.Clear();
        }
        /// <summary>
        /// Implement this to react to the actor running out
        /// of state to act on.
        /// </summary>
        protected virtual void OnStatesExhausted(){ }
        #endregion
        #region State Enter/Exit Piping Methods
        /// <summary>
        /// Required implementation that tells the subclass to
        /// enter a specific state with the given state data.
        /// </summary>
        /// <param name="state">The state to enter.</param>
        protected abstract void EnterState(TState state);
        /// <summary>
        /// Call this method once the actor has finished their
        /// action with the state previously given.
        /// </summary>
        protected void OnStateExited()
        {
            // Check for new state to execute on.
            isExecuting = false;
            AdvanceStateFeed();
        }
        #endregion
        #region Queue Feeding Function
        private void AdvanceStateFeed()
        {
            // Only run when there is no state currently
            // being executed by the subclass actor.
            if (!isExecuting)
            {
                // Advance to the next state.
                executingIndex++;
                // Enter the next state if it is in the
                // array range and we have not been interrupted
                // by a new feed of state.
                if (executingIndex <= currentlyExecuting.Length - 1
                    && !willInterupt)
                {
                    isExecuting = true;
                    EnterState(currentlyExecuting[executingIndex]);
                }
                else
                {
                    willInterupt = false;
                    // Reset the index and move to the next
                    // set of state to execute on.
                    executingIndex = 0;
                    if (stateQueue.Count > 0)
                    {
                        currentlyExecuting = stateQueue.Dequeue();
                        isExecuting = true;
                        EnterState(currentlyExecuting[0]);
                    }
                    // If there is no state, reset the sentinel
                    // value of currentlyExecuting.
                    else
                    {
                        currentlyExecuting = new TState[0];
                        OnStatesExhausted();
                    }
                }
            }
        }
        #endregion
    }
}
