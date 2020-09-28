using System;
using System.Runtime.Serialization;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    /// A container for DoubleData class that indicates whether the contained DoubleData is speech or not. 
    /// </summary>
    public class SpeechClassifiedData:IData
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechClassifiedData"/> class.
        /// </summary>
        /// <param name="doubleData">The DoubleData.</param>
        /// <param name="isSpeech">Indicates whether the DoubleData is speech.</param>
        public SpeechClassifiedData(DoubleData doubleData, Boolean isSpeech) 
        {
            DoubleData = doubleData;
            IsSpeech = isSpeech;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this SpeechClassifiedData is speech or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is speech; otherwise, <c>false</c>.
        /// </value>
        public bool IsSpeech { get;  set; }


        /// <summary>
        /// Gets the data values.
        /// </summary>

        public double[] Values
        {
            get { return DoubleData.Values; }
        }

        /// <summary>
        /// Gets the sample rate of the data.
        /// </summary>
        /// <value>
        /// Gets the sample rate of the data.
        /// </value>
        public int SampleRate
        {
            get { return DoubleData.SampleRate; }
        }

        /// <summary>
        /// Gets the time in milliseconds at which the audio data is collected.
        /// </summary>
        /// <value>
        /// The difference, in milliseconds, between the time the audio data is collected and midnight, January 1, 1970.
        /// </value>
        public long CollectTime
        {
            get { return DoubleData.CollectTime; }
        }

        /// <summary>
        /// Gets the position of the first sample in the original data. The very first sample number is zero.
        /// </summary>
        /// <value>
        /// The position of the first sample in the original data
        /// </value>
        public long FirstSampleNumber
        {
            get { return DoubleData.FirstSampleNumber; }
        }

        /// <summary>
        /// Gets the DoubleData contained by this SpeechClassifiedData.
        /// </summary>
        /// <value>
        /// The DoubleData contained by this SpeechClassifiedData.
        /// </value>
        public DoubleData DoubleData { get; private set; }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "SpeechClassifiedData containing " + DoubleData + " classified as " + (IsSpeech ? "speech" : "non-speech");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
