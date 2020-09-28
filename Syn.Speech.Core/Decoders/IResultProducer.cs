using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders
{
    /// <summary>
    /// Some API-elements shared by components which are able to produce Results.
    /// </summary>
    public interface IResultProducer : IConfigurable
    {
        /// <summary>
        /// Registers a new listener for Result.
        /// </summary>
        /// <param name="resultListener"></param>
        void AddResultListener(IResultListener resultListener);

        /// <summary>
        /// Removes a listener from this ResultProducer -instance.
        /// </summary>
        /// <param name="resultListener"></param>
        void RemoveResultListener(IResultListener resultListener);
    }
}
