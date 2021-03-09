using UnityEngine.InputSystem;

namespace AI_PROG_SP21.Extensions.Unity
{
    /// <summary>
    /// Provides extensions for CallbackContext.
    /// </summary>
    public static class CallbackContextExtensions
    {
        #region Button State Shorthands
        /// <summary>
        /// Returns true if the button was pressed down on this call.
        /// </summary>
        /// <param name="context">The input action context.</param>
        /// <returns>True if the button was pressed down on this call.</returns>
        public static bool ButtonPressedDown(this InputAction.CallbackContext context)
        {
            return context.ReadValueAsButton() && !context.performed;
        }
        #endregion
    }
}
