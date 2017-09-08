//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.NGram.Large
{
    /// <summary>
    /// Represents a probability, a backoff probability, and the location of the first bigram entry. 
    /// </summary>
    public class UnigramProbability
    {
        /**
        /// Constructs a UnigramProbability
         *
        /// @param wordID           the id of the word
        /// @param logProbability   the probability
        /// @param logBackoff       the backoff probability
        /// @param firstBigramEntry the first bigram entry
         */
        public UnigramProbability(int wordID, float logProbability,
                                  float logBackoff, int firstBigramEntry) 
        {
            WordID = wordID;
            LogProbability = logProbability;
            LogBackoff = logBackoff;
            FirstBigramEntry = firstBigramEntry;
        }


        /**
        /// Returns a string representation of this object
         *
        /// @return the string form of this object
         */

        public override string ToString() 
        {
            return "Prob: " + LogProbability + ' ' + LogBackoff;
        }


        /**
        /// Returns the word ID of this unigram
         *
        /// @return the word ID of this unigram
         */

        public int WordID { get; private set; }

        public float LogProbability { get; private set; }

        /**
        /// Returns the log backoff weight of this unigram
         *
        /// @return the log backoff weight of this unigram
         */

        public float LogBackoff { get; private set; }


        /**
        /// Returns the index of the first bigram entry of this unigram.
         *
        /// @return the index of the first bigram entry of this unigram
         */

        public int FirstBigramEntry { get; private set; }

        public void SetLogProbability(float logProbabilityValue)
        {
            LogProbability = logProbabilityValue;
        }

        /**
        /// Sets the log backoff weight.
         *
        /// @param logBackoff the new log backoff weight
         */
        public void SetLogBackoff(float logBackoff) 
        {
            LogBackoff = logBackoff;
        }

       

    }
}
