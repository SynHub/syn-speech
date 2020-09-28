using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// Contains statistics about a frame.
    /// <p/>
    /// Note that all scores are maintained in LogMath log base
    /// </summary>
    public abstract class FrameStatistics
    {
        /// <summary>
        /// Gets the frame number
        /// </summary>
        /// <returns></returns>
        public abstract int GetFrameNumber();

        /// <summary>
        /// Gets the feature associated with this frame
        /// </summary>
        /// <returns></returns>
        public abstract IData GetData();

        /// <summary>
        /// Gets the best score for this frame
        /// </summary>
        /// <returns></returns>
        public abstract float GetBestScore();

        /// <summary>
        /// Gets the unit that had the best score for this frame
        /// </summary>
        /// <returns></returns>
        public abstract Unit GetBestUnit();

        /// <summary>
        /// Gets the best scoring hmm state for this frame
        /// </summary>
        /// <returns></returns>
        public abstract int GetBestState();
    }

}
