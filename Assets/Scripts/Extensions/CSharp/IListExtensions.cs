using System;
using System.Collections.Generic;

namespace Extensions.CSharp
{
    /// <summary>
    /// Provides extension methods for class that implement IList.
    /// </summary>
    public static class IListExtensions
    {
        #region Argument Checking
        /// <summary>
        /// Checks if the given arguments are valid indices in this array.
        /// </summary>
        /// <param name="collection">The collection to check in.</param>
        /// <param name="indices">The indices to check in the collection.</param>
        /// <returns>True if all indices are in range.</returns>
        public static bool IndexArgsAreInRange<T>(this IList<T> collection, params int[] indices)
        {
            foreach (int index in indices)
                if (index < 0 || index > collection.Count - 1)
                    return false;
            return true;
        }
        #endregion
        #region Conversion Utilities
        /// <summary>
        /// Assembles a new array by calling an accessor or method on the original array type.
        /// </summary>
        /// <param name="collection">The collection to assemble from.</param>
        /// <param name="accessor">The function to access the new array value.</param>
        /// <returns>A new collection assembled by calling the accessor function at each index.</returns>
        public static TTo[] ArrayInto<TFrom, TTo>(this IList<TFrom> collection, Func<TFrom, TTo> accessor)
        {
            TTo[] converted = new TTo[collection.Count];
            for (int i = 0; i < collection.Count; i++)
                converted[i] = accessor(collection[i]);
            return converted;
        }
        #endregion
    }
}
