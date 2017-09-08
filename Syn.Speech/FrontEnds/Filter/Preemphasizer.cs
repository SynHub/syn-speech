using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Filter
{
    /// <summary>
    /// Implements a high-pass filter that compensates for attenuation in the audio data. Speech signals have an attenuation
    /// (a decrease in intensity of a signal) of 20 dB/dec. It increases the relative magnitude of the higher frequencies
    /// with respect to the lower frequencies.
    ///
    /// The Preemphasizer takes a {@link Data}object that usually represents audio data as input, and outputs the same {@link
    /// Data}object, but with preemphasis applied. For each value X[i] in the input Data object X, the following formula is
    /// applied to obtain the output Data object Y:
    ///     
    /// <code> Y[i] = X[i] - (X[i-1]/// preemphasisFactor) </code>
    /// 
    /// where 'i' denotes time.
    /// 
    /// The preemphasis factor has a value defined by the field {@link #PROP_PREEMPHASIS_FACTOR} of 0.97. A common value for
    /// this factor is something around 0.97.
    /// 
    /// Other {@link Data}objects are passed along unchanged through this Preemphasizer.
    /// <p/>
    /// The Preemphasizer emphasizes the high frequency components, because they usually contain much less energy than lower
    /// frequency components, even though they are still important for speech recognition. It is a high-pass filter because
    /// it allows the high frequency components to "pass through", while weakening or filtering out the low frequency
    /// components.
    /// </summary>
    public class Preemphasizer : BaseDataProcessor
    {
        /** The property for preemphasis factor/alpha. */
        [S4Double(DefaultValue = 0.97)]
        public static string PropPreemphasisFactor = "factor";

        private double _preemphasisFactor;
        private double _prior;

        public Preemphasizer( double preemphasisFactor ) 
        {
            _preemphasisFactor = preemphasisFactor;
        }

        public Preemphasizer( ) 
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _preemphasisFactor = ps.GetDouble(PropPreemphasisFactor);
        }

        /// <summary>
        /// Returns the next Data object being processed by this Preemphasizer, or if it is a Signal, it is returned without
        /// modification.
        /// </summary>
        /// <returns>the next available Data object, returns null if no Data object is available</returns>
        public override IData GetData() 
        {
            IData input = Predecessor.GetData();
            if (input != null) {
                if (input is DoubleData) 
                {
                    ApplyPreemphasis(((DoubleData) input).Values);
                } 
                else if (input is DataEndSignal || input is SpeechEndSignal) 
                {
                    _prior = 0;
                }
            }
            return input;
        }


        /// <summary>
        /// Applies pre-emphasis filter to the given Audio. The preemphasis is applied in place.
        /// </summary>
        /// <param name="_in">audio data</param>
        private void ApplyPreemphasis(double[] _in) 
        {
            // set the prior value for the next Audio
            double nextPrior = _prior;
            if (_in.Length > 0) 
            {
                nextPrior = _in[_in.Length - 1];
            }
            if (_in.Length > 1 && _preemphasisFactor != 0.0) 
            {
                // do preemphasis
                double current;
                double previous = _in[0];
                _in[0] = previous - _preemphasisFactor* _prior;
                for (int i = 1; i < _in.Length; i++) 
                {
                    current = _in[i];
                    _in[i] = current - _preemphasisFactor* previous;
                    previous = current;
                }
            }
            _prior = nextPrior;
        }
    }
}
