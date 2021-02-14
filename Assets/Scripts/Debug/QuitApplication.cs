using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Extensions.Unity;

namespace Debug
{
    /// <summary>
    /// Provides a very basic script for enabling exit input.
    /// </summary>
    public sealed class QuitApplication : MonoBehaviour
    {
        #region JavaScript Interface
        [DllImport("__Internal")]
        private static extern void Quit();
        #endregion
        /// <summary>
        /// Used by the Unity Input System to invoke on button behaviour.
        /// </summary>
        /// <param name="context">The input action context.</param>
        public void RecieveInput(InputAction.CallbackContext context)
        {
            // Verify the quit button is down.
            if (context.ButtonPressedDown())
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WebGLPlayer: Quit(); break;
                    default: Application.Quit(); break;
                }
            }
        }
    }
}
