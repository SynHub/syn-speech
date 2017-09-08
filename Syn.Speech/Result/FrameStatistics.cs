using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Linguist.Acoustic;

namespace Syn.Speech.Result
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
        public abstract int getFrameNumber();

        /// <summary>
        /// Gets the feature associated with this frame
        /// </summary>
        /// <returns></returns>
        public abstract IData getData();

        /// <summary>
        /// Gets the best score for this frame
        /// </summary>
        /// <returns></returns>
        public abstract float getBestScore();

        /// <summary>
        /// Gets the unit that had the best score for this frame
        /// </summary>
        /// <returns></returns>
        public abstract Unit getBestUnit();

        /// <summary>
        /// Gets the best scoring hmm state for this frame
        /// </summary>
        /// <returns></returns>
        public abstract int getBestState();
    }

}
