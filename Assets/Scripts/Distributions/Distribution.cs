namespace Distributions
{
    /// <summary>
    /// Base class for all distribution collections.
    /// </summary>
    /// <typeparam name="T">The object held in the distribution.</typeparam>
    public abstract class Distribution<T>
    {
        /// <summary>
        /// Prompts the next random element to be chosen.
        /// </summary>
        /// <returns>A random item based on the distribution.</returns>
        public abstract T Next();
    }
}
