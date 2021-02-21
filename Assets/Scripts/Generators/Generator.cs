using UnityEngine;

namespace Generators
{
    /// <summary>
    /// Base class for generators that instantiate GameObjects.
    /// </summary>
    public abstract class Generator : MonoBehaviour
    {
        #region Inspector Fields
        [Tooltip("Whether this generator is triggered when the scene starts.")]
        [SerializeField] private bool generatesOnAwake = false;
        [Tooltip("Template that will be instantiated.")]
        [SerializeField] protected GameObject patternTemplate = null;
        #endregion
        #region Initialization
        protected virtual void Awake()
        {
            LastGeneratedObjects = new GameObject[0];
            if (generatesOnAwake)
                Generate();
        }
        #endregion
        #region Accessors
        /// <summary>
        /// Holds the collection of new objects from
        /// the latest call to Generate.
        /// </summary>
        public GameObject[] LastGeneratedObjects { get; protected set; }
        #endregion
        #region Abstract Requirements
        /// <summary>
        /// Generates instances using the pattern template.
        /// </summary>
        public abstract void Generate();
        #endregion
    }
}
