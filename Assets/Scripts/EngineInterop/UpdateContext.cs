using System;

namespace AI_PROG_SP21.EngineInterop
{
    #region Interface
    /// <summary>
    /// Wraps the standard update functions of game engines.
    /// </summary>
    public interface IUpdateContext
    {
        /// <summary>
        /// Called on each draw cycle, passing the elapsed time in seconds.
        /// </summary>
        event Action<float> Draw;
        /// <summary>
        /// Called on each fixed cycle, passing the elapsed time in seconds.
        /// </summary>
        event Action<float> FixedStep;
    }
    #endregion
    #region Base Class
    public abstract class UpdateContext : IUpdateContext
    {
        public abstract event Action<float> Draw;
        public abstract event Action<float> FixedStep;
    }
    #endregion
}
