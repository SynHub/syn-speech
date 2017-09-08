using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder
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
        void addResultListener(IResultListener resultListener);

        /// <summary>
        /// Removes a listener from this ResultProducer -instance.
        /// </summary>
        /// <param name="resultListener"></param>
        void removeResultListener(IResultListener resultListener);
    }
}
