using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AI_PROG_SP21.Input
{
    /// <summary>
    /// A broadcaster for two-axis joystick input.
    /// </summary>
    public sealed class StickBroadcaster : MonoBehaviour
    {
        #region Broadcasting Delegate
        /// <summary>
        /// Called whenever the stick values change.
        /// </summary>
        public Action<Vector2> StickChanged { private get; set; }
        #endregion
        #region Input Listeners
        /// <summary>
        /// Recieves new stick input from the Input Bindings.
        /// </summary>
        /// <param name="context">The input context.</param>
        public void RecieveLeftStick(InputAction.CallbackContext context)
        {
            // Notify new stick value.
            StickChanged?.Invoke(context.ReadValue<Vector2>());
        }
        #endregion
    }
}
