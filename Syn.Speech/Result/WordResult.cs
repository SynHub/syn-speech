using System;
using Syn.Speech.Common;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;

namespace Syn.Speech.Result
{
    /// <summary>
    /// Represents a word in a recognition result.
    /// This is designed specifically for obtaining confidence scores.
    /// All scores are maintained in LogMath log base.
    /// </summary>
    public class WordResult
    {
        
        private IWord word;
        private TimeFrame timeFrame;

        private double score;
        private double confidence;

        /// </summary>
        /// Construct a word result from a string and a confidence score.
        ///
        /// @param w the word
        /// @param confidence the confidence for this word
        /// </summary>
        public WordResult(String w, double confidence) 
        {
            Pronunciation[] pros = {Pronunciation.UNKNOWN};
            word = new Word(w, pros, false);
            timeFrame = TimeFrame.NULL;
            this.confidence = confidence;
            this.score = LogMath.LOG_ZERO;
        }

        /// </summary>
        /// Construct a word result with full information.
        ///
        /// @param w the word object to store
        /// @param timeFrame time frame
        /// @param ef word end time
        /// @param score score of the word
        /// @param confidence confidence (posterior) of the word
        /// </summary>
        public WordResult(IWord w, TimeFrame timeFrame,
                double score, double confidence)
        {
            this.word = w;
            this.timeFrame = timeFrame;
            this.score = score;
            this.confidence = confidence;
        }

        /// </summary>
        /// Construct a WordResult using a Node object and a confidence (posterior).
        ///
        /// This does not use the posterior stored in the Node object, just its
        /// word, start and end.
        ///
        /// TODO: score is currently set to zero
        ///
        /// @param node the node to extract information from
        /// @param confidence the confidence (posterior) to assign
        /// </summary>
        public WordResult(Node node, double confidence):
            this(node.getWord(),
                 new TimeFrame(node.getBeginTime(), node.getEndTime()),
                 LogMath.LOG_ZERO, confidence)
        {
            
        }

        /// </summary>
        /// Gets the total score for this word.
        ///
        /// @return the score for the word (in LogMath log base)
        /// </summary>
        public double getScore() 
        {
            return score;
        }

        /// </summary>
        /// Returns a log confidence score for this WordResult.
        ///
        /// Use the getLogMath().logToLinear() method to convert the log confidence
        /// score to linear. The linear value should be between 0.0 and 1.0
        /// (inclusive) for this word.
        ///
        /// @return a log confidence score which linear value is in [0, 1]
        /// </summary>
        public double getConfidence() 
        {
            // TODO: can confidence really be greater than 1?
            return Math.Min(confidence, LogMath.LOG_ONE);
        }

        /// </summary>
        /// Gets the pronunciation for this word.
        ///
        /// @return the pronunciation for the word
        /// </summary>
        public IPronunciation getPronunciation() 
        {
            return word.getMostLikelyPronunciation();
        }

        /// </summary>
        /// Gets the word object associated with the given result.
        ///
        /// @return the word object
        /// </summary>
        public IWord getWord() 
        {
            return word;
        }

        /// </summary>
        /// Gets time frame for the word
        /// </summary>
        public TimeFrame getTimeFrame() 
        {
            return timeFrame;
        }

        /// </summary>
        /// Does this word result represent a filler token?
        ///
        /// @return true if this is a filler
        /// </summary>
        public Boolean isFiller() 
        {
            return word.isFiller() || word.ToString().Equals("<skip>");
        }

        override
        public String ToString() {
            return String.Format("{{0}, {1}, [{2}]}", word, confidence, timeFrame);
        }
    }
}
