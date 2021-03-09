using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using AI_PROG_SP21.Extensions.Unity;

namespace AI_PROG_SP21.Debug
{
    /// <summary>
    /// Provides a very basic script for enabling scene resets.
    /// </summary>
    public sealed class SceneReset : MonoBehaviour
    {
        /// <summary>
        /// Used by the Unity Input System to invoke on button behaviour.
        /// </summary>
        /// <param name="context">The input action context.</param>
        public void RecieveInput(InputAction.CallbackContext context)
        {
            // Verify the reset button is down.
            if (context.ButtonPressedDown())
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
