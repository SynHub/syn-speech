using System;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    /// The noise Wiener filter. Parameters are taken from the article
    /// "An Effective Subband OSF-Based VAD With Noise Reduction
    /// for Robust Speech Recognition" by Ramirez et all. IEEE
    /// Transactions on Speech And Audio Processing, Vol 13, No 6, 2005
    /// <br />
    /// Subband VAD is not implemented yet, default endpointer is used.
    /// The frontend configuration with filtering should look like:
    /// <br /><br />
    /// &lt;item&gt;audioFileDataSource &lt;/item&gt;<br />
    /// &lt;item&gt;dataBlocker &lt;/item&gt;<br />
    /// &lt;item&gt;preemphasizer &lt;/item&gt;<br />
    /// &lt;item&gt;windower &lt;/item&gt;<br />
    /// &lt;item&gt;fft &lt;/item&gt;<br />
    /// &lt;item&gt;wiener &lt;/item&gt;<br />
    /// &lt;item&gt;speechClassifier &lt;/item&gt;<br />
    /// &lt;item&gt;speechMarker &lt;/item&gt;<br />
    /// &lt;item&gt;nonSpeechDataFilter &lt;/item&gt;<br />
    /// &lt;item&gt;melFilterBank &lt;/item&gt;<br />
    /// &lt;item&gt;dct &lt;/item&gt;<br />
    /// &lt;item&gt;liveCMN &lt;/item&gt;<br />
    /// &lt;item&gt;featureExtraction &lt;/item&gt;<br />
    /// </summary>
    public class WienerFilter : BaseDataProcessor
    {
        double[] _prevNoise;
        double[] _prevSignal;
        double[] _prevInput;

        private const double Lambda = 0.99;
        private const double Gamma = 0.98;
        private const double EtaMin = 1e-2;
        protected AbstractVoiceActivityDetector Classifier;

        /// <summary>
        /// The name of the transform matrix file.
        /// </summary>
        [S4Component(Type = typeof(AbstractVoiceActivityDetector))]
        public static string PropClassifier = "classifier";

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            Classifier = (AbstractVoiceActivityDetector)ps.GetComponent(PropClassifier);
        }

        public override IData GetData()
        {
            IData inputData = Predecessor.GetData();

            /* signal, reset smoother */
            if (!(inputData is DoubleData))
            {
                _prevNoise = null;
                _prevSignal = null;
                _prevInput = null;
                return inputData;
            }
            DoubleData inputDoubleData = (DoubleData)inputData;
            double[] input = inputDoubleData.Values;
            int length = input.Length;

            /* no previous data, just return input */
            if (_prevNoise == null)
            {
                _prevNoise = new double[length];
                _prevSignal = new double[length];
                _prevInput = new double[length];
                return inputData;
            }

            double[] smoothedInput = Smooth(input);
            double[] noise = EstimateNoise(smoothedInput);
            double[] signal = Filter(input, smoothedInput, noise);

            Array.Copy(noise, 0, _prevNoise, 0, length);
            Array.Copy(signal, 0, _prevSignal, 0, length);
            Array.Copy(input, 0, _prevInput, 0, length);

            DoubleData outputData = new DoubleData(signal, inputDoubleData.SampleRate,
                    inputDoubleData.FirstSampleNumber);

            return outputData;
        }

        private double[] Filter(double[] input, double[] smoothedInput, double[] noise)
        {
            int length = input.Length;
            double[] signal = new double[length];

            for (int i = 0; i < length; i++)
            {
                double max = Math.Max(smoothedInput[i] - noise[i], 0);
                double s = Gamma * _prevSignal[i] + (1 - Gamma) * max;
                double eta = Math.Max(s / noise[i], EtaMin);
                signal[i] = eta / (1 + eta) * input[i];
            }
            return signal;
        }

        private double[] EstimateNoise(double[] smoothedInput)
        {
            int length = smoothedInput.Length;
            double[] noise = new double[length];

            for (int i = 0; i < length; i++)
            {
                if (Classifier.IsSpeech)
                {
                    noise[i] = _prevNoise[i];
                }
                else
                {
                    noise[i] = Lambda * _prevNoise[i] + (1 - Lambda)
                            * smoothedInput[i];
                }
            }
            return noise;
        }

        private double[] Smooth(double[] input)
        {
            int length = input.Length;
            double[] smoothedInput = new double[length];

            for (int i = 1; i < length - 1; i++)
            {
                smoothedInput[i] = (input[i] + input[i - 1] + input[i + 1] + _prevInput[i]) / 4;
            }
            smoothedInput[0] = (input[0] + input[1] + _prevInput[0]) / 3;
            smoothedInput[length - 1] = (input[length - 1] + input[length - 2] + _prevInput[length - 1]) / 3;

            return smoothedInput;
        }
    }
}
