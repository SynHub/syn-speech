using System;

namespace Syn.Speech.FrontEnd
{
    /// <summary>
    /// A signal that indicates the end of speech.
    /// </summary>
    public class SpeechEndSignal: Signal
    {
        /** Constructs a SpeechEndSignal. */
        public SpeechEndSignal() 
            :this(DateTime.Now.Ticks)
        {
            
        }


        /**
        /// Constructs a SpeechEndSignal with the given creation time.
         *
        /// @param time the creation time of the SpeechEndSignal
         */
        public SpeechEndSignal(long time) 
            :base(time)
        {
            ;
        }


        /**
        /// Returns the string "SpeechEndSignal".
         *
        /// @return the string "SpeechEndSignal"
         */
        override
        public String ToString() 
        {
            return "SpeechEndSignal";
        }
    }
}
