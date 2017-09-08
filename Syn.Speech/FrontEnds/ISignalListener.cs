//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// The listener interface for being informed when a {@link Signal Signal} is generated.
    /// </summary>
    public interface ISignalListener
    {

        /// <summary>
        /// Method called when a signal is detected.
        /// </summary>
        /// <param name="signal">The signal.</param>
        void SignalOccurred(Signal signal);
    }
}
