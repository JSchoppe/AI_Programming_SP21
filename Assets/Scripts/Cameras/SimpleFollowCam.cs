using UnityEngine;

namespace AI_PROG_SP21.Cameras
{
    /// <summary>
    /// A camera that is positioned relative to another
    /// transform but does not inherit rotation or scale.
    /// </summary>
    public sealed class SimpleFollowCam : MonoBehaviour
    {
        #region Fields
        private Vector3 boomOffset;
        #endregion
        #region Inspector Fields
        [Tooltip("The object to stay relative to position wise.")]
        [SerializeField] private Transform target = default;
        #endregion
        #region Initialization
        private void Awake()
        {
            // Get the local offset.
            boomOffset = transform.position - target.position;
        }
        #endregion
        #region Update Behaviour
        private void Update()
        {
            // Keep the camera aligned to the initial offset.
            transform.position = target.position + boomOffset;
        }
        #endregion
    }
}
