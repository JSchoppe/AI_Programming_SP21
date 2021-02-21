using System.Collections.Generic;
using UnityEngine;

namespace AI.Actors.GroupActors
{
    /// <summary>
    /// Base class for actors that respond to other actors of the same class.
    /// </summary>
    /// <typeparam name="T">The actor subclass type.</typeparam>
    public abstract class GroupActor<T> : MonoBehaviour
    {
        #region Fields
        private static List<GroupActor<T>> actorGroup;
        #endregion
        #region Other Actor Accessors
        /// <summary>
        /// Retrieves a list of all other actor of the same class.
        /// </summary>
        public GroupActor<T>[] OtherActors
        {
            get
            {
                List<GroupActor<T>> otherActors = new List<GroupActor<T>>();
                foreach (GroupActor<T> actor in actorGroup)
                    if (actor != this)
                        otherActors.Add(actor);
                return otherActors.ToArray();
            }
        }
        #endregion
        #region Initialization + Destruction
        protected virtual void Awake()
        {
            // Create the static collection of similar actors
            // if it has not been created yet.
            if (actorGroup == null)
                actorGroup = new List<GroupActor<T>>();
            actorGroup.Add(this);
        }
        protected virtual void OnDestroy()
        {
            actorGroup.Remove(this);
        }
        #endregion
    }
}
