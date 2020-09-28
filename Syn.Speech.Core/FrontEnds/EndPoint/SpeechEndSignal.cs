using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    /// A signal that indicates the end of speech.
    /// </summary>
    public class SpeechEndSignal: Signal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechEndSignal"/> class.
        /// </summary>
        public SpeechEndSignal() 
            :this(Java.CurrentTimeMillis())
        {
            
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechEndSignal"/> class with the given creation time.
        /// </summary>
        /// <param name="time">Time the creation time of the SpeechEndSignal.</param>
        public SpeechEndSignal(long time) :base(time)
        {
        }

        /// <summary>
        /// Returns the string "SpeechEndSignal".
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() 
        {
            return "SpeechEndSignal";
        }
    }
}
