using System;

namespace Syn.Speech.FrontEnd
{
    /// <summary>
    /// A signal that indicates the start of speech. 
    /// </summary>
    public class SpeechStartSignal: Signal
    {
         /** Constructs a SpeechStartSignal. */
        public SpeechStartSignal() 
            :this(DateTime.Now.Ticks)
        {
            
        }


        /**
        /// Constructs a SpeechStartSignal at the given time.
         *
        /// @param time the time this SpeechStartSignal is created
         */
        public SpeechStartSignal(long time) 
            : base(time)
        {
        }


        /**
        /// Returns the string "SpeechStartSignal".
         *
        /// @return the string "SpeechStartSignal"
         */
        override
        public String ToString() 
        {
            return "SpeechStartSignal";
        }
    }
}
