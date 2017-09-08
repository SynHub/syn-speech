using System;
using System.IO;
using Syn.Logging;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Api
{
    /// <summary>
    /// Speech recognizer that works with audio resources. 
    /// </summary>
    public class StreamSpeechRecognizer : AbstractSpeechRecognizer
    {
        /// <summary>
        /// Constructs new stream recognizer.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public StreamSpeechRecognizer(Configuration configuration) : base(configuration) { }

        /// <summary>
        /// Allocates the speech recognizer and starts the speech recognition process.
        /// </summary>
        /// <param name="stream"></param>
        public void StartRecognition(Stream stream)
        {
            StartRecognition(stream, TimeFrame.Infinite);
        }

        /// <summary>
        /// Starts recognition process.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="timeFrame"></param>
        public void StartRecognition(Stream stream, TimeFrame timeFrame)
        {
            try
            {
                Recognizer.Allocate();
                Context.SetSpeechSource(stream, timeFrame);
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }

        /// <summary>
        /// Stops recognition process.
        /// </summary>
        public void StopRecognition()
        {
            try
            {
                Recognizer.Deallocate();
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }
    }
}
