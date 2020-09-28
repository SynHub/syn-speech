using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// A signal that indicates the start of data.
    /// </summary>
    public class DataStartSignal: Signal
    {

        /// <summary>
        /// A constant that is attached to all DataStartSignal passing this component. This allows subsequent
        /// <code>DataProcessor</code>s (like the <code>Scorer</code>) to adapt their processing behavior.
        /// </summary>
        public static string SpeechTaggedFeatureStream = "vadTaggedFeatureStream";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStartSignal"/> class.
        /// </summary>
        /// <param name="sampleRate">The sampling rate of the started data stream.</param>
        public DataStartSignal(int sampleRate): this(sampleRate, Java.CurrentTimeMillis())
        {
            
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataStartSignal"/> class.
        /// </summary>
        /// <param name="sampleRate">The sampling rate of the started data stream..</param>
        /// <param name="time">The time this DataStartSignal is created.</param>
        public DataStartSignal(int sampleRate, long time): this(sampleRate, time, false)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStartSignal"/> class.
        /// </summary>
        /// <param name="sampleRate">The sampling rate of the started data stream.</param>
        /// <param name="tagAsVadStream"><code>true</code> if this feature stream will contain vad-signals.</param>
        public DataStartSignal(int sampleRate, Boolean tagAsVadStream) :this(sampleRate, Java.CurrentTimeMillis(), tagAsVadStream)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStartSignal"/> class.
        /// </summary>
        /// <param name="sampleRate">The sampling rate of the started data stream..</param>
        /// <param name="time">The time this DataStartSignal is created.</param>
        /// <param name="tagAsVadStream"><code>true</code> if this feature stream will contain vad-signals.</param>
        public DataStartSignal(int sampleRate, long time, Boolean tagAsVadStream) :base(time)
        {
            
            this.SampleRate = sampleRate;

            if (tagAsVadStream) {
                GetProps().Add(SpeechTaggedFeatureStream, tagAsVadStream);
            }
        }

        /// <summary>
        /// Returns the string "DataStartSignal".
        /// </summary>
        public override string ToString() 
        {
            return "DataStartSignal: creation time: " + Time;
        }

        /// <summary>
        /// Gets the sampling rate of the started data stream.
        /// </summary>
        public int SampleRate { get; private set; }

        public static void TagAsVadStream(DataStartSignal dsSignal) 
        {
            Java.Put(dsSignal.GetProps(),SpeechTaggedFeatureStream, true);
            //dsSignal.getProps().Add(, true);
        }

        public static void UntagAsVadStream(DataStartSignal dsSignal) 
        {
            dsSignal.GetProps().Remove(SpeechTaggedFeatureStream);
        }
    }
}
