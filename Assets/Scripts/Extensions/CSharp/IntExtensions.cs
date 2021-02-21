namespace Extensions.CSharp
{
    /// <summary>
    /// Provides extension methods for the int data type.
    /// </summary>
    public static class IntExtensions
    {
        #region Clamp In Place
        /// <summary>
        /// Clamps an integer in place.
        /// </summary>
        /// <param name="value">The integer value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public static void Limit(this ref int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
        }
        /// <summary>
        /// Clamps an integer from the left.
        /// </summary>
        /// <param name="value">The integer value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        public static void LowerLimit(this ref int value, int min)
        {
            if (value < min)
                value = min;
        }
        /// <summary>
        /// Clamps an integer from the right.
        /// </summary>
        /// <param name="value">The integer value to clamp.</param>
        /// <param name="max">The maximum value.</param>
        public static void UpperLimit(this ref int value, int max)
        {
            if (value > max)
                value = max;
        }
        #endregion
    }
}
