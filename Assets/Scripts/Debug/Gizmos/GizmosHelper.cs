using UnityEngine;

namespace AI_PROG_SP21.Debug.Gizmos
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
        // TODO this method has not been tested.
        #region DrawArrow
        /// <summary>
        /// Draws an arrow using the current gizmo appearence.
        /// </summary>
        /// <param name="start">The start of the arrow.</param>
        /// <param name="end">The end of the arrow with arrow head.</param>
        /// <param name="arrowWidth">The width of the arrow head.</param>
        public static void DrawArrow(Vector3 start, Vector3 end, float arrowWidth)
        {
            if (start != end)
            {
                Vector3 stepBack = (end - start).normalized * -arrowWidth;
                Vector3 stepSide = Vector3.Cross(end - start, Vector3.up).normalized * arrowWidth;
                UnityEngine.Gizmos.DrawLine(start, end);
                UnityEngine.Gizmos.DrawLine(end, end + stepBack + stepSide);
                UnityEngine.Gizmos.DrawLine(end, end + stepBack - stepSide);
            }
        }
        /// <summary>
        /// Draws an arrow using the current gizmo appearence.
        /// </summary>
        /// <param name="start">The start of the arrow.</param>
        /// <param name="end">The end of the arrow with arrow head.</param>
        public static void DrawArrow(Vector3 start, Vector3 end)
        {
            DrawArrow(start, end, 0.25f);
        }
        #endregion
        #region Draw Empty
        /// <summary>
        /// Draws an empty using the current gizmo appearence.
        /// </summary>
        /// <param name="point">The location of the empty.</param>
        /// <param name="radius">The radius of the gizmo.</param>
        public static void DrawEmpty(Vector3 point, float radius)
        {
            UnityEngine.Gizmos.DrawLine(
                point + Vector3.left * radius,
                point + Vector3.right * radius);
            UnityEngine.Gizmos.DrawLine(
                point + Vector3.down * radius,
                point + Vector3.up * radius);
            UnityEngine.Gizmos.DrawLine(
                point + Vector3.back * radius,
                point + Vector3.forward * radius);
        }
        /// <summary>
        /// Draws an empty using the current gizmo appearence.
        /// </summary>
        /// <param name="point">The location of the empty.</param>
        public static void DrawEmpty(Vector3 point)
        {
            DrawEmpty(point, 0.25f);
        }
        #endregion
    }
#endif
}
