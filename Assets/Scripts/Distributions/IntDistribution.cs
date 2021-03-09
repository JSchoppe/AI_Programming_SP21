using System;
using UnityEngine;

namespace AI_PROG_SP21.Distributions
{
    /// <summary>
    /// A distribution for ints that can be used in the inspector.
    /// </summary>
    [Serializable]
    public sealed class IntDistribution : Distribution<int>
    {
        #region Fields
        [Tooltip("The left end of the distribution.")]
        public int min = 0;
        [Tooltip("The right end of the distribution.")]
        public int max = 1;
        [Tooltip("The type of distribution.")]
        public ContinuousDistributionType distributionType =
            ContinuousDistributionType.Flat;
        #endregion
        #region Next Method
        /// <summary>
        /// Pulls the next value from the distribution.
        /// </summary>
        /// <returns>An integer value under the distribution curve.</returns>
        public override int Next()
        {
            switch (distributionType)
            {
                case ContinuousDistributionType.Flat:
                    return NextFlat();
                default: throw new NotImplementedException();
            }
        }
        private int NextFlat()
        {
            return UnityEngine.Random.Range(min, max + 1);
        }
        #endregion
    }
}
