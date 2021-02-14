using UnityEngine;
using UnityEngine.InputSystem;
using Extensions.Unity;
using Extensions.CSharp;

namespace Input
{
    #region Broadcaster Listener Delegates
    /// <summary>
    /// A reciever for when an input raycast hit something.
    /// </summary>
    /// <param name="hitInfo">The raycast hit info.</param>
    public delegate void ColliderClickedListener(RaycastHit hitInfo);
    #endregion
    /// <summary>
    /// Provides an input mechanism for when the user clicks a collider in the viewport.
    /// </summary>
    public sealed class ColliderClickBroadcaster : MonoBehaviour
    {
        #region Broadcaster Events
        /// <summary>
        /// Called whenever a collider is clicked in the scene.
        /// </summary>
        public event ColliderClickedListener ColliderClicked;
        #endregion
        #region Temporary Use Fields
        private RaycastHit hit;
        #endregion
        #region Inspector Fields
        [Header("Raycast Parameters")]
        [Tooltip("The length of the raycast from the camera.")]
        [SerializeField] private float maxCheckDistance = 1f;
        [Tooltip("What to cast against.")]
        [SerializeField] private LayerMask layerMask = default;
        private void OnValidate()
        {
            maxCheckDistance.LowerLimit(0f);
        }
        #endregion
        #region Recieve Input
        /// <summary>
        /// Simulates a click input for this broadcaster.
        /// </summary>
        public void RecieveInput()
        {
            // If this is a button down event and a raycast
            // finds a suitable hit target...
            if (Physics.Raycast(
                    Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()),
                    out hit,
                    maxCheckDistance,
                    layerMask))
                // Then notify any listeners that this action has been invoked.
                ColliderClicked?.Invoke(hit);
        }
        /// <summary>
        /// Used by the Unity Input System to invoke on clicked behaviour.
        /// </summary>
        /// <param name="context">The input action context.</param>
        public void RecieveInput(InputAction.CallbackContext context)
        {
            // Verify the mouse button is down.
            if (context.ButtonPressedDown())
                RecieveInput();
        }
        #endregion
    }
}
