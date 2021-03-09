using UnityEngine;
using UnityEngine.InputSystem;

namespace AI_PROG_SP21.Cameras
{
    /// <summary>
    /// Replicates the free cam editor behaviour for runtime.
    /// </summary>
    public sealed class FreeCam : MonoBehaviour
    {
        #region Input State Fields
        private Vector2 leftStickInput;
        private Vector2 rightStickInput;
        private float eulerX;
        #endregion
        #region Inspector Fields
        [Tooltip("The sensitivity for the camera movement. Relative to delta time.")]
        [SerializeField] private Vector2 moveSensitivity = Vector2.one;
        [Tooltip("The sensitivity for the camera rotation. Relative to screen height.")]
        [SerializeField] private Vector2 lookSensitivity = Vector2.one;
        [Tooltip("The elevation angle limit for the camera.")]
        [Range(0f, 89f)][SerializeField] private float verticalLookLimit = 89f;
        #endregion
        #region MonoBehaviour
        private void Awake()
        {
            eulerX = transform.eulerAngles.x;
        }
        private void Update()
        {
            // Move the camera along the transform right
            // and forward axes using the user input.
            transform.position +=
                transform.right * leftStickInput.x * moveSensitivity.x * Time.deltaTime +
                transform.forward * leftStickInput.y * moveSensitivity.y * Time.deltaTime;
            // If the right mouse button is pressed,
            // pivot the camera based on mouse movement.
            if (Mouse.current.rightButton.isPressed)
            {
                eulerX -= rightStickInput.y * lookSensitivity.y;
                eulerX = Mathf.Clamp(eulerX, -verticalLookLimit, verticalLookLimit);
                transform.eulerAngles = new Vector3
                {
                    x = eulerX,
                    y = transform.eulerAngles.y + rightStickInput.x * lookSensitivity.x,
                    z = 0f
                };
            }
        }
        #endregion
        #region Input Listeners
        /// <summary>
        /// Recieves input from the new input system for camera movement.
        /// </summary>
        /// <param name="context">The input system context.</param>
        public void RecieveLeftStick(InputAction.CallbackContext context)
        {
            leftStickInput = context.ReadValue<Vector2>();
        }
        /// <summary>
        /// Recieves input from the new input system for camera rotation.
        /// </summary>
        /// <param name="context">The input system context.</param>
        public void RecieveRightStick(InputAction.CallbackContext context)
        {
            rightStickInput = context.ReadValue<Vector2>() / Screen.height;
        }
        #endregion
    }
}
