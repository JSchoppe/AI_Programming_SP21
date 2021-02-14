using UnityEngine;
using Debug.Gizmos;

namespace Cameras
{
    /// <summary>
    /// Implements a camera that follows a track to get as close to the target as possible.
    /// </summary>
    public sealed class LinearTrackCam : MonoBehaviour
    {
        #region Inspector Fields
        [Tooltip("The global space points for the camera to travel between.")]
        [SerializeField] private Vector3[] trackNodes = null;
        [Tooltip("The target transform to follow and look at.")]
        [SerializeField] private Transform target = null;
        #endregion
#if UNITY_EDITOR
        #region Gizmos Drawing
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            GizmosHelper.DrawPolyline(trackNodes);
        }
        #endregion
#endif
        // TODO this method may not be efficient.
        #region Camera Movement Implementation
        private void Update()
        {
            // Look for the closest snap distance.
            float closestSnapDistance = float.MaxValue;
            Vector3 closestSnapPosition = default;
            // Check each pair of points.
            for (int i = 1; i < trackNodes.Length; i++)
            {
                // Use projection to find the snap point.
                Vector3 snap = Vector3.Project(
                    target.position - trackNodes[i],
                    trackNodes[i - 1] - trackNodes[i]) + trackNodes[i];
                // Is the distance closer than our record?
                float sqrDistance = (target.position - snap).sqrMagnitude;
                if (sqrDistance < closestSnapDistance)
                {
                    // If so then update our record.
                    closestSnapDistance = sqrDistance;
                    closestSnapPosition = snap;
                }
            }
            // Set the camera to reflect out record.
            transform.position = closestSnapPosition;
            transform.LookAt(target);
        }
        #endregion
    }
}
