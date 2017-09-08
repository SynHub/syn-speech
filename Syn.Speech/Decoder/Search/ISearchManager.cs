using Syn.Speech.Common;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Decoder.Search
{
    public interface ISearchManager: IConfigurable
    {
        /// <summary>
        /// Allocates the resources necessary for this search. This should be called once before an recognitions are
        /// performed
        /// </summary>
        void allocate();
        /// <summary>
        /// Deallocates resources necessary for this search. This should be called once after all recognitions are completed
        /// at the search manager is no longer needed.
        /// </summary>
        void deallocate();

        /// <summary>
        /// Prepares the SearchManager for recognition.  This method must be called before recognize 
        /// is called. Typically, start and stop  are called bracketing an utterance.
        /// </summary>
        void startRecognition();

        /// <summary>
        /// Performs post-recognition cleanup. This method should be called after recognize returns a final result.
        /// </summary>
        void stopRecognition();

        /// <summary>
        /// Performs recognition. Processes no more than the given number of frames before returning. This method returns a
        /// partial result after nFrames have been processed, or a final result if recognition completes while processing
        /// frames.  If a final result is returned, the actual number of frames processed can be retrieved from the result.
        /// This method may block while waiting for frames to arrive.
        /// </summary>
        /// <param name="nFrames">the maximum number of frames to process. A final result may be returned before all nFrames are processed.</param>
        /// <returns>the recognition result, the result may be a partial or a final result; or return null if no frames are
        /// arrived</returns>
        Results.Result recognize(int nFrames);

    }
}
