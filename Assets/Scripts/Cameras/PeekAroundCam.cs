using UnityEngine;

using AI_PROG_SP21.Input;

namespace AI_PROG_SP21.Cameras
{
    /// <summary>
    /// Controls the camera transform such that it can peek in directions
    /// along the local x and y axes, driven by an input stick.
    /// </summary>
    public sealed class PeekAroundCam : MonoBehaviour
    {
        #region Input State Fields
        private Vector2 stick;
        #endregion
        #region Inspector Fields
        [Tooltip("Controls how far the camera can pan in any direction.")]
        [SerializeField] private float peekExtents = 1f;
        [Tooltip("Controls how quickly the camera snaps to the desired pan.")]
        [SerializeField] private float peekSpeed = 1f;
        [Tooltip("Provides the input for the panning.")]
        [SerializeField] private StickBroadcaster inputBroadcaster = default;
        #endregion
        #region Initialization and Destruction
        private void Awake()
        {
            inputBroadcaster.StickChanged = (Vector2 newValue) => { stick = newValue; };
        }
        private void OnDestroy()
        {
            inputBroadcaster.StickChanged = default;
        }
        #endregion
        #region Update Behaviour
        private void Update()
        {
            // Calculate the direction and movement this frame.
            Vector2 targetPosition = stick * peekExtents;
            Vector2 direction = targetPosition - (Vector2)transform.localPosition;
            float deltaPosition = peekSpeed * Time.deltaTime;
            // If we overshoot then snap.
            if (direction.magnitude < deltaPosition)
                transform.localPosition = targetPosition;
            else
            {
                // Otherwise move towards the desired target.
                direction.Normalize();
                transform.localPosition += (Vector3)(direction * deltaPosition);
            }
        }
        #endregion
    }
}
