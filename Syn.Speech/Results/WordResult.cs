using System;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// Represents a word in a recognition result.
    /// This is designed specifically for obtaining confidence scores.
    /// All scores are maintained in LogMath log base.
    /// </summary>
    public class WordResult
    {
        private readonly double _confidence;

        /// <summary>
        /// Construct a word result with full information.
        /// @param w the word object to store
        /// @param timeFrame time frame
        /// @param ef word end time
        /// @param score score of the word
        /// @param confidence confidence (posterior) of the word
        /// </summary>
        public WordResult(Word w, TimeFrame timeFrame, double score, double posterior)
        {
            Word = w;
            TimeFrame = timeFrame;
            Score = score;
            _confidence = posterior;
        }

        /// <summary>
        /// Construct a WordResult using a Node object and a confidence (posterior).
        /// This does not use the posterior stored in the Node object, just its
        /// word, start and end.
        /// TODO: score is currently set to zero
        /// @param node the node to extract information from
        /// @param confidence the confidence (posterior) to assign
        /// </summary>
        public WordResult(Node node):
            this(node.Word, new TimeFrame(node.BeginTime, node.EndTime), node.ViterbiScore, node.Posterior)
        {
            
        }

        /// <summary>
        /// Gets the total score for this word.
        /// @return the score for the word (in LogMath log base)
        /// </summary>
        public double Score { get; private set; }

        /// <summary>
        /// Returns a log confidence score for this WordResult.
        /// Use the getLogMath().logToLinear() method to convert the log confidence
        /// score to linear. The linear value should be between 0.0 and 1.0
        /// (inclusive) for this word.
        /// @return a log confidence score which linear value is in [0, 1]
        /// </summary>
        public double GetConfidence() 
        {
            // TODO: can confidence really be greater than 1?
            return Math.Min(_confidence, LogMath.LogOne);
        }

        /// <summary>
        /// Gets the pronunciation for this word.
        /// @return the pronunciation for the word
        /// </summary>
        public Pronunciation GetPronunciation() 
        {
            return Word.GetMostLikelyPronunciation();
        }

        /// <summary>
        /// Gets the word object associated with the given result.
        /// @return the word object
        /// </summary>
        public Word Word { get; private set; }

        /// <summary>
        /// Gets time frame for the word
        /// </summary>
        public TimeFrame TimeFrame { get; private set; }

        /// <summary>
        /// Does this word result represent a filler token?
        /// @return true if this is a filler
        /// </summary>
        public Boolean IsFiller() 
        {
            return Word.IsFiller || Word.ToString().Equals("<skip>");
        }


        public override string ToString()
        {
            return String.Format("{0}, {1}, [{2}]", Word, _confidence, TimeFrame);
        }
    }
}
