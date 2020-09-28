//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    /// An abstract analyzer that signals about presense of speech in last processing frame.
    /// This information is used in noise filtering components to estimate noise spectrum
    /// for example.
    /// </summary>
    public abstract class AbstractVoiceActivityDetector: BaseDataProcessor
    {
        /// <summary>
        /// Returns the state of speech detected.
        /// </summary>
        /// <value>If last processed data object was classified as speech.</value>
        public abstract bool IsSpeech { get; }
    }
}
