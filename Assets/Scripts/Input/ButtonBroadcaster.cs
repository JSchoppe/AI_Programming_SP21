using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AI_PROG_SP21.Input
{
    /// <summary>
    /// A broadcaster for a button input.
    /// </summary>
    public sealed class ButtonBroadcaster : MonoBehaviour
    {
        #region Broadcasting Delegates
        /// <summary>
        /// Called when the button is pressed down.
        /// </summary>
        public Action ButtonDown;
        /// <summary>
        /// Called when the button is released.
        /// </summary>
        public Action ButtonUp;
        #endregion
        #region Input Listeners
        /// <summary>
        /// Used by the Unity Input System to invoke button behaviour.
        /// </summary>
        /// <param name="context">The input action context.</param>
        public void RecieveInput(InputAction.CallbackContext context)
        {
            // Call the corresponding callback.
            if (context.ReadValueAsButton())
            {
                if (context.performed) ButtonUp?.Invoke();
                else ButtonDown?.Invoke();
            }
        }
        #endregion
    }
}
