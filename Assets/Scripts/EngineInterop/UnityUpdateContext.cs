using System;
using UnityEngine;

namespace AI_PROG_SP21.EngineInterop
{
    /// <summary>
    /// Exposes the update loops in Unity.
    /// </summary>
    public sealed class UnityUpdateContext : UpdateContext
    {
        #region Singleton Field
        private static UpdateSingleton singleton;
        #endregion
        // TODO this is not an enforced singleton.
        #region Constructor
        /// <summary>
        /// Creates a new instance of the Unity Update Context.
        /// </summary>
        public UnityUpdateContext()
        {
            // Create the singleton if it doesn't exist yet.
            if (singleton is null)
            {
                GameObject singletonHost = new GameObject();
                UnityEngine.Object.DontDestroyOnLoad(singletonHost);
                singletonHost.name = "UPDATE_SINGLETON";
                singleton = singletonHost.AddComponent<UpdateSingleton>();
            }
        }
        #endregion
        #region UpdateContext Events
        public override event Action<float> Draw
        {
            add
            {
                singleton.OnUpdate += value;
            }
            remove
            {
                singleton.OnUpdate -= value;
            }
        }
        public override event Action<float> FixedStep
        {
            add
            {
                singleton.OnFixedUpdate += value;
            }
            remove
            {
                singleton.OnFixedUpdate -= value;
            }
        }
        #endregion
        #region Singleton Definition
        // In unity a MonoBehaviour is instantiated and other
        // desired lightweight behaviours can hijack the
        // update loops.
        private sealed class UpdateSingleton : MonoBehaviour
        {
            public event Action<float> OnUpdate;
            public event Action<float> OnFixedUpdate;
            private void Update()
            {
                OnUpdate?.Invoke(Time.deltaTime);
            }
            private void FixedUpdate()
            {
                OnFixedUpdate?.Invoke(Time.fixedDeltaTime);
            }
        }
        #endregion
    }
}
