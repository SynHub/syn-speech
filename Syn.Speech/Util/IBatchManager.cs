//REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Provides a standard interface to a batching mechanism
    /// </summary>
    public interface IBatchManager
    {
        /**
        /// Starts processing the batch
         *
        /// @throws IOException if an error occurs while processing the batch file
         */
        void Start();


        /**
        /// Gets the next available batch item or null if no more are available
         *
        /// @return the next available batch item
        /// @throws IOException if an error occurs while processing the batch file
         */
        BatchItem GetNextItem();


        /**
        /// Stops processing the batch
         *
        /// @throws IOException if an error occurs while processing the batch file
         */
        void Stop();


        /**
        /// Returns the name of the file
         *
        /// @return the filename
         */
        string Filename { get; }
    }
}
