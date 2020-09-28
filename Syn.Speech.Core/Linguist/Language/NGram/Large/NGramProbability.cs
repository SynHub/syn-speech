//REFACTORED
namespace Syn.Speech.Linguist.Language.NGram.Large
{
    /// <summary>
    ///  Represents a word ID (Nth word of a N-gram), and a N-gram probability ID.
    /// </summary>
    public class NGramProbability
    {
        /**
        /// Constructs a NGramProbability
        /// 
        /// @param which
        ///            which follower of the first word is this NGram
        /// @param wordID
        ///            the ID of the Nth word in a NGram
        /// @param probabilityID
        ///            the index into the probability array
        /// @param backoffID
        ///            the index into the backoff probability array
        /// @param firstNPlus1GramEntry
        ///            the first N+1Gram entry
         */
        public NGramProbability(int which, int wordID, int probabilityID,
                int backoffID, int firstNPlus1GramEntry) 
        {
            WhichFollower = which;
            WordID = wordID;
            ProbabilityID = probabilityID;
            BackoffID = backoffID;
            FirstNPlus1GramEntry = firstNPlus1GramEntry;
        }

        /**
        /// Returns which follower of the first word is this NGram
        /// 
        /// @return which follower of the first word is this NGram
         */

        public int WhichFollower { get; private set; }

        /**
        /// Returns the Nth word ID of this NGram
        /// 
        /// @return the Nth word ID
         */

        public int WordID { get; private set; }

        /**
        /// Returns the NGram probability ID.
        /// 
        /// @return the NGram probability ID
         */

        public int ProbabilityID { get; private set; }

        /**
        /// Returns the backoff weight ID.
        /// 
        /// @return the backoff weight ID
         */

        public int BackoffID { get; private set; }

        /**
        /// Returns the index of the first N+1Gram entry.
        /// 
        /// @return the index of the first N+1Gram entry
         */

        public int FirstNPlus1GramEntry { get; private set; }
    }
}
