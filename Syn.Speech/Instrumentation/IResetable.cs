//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{
    /// <summary>
    /// Defines the interface for an object that is resetable
    /// </summary>
    public interface IResetable
    {
        /// <summary>
        /// Resets this component. Typically this is for components that keep track of statistics
        /// </summary>
        void Reset();
    }
}
