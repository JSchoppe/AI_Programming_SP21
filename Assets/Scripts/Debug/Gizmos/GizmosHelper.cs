using UnityEngine;

namespace Debug.Gizmos
{
#if UNITY_EDITOR
    /// <summary>
    /// Provides helper methods for drawing gizmos.
    /// </summary>
    public static class GizmosHelper
    {
        #region Draw Polyline
        /// <summary>
        /// Draws a polyline from the given points using the current gizmo appearence.
        /// </summary>
        /// <param name="points">The points to connect in global space.</param>
        /// <param name="doesLoop">Whether the last point connects back to the first.</param>
        public static void DrawPolyline(Vector3[] points, bool doesLoop)
        {
            if (points != null && points.Length > 1)
            {
                for (int i = 1; i < points.Length; i++)
                    UnityEngine.Gizmos.DrawLine(points[i], points[i - 1]);
                if (doesLoop)
                    UnityEngine.Gizmos.DrawLine(points[points.Length - 1], points[0]);
            }
        }
        /// <summary>
        /// Draws a polyline from the given points using the current gizmo appearence.
        /// </summary>
        /// <param name="points">The points to connect in global space.</param>
        public static void DrawPolyline(Vector3[] points)
        {
            DrawPolyline(points, false);
        }
        #endregion
    }
#endif
}
