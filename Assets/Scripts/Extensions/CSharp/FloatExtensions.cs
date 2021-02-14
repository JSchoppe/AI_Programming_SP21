namespace Extensions.CSharp
{
    /// <summary>
    /// Provides extension methods for the float data type.
    /// </summary>
    public static class FloatExtensions
    {
        #region Clamp In Place
        /// <summary>
        /// Clamps a float in place.
        /// </summary>
        /// <param name="value">The floating point value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public static void Limit(this ref float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
        }
        /// <summary>
        /// Clamps a float from the left.
        /// </summary>
        /// <param name="value">The floating point value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        public static void LowerLimit(this ref float value, float min)
        {
            if (value < min)
                value = min;
        }
        /// <summary>
        /// Clamps a float from the right.
        /// </summary>
        /// <param name="value">The floating point value to clamp.</param>
        /// <param name="max">The maximum value.</param>
        public static void UpperLimit(this ref float value, float max)
        {
            if (value > max)
                value = max;
        }
        #endregion
    }
}
