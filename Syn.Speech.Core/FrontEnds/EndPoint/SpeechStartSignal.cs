using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    /// A signal that indicates the start of speech. 
    /// </summary>
    public class SpeechStartSignal: Signal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechStartSignal"/> class.
        /// Constructs a SpeechStartSignal.
        /// </summary>
        public SpeechStartSignal() 
            :this(DateTime.Now.Ticks)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechStartSignal"/> class.
        /// Constructs a SpeechStartSignal at the given time.
        /// </summary>
        /// <param name="time">The time this SpeechStartSignal is created.</param>
        public SpeechStartSignal(long time) 
            : base(time)
        {
        }

        /// <summary>
        /// Returns the string "SpeechStartSignal".
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() 
        {
            return "SpeechStartSignal";
        }
    }
}
