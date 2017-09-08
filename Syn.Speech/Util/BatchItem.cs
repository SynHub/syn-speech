using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    ///  Provides a standard interface to for a single decode in a batch of decodes
    /// </summary>
    public class BatchItem
    {
        /**
        /// Creates a batch item
         *
        /// @param filename   the filename
        /// @param transcript the transcript
         */
        public BatchItem(String filename, string transcript) 
        {
            Filename = filename;
            Transcript = transcript;
        }


        /**
        /// Gets the filename for this batch
         *
        /// @return the file name
         */

        public string Filename { get; private set; }


        /**
        /// Gets the transcript for the batch
         *
        /// @return the transcript (or null if there is no transcript)
         */

        public string Transcript { get; private set; }
    }
}
