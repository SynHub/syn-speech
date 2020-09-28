//PATROLLED + REFACTORED
namespace Syn.Speech.Results
{

    /**
     * Represents a path of words through the recognition result lattice.
     * <p/>
     * All scores are maintained in the logMath log domain
     */
    public interface IPath
    {

        /**
         * Gets the total score for this path. Scores are in the LogMath log domain
         *
         * @return the score for the path in the LogMath log domaain.
         */
        double GetScore();


        /**
         * Returns a log confidence score for this path. Use the getLogMath().logToLinear() method to convert the log
         * confidence score to linear. The linear value should be between 0.0 and 1.0 (inclusive).
         *
         * @return a log confidence score which linear value is between 0.0 and 1.0 (inclusive)
         */
        double GetConfidence();

        /**
         * Gets the ordered set of words for this path
         *
         * @return an array containing zero or more words
         */
        WordResult[] GetWords();


        /**
         * Gets the transcription of the path.
         *
         * @return the transcription of the path.
         */
        string GetTranscription();

        /**
         * Gets the transcription of the path skipping the filler words
         *
         * @return the transcription of the path without fillers.
         */
        string GetTranscriptionNoFiller();

        /// <summary>
        /// Returns a string representation of this object
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        string ToString();
    }
}
