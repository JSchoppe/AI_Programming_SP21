using UnityEngine;

namespace Extensions.Unity
{
    /// <summary>
    /// Provides extensions for Transform.
    /// </summary>
    public static class TransformExtensions
    {
        #region XYZ Setters
        /// <summary>
        /// Sets the global position of X for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new X value.</param>
        public static void SetPositionX(this Transform transform, float value)
        {
            transform.position = new Vector3
            {
                x = value, y = transform.position.y, z = transform.position.z
            };
        }
        /// <summary>
        /// Sets the global position of Y for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Y value.</param>
        public static void SetPositionY(this Transform transform, float value)
        {
            transform.position = new Vector3
            {
                x = transform.position.x,
                y = value,
                z = transform.position.z
            };
        }
        /// <summary>
        /// Sets the global position of Z for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Z value.</param>
        public static void SetPositionZ(this Transform transform, float value)
        {
            transform.position = new Vector3
            {
                x = transform.position.x,
                y = transform.position.y,
                z = value
            };
        }
        /// <summary>
        /// Sets the local position of X for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new X value.</param>
        public static void SetLocalPositionX(this Transform transform, float value)
        {
            transform.localPosition = new Vector3
            {
                x = value,
                y = transform.localPosition.y,
                z = transform.localPosition.z
            };
        }
        /// <summary>
        /// Sets the local position of Y for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Y value.</param>
        public static void SetLocalPositionY(this Transform transform, float value)
        {
            transform.localPosition = new Vector3
            {
                x = transform.localPosition.x,
                y = value,
                z = transform.localPosition.z
            };
        }
        /// <summary>
        /// Sets the local position of Z for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Z value.</param>
        public static void SetLocalPositionZ(this Transform transform, float value)
        {
            transform.localPosition = new Vector3
            {
                x = transform.localPosition.x,
                y = transform.localPosition.y,
                z = value
            };
        }
        /// <summary>
        /// Sets the global euler angle X for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new X angle.</param>
        public static void SetEulerAngleX(this Transform transform, float value)
        {
            transform.eulerAngles = new Vector3
            {
                x = value,
                y = transform.eulerAngles.y,
                z = transform.eulerAngles.z
            };
        }
        /// <summary>
        /// Sets the global euler angle Y for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Y angle.</param>
        public static void SetEulerAngleY(this Transform transform, float value)
        {
            transform.eulerAngles = new Vector3
            {
                x = transform.eulerAngles.x,
                y = value,
                z = transform.eulerAngles.z
            };
        }
        /// <summary>
        /// Sets the global euler angle Z for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Z angle.</param>
        public static void SetEulerAngleZ(this Transform transform, float value)
        {
            transform.eulerAngles = new Vector3
            {
                x = transform.eulerAngles.x,
                y = transform.eulerAngles.y,
                z = value
            };
        }
        /// <summary>
        /// Sets the local euler angle X for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new X angle.</param>
        public static void SetLocalEulerAngleX(this Transform transform, float value)
        {
            transform.localEulerAngles = new Vector3
            {
                x = value,
                y = transform.localEulerAngles.y,
                z = transform.localEulerAngles.z
            };
        }
        /// <summary>
        /// Sets the local euler angle Y for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Y angle.</param>
        public static void SetLocalEulerAngleY(this Transform transform, float value)
        {
            transform.localEulerAngles = new Vector3
            {
                x = transform.localEulerAngles.x,
                y = value,
                z = transform.localEulerAngles.z
            };
        }
        /// <summary>
        /// Sets the local euler angle Z for this transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="value">The new Z angle.</param>
        public static void SetLocalEulerAngleZ(this Transform transform, float value)
        {
            transform.localEulerAngles = new Vector3
            {
                x = transform.localEulerAngles.x,
                y = transform.localEulerAngles.y,
                z = value
            };
        }
        #endregion
    }
}
