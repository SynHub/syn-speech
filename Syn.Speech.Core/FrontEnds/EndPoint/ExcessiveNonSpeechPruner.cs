using System.Diagnostics;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    /// Removes excessive non-speech-segments from a speech stream. 
    /// Compared with <code>NonSpeechDatatFilter</code> this component does not remove all non-speech frames. 
    /// It just reduces the non-speech parts to a user defined length.
    /// <see cref="SpeechMarker"/>
    /// <see cref="NonSpeechDataFilter"/>
    /// </summary>
    public class ExcessiveNonSpeechPruner : BaseDataProcessor
    {
        /// <summary>
        /// The property for the maximum amount of (subsequent) none-speech time (in ms) to be preserved in the speech stream.
        /// </summary>
        [S4Integer(DefaultValue = Integer.MAX_VALUE)]
        public static string PropMaxNonSpeechTimeMs = "maxNonSpeechTimeMs";

        private int _maxNonSpeechTime;
        private bool _inSpeech;
        private int _nonSpeechCounter;

        public ExcessiveNonSpeechPruner(int maxNonSpeechTime)
        {
            _maxNonSpeechTime = maxNonSpeechTime;
        }

        public ExcessiveNonSpeechPruner()
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            _maxNonSpeechTime = ps.GetInt(PropMaxNonSpeechTimeMs);
        }

        /// <summary>
        /// Returns the processed Data output.
        /// </summary>
        /// <returns>
        /// an Data object that has been processed by this DataProcessor
        /// </returns>
        public override IData GetData()
        {
            IData data = Predecessor.GetData();

            if (data is SpeechEndSignal || data is DataStartSignal)
            {
                _inSpeech = false;
                _nonSpeechCounter = 0;
            }
            else if (data is SpeechStartSignal)
            {
                _inSpeech = true;
            }
            else if (data is DoubleData || data is FloatData)
            {
                if (!_inSpeech)
                {
                    _nonSpeechCounter += GetAudioTime(data);
                    if (_nonSpeechCounter >= _maxNonSpeechTime)
                        data = GetData();
                }
            }

            return data;
        }

        /// <summary>
        /// Returns the amount of audio data in milliseconds in the given SpeechClassifiedData object.
        /// </summary>
        /// <param name="data">The SpeechClassifiedData object.</param>
        /// <returns>The amount of audio data in milliseconds.</returns>
        public int GetAudioTime(IData data)
        {
            if (data is DoubleData)
            {
                DoubleData audio = (DoubleData)data;
                return (int)((audio.Values.Length * 1000.0f / audio.SampleRate));
            }
            else if (data is FloatData)
            {
                FloatData audio = (FloatData)data;
                return (int)((audio.Values.Length * 1000.0f / audio.SampleRate));
            }

            Debug.Assert(false);
            return -1;
        }
    }
}
