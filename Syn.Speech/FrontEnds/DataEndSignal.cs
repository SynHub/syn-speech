using Syn.Speech.Helper;
//PATROLLED
//REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// A signal that indicates the end of data.
    /// </summary>
    public class DataEndSignal : Signal
    {
        /// <summary>
        /// Constructs a DataEndSignal.
        /// </summary>
        /// <param name="duration">The duration of the entire data stream in milliseconds.</param>
        public DataEndSignal(long duration)
            : this(duration, Java.CurrentTimeMillis())
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEndSignal"/> class.
        /// </summary>
        /// <param name="duration">The duration of the entire data stream in milliseconds.</param>
        /// <param name="time">The creation time of the DataEndSignal.</param>
        public DataEndSignal(long duration, long time) :base(time)
        {
            
            Duration = duration;
        }

        /// <summary>
        /// Returns the duration of the entire data stream in milliseconds
        /// </summary>
        public long Duration { get; private set; }



        /// <summary>
        ///  Returns the string "DataEndSignal".
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() 
        {
            return ("DataEndSignal: creation time: " + Time + ", duration: " +
                    Duration + "ms");
        }
    }
}
