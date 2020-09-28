using System;
using System.Runtime.Serialization;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// A Data object that holds data of primitive type float.
    /// </summary>
    public class FloatData : Object, IData, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatData"/> class.
        /// </summary>
        /// <param name="values">The data values.</param>
        /// <param name="sampleRate">The sample rate of the data.</param>
        /// <param name="firstSampleNumber">The position of the first sample in the original data.</param>
        public FloatData(float[] values, int sampleRate, long firstSampleNumber) 
            :this(values, sampleRate, firstSampleNumber* 1000 / sampleRate, firstSampleNumber)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatData"/> class.
        /// </summary>
        /// <param name="values">The data values.</param>
        /// <param name="sampleRate">The sample rate of the data.</param>
        /// <param name="collectTime">The time at which this data is collected.</param>
        /// <param name="firstSampleNumber">The position of the first sample in the original data.</param>
        public FloatData(float[] values, int sampleRate, long collectTime, long firstSampleNumber) 
        {
            this.Values = values;
            this.SampleRate = sampleRate;
            this.CollectTime = collectTime;
            this.FirstSampleNumber = firstSampleNumber;
        }


        /// <summary>
        /// Gets the values of this data.
        /// </summary>
        public float[] Values { get; private set; }

        /// <summary>
        /// Gets the sample rate of this data.
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
        /// the difference, in milliseconds, between the time the audio data is collected and midnight, January 1, 1970
        /// </value>
        public long CollectTime { get; private set; }

        object ICloneable.Clone()
        {
            try
            {
                //
                //The MemberwiseClone method creates a shallow copy by creating a new object, 
                //and then copying the nonstatic fields of the current object to the new object
                var data = (FloatData)MemberwiseClone();
                return data;
            }
            catch (Exception e)
            {
                throw new SystemException(e.ToString());
            }
        }

        /** Converts a given Data-object into a <code>FloatData</code> if possible.
        ///  @param data
         */
        public static FloatData ToFloatData(IData data) 
        {
            FloatData convertData;
            if (data is FloatData)
                convertData = (FloatData) data;
            else if (data is DoubleData) 
            {
                var dd = (DoubleData) data;
                convertData = new FloatData(MatrixUtils.Double2Float(dd.Values), dd.SampleRate,
                        dd.FirstSampleNumber);
            } else
                throw new ArgumentException("data type '" + data.GetType().Name + "' is not supported");

            return convertData;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var toReturn = string.Empty;
            var flag = 10;
            for (var i = 0; i < Values.Length && i < flag; i++)
            {
                toReturn = toReturn + " " + "[" + Values[i] + "]";
            }

            return toReturn;

        }
    }
}
