using System;
using Syn.Speech.Util.machlearn;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// A Data object that holds data of primitive type double.
    /// </summary>
    public class DoubleData: OVector, IData
    {
        public DoubleData(double[] values) :base(values)
        {
           
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleData"/> class.
        /// </summary>
        /// <param name="values">The data values.</param>
        /// <param name="sampleRate">The sample rate of the data.</param>
        /// <param name="firstSampleNumber">The position of the first sample in the original data.</param>
        public DoubleData(double[] values, int sampleRate, long firstSampleNumber)  :base(values)
        {
            SampleRate = sampleRate;
            CollectTime = firstSampleNumber* 1000 / sampleRate;
            FirstSampleNumber = firstSampleNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleData"/> class.
        /// </summary>
        /// <param name="values">The data values.</param>
        /// <param name="sampleRate">The sample rate of the data.</param>
        /// <param name="collectTime">The time at which this data is collected.</param>
        /// <param name="firstSampleNumber">The position of the first sample in the original data.</param>
        public DoubleData(double[] values, int sampleRate, long collectTime, long firstSampleNumber)  :base(values)
        {
            SampleRate = sampleRate;
            CollectTime = collectTime;
            FirstSampleNumber = firstSampleNumber;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents that describes the data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() 
        {
            return ("DoubleData: " + SampleRate + "Hz, first sample #: " +
                    FirstSampleNumber + ", collect time: " + CollectTime);
        }


        /// <summary>
        /// Gets the sample rate of the data.
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Gets the position of the first sample in the original data. The very first sample number is zero.
        /// </summary>
        public long FirstSampleNumber { get; private set; }

        /// <summary>
        /// Gets the time in milliseconds at which the audio data is collected.
        /// </summary>
        /// <value>
        /// The difference, in milliseconds, between the time the audio data is collected and midnight, January 1, 1970
        /// </value>
        public long CollectTime { get; private set; }

        public DoubleData Clone()
        {
            try 
            {
                var data = (DoubleData)base.MemberwiseClone();
                data.SampleRate = SampleRate;
                data.CollectTime = CollectTime;
                data.FirstSampleNumber = FirstSampleNumber;
                return data;
            } 
            catch (Exception) 
            {
                throw new Exception("CloneNotSupportedException");
            }
        }

    }
}
