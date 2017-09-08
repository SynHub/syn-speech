//INCOMPLETE + REFACTORED
namespace Syn.Speech.Api
{
    public class LiveSpeechRecognizer : AbstractSpeechRecognizer
    {
        private readonly Microphone _microphone;

        public LiveSpeechRecognizer(Configuration configuration)
            : base(configuration)
        {
            _microphone = SpeechSourceProvider.GetMicrophone();
            //((StreamDataSource) context.getInstance(typeof (StreamDataSource))).setInputStream(microphone.getStream());

        }

        protected LiveSpeechRecognizer(Context context)
            : base(context)
        {
        }

        /// <summary>
        /// Starts recognition process.
        /// Recognition process is paused until the next call to startRecognition.
        /// </summary>
        /// <param name="clear">clear cached microphone data.</param>
        public void StartRecognition(bool clear)
        {
            Recognizer.Allocate();
            _microphone.StartRecording();
        }

        /// <summary>
        /// Stops recognition process.
        /// </summary>
        public void StopRecognition()
        {
            _microphone.StartRecording();
            Recognizer.Deallocate();
        }
    }
}
