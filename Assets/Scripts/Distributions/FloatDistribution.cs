using System;
using UnityEngine;

namespace Distributions
{
    /// <summary>
    /// A distribution for floats that can be used in the inspector.
    /// </summary>
    [Serializable]
    public sealed class FloatDistribution : Distribution<float>
    {
        #region Fields
        [Tooltip("The left end of the distribution.")]
        public float min = 0f;
        [Tooltip("The right end of the distribution.")]
        public float max = 1f;
        [Tooltip("The type of distribution.")]
        public ContinuousDistributionType distributionType =
            ContinuousDistributionType.Flat;
        #endregion
        #region Next Method
        /// <summary>
        /// Pulls the next value from the distribution.
        /// </summary>
        /// <returns>A floating point value under the distribution curve.</returns>
        public override float Next()
        {
            switch (distributionType)
            {
                case ContinuousDistributionType.Flat:
                    return NextFlat();
                default: throw new NotImplementedException();
            }
        }
        private float NextFlat()
        {
            return min + UnityEngine.Random.value * (max - min);
        }
        #endregion
    }
}
