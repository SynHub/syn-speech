using System;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Denoises
{
    /// <summary>
    /// The noise filter, same as implemented in sphinxbase/sphinxtrain/pocketsphinx.
    /// Noise removal algorithm is inspired by the following papers Computationally
    /// Efficient Speech Enchancement by Spectral Minina Tracking by G. Doblinger
    /// Power-Normalized Cepstral Coefficients (PNCC) for Robust Speech Recognition
    /// by C. Kim.
    /// For the recent research and state of art see papers about IMRCA and A
    /// Minimum-Mean-Square-Error Noise Reduction Algorithm On Mel-Frequency Cepstra
    /// For Robust Speech Recognition by Dong Yu and others
    /// </summary>
    public class Denoise:BaseDataProcessor
    {
        double[] _power;
        double[] _noise;
        double[] _floor;
        double[] _peak;

        [S4Double(DefaultValue = 0.7)]
        public static string LambdaPower = "lambdaPower";
        double _lambdaPower;

        [S4Double(DefaultValue = 0.995)]
        public static string LambdaA = "lambdaA";
        double _lambdaA;

        [S4Double(DefaultValue = 0.5)]
        public static string LambdaB = "lambdaB";
        double _lambdaB;

        [S4Double(DefaultValue = 0.85)]
        public static string LambdaT = "lambdaT";
        double lambdaT;

        [S4Double(DefaultValue = 0.2)]
        public static string MuT = "muT";
        double _muT;

        [S4Double(DefaultValue = 20.0)]
        public static string MaxGain = "maxGain";
        double _maxGain;

        [S4Integer(DefaultValue = 4)]
        public static string SmoothWindow = "smoothWindow";
        int _smoothWindow;

        static double EPS = 1e-10;

        public Denoise(double lambdaPower, double lambdaA, double lambdaB,
                double lambdaT, double muT, 
                double maxGain, int smoothWindow) {
            _lambdaPower = lambdaPower;
            _lambdaA = lambdaA;
            _lambdaB = lambdaB;
            this.lambdaT = lambdaT;
            _muT = muT;
            _maxGain = maxGain;
            _smoothWindow = smoothWindow;
        }

        public Denoise() {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _lambdaPower = ps.GetDouble(LambdaPower);
            _lambdaA = ps.GetDouble(LambdaA);
            _lambdaB = ps.GetDouble(LambdaB);
            lambdaT = ps.GetDouble(LambdaT);
            _muT = ps.GetDouble(MuT);
            _maxGain = ps.GetDouble(MaxGain);
            _smoothWindow = ps.GetInt(SmoothWindow);
        }


        public override IData GetData()
        {
            var inputData = Predecessor.GetData();
            int i;

            if (inputData is DataStartSignal) {
                _power = null;
                _noise = null;
                _floor = null;
                _peak = null;
                return inputData;
            }
            if (!(inputData is DoubleData)) {
                return inputData;
            }

            var inputDoubleData = (DoubleData) inputData;
            var input = inputDoubleData.Values;
            var length = input.Length;

            if (_power == null)
                InitStatistics(input, length);

            UpdatePower(input);

            EstimateEnvelope(_power, _noise);

            var signal = new double[length];
            for (i = 0; i < length; i++) {
                signal[i] = Math.Max(_power[i] - _noise[i], 0.0);
            }

            EstimateEnvelope(signal, _floor);

            TempMasking(signal);

            PowerBoosting(signal);

            var gain = new double[length];
            for (i = 0; i < length; i++) {
                gain[i] = signal[i] / (_power[i] + EPS);
                gain[i] = Math.Min(Math.Max(gain[i], 1.0 / _maxGain), _maxGain);
            }
            var smoothGain = Smooth(gain);

            for (i = 0; i < length; i++) {
                input[i] *= smoothGain[i];
            }

            return inputData;
        }

        private double[] Smooth(double[] gain) 
        {
            var result = new double[gain.Length];
            for (var i = 0; i < gain.Length; i++) {
                var start = Math.Max(i - _smoothWindow, 0);
                var end = Math.Min(i + _smoothWindow + 1, gain.Length);
                var sum = 0.0;
                for (var j = start; j < end; j++) {
                    sum += gain[j];
                }
                result[i] = sum / (end - start);
            }
            return result;
        }

        private void PowerBoosting(double[] signal) 
        {
            for (var i = 0; i < signal.Length; i++) 
            {
                if (signal[i] < _floor[i])
                    signal[i] = _floor[i];
            }
        }

        private void TempMasking(double[] signal) 
        {
            for (var i = 0; i < signal.Length; i++) 
            {
                var _in = signal[i];

                _peak[i] *= lambdaT;
                if (signal[i] < lambdaT* _peak[i])
                    signal[i] = _peak[i]* _muT;

                if (_in > _peak[i])
                    _peak[i] = _in;
            }
        }

        private void UpdatePower(double[] input) 
        {
            for (var i = 0; i < input.Length; i++) 
            {
                _power[i] = _lambdaPower* _power[i] + (1 - _lambdaPower)* input[i];
            }
        }

        private void EstimateEnvelope(double[] signal, double[] envelope) 
        {
            for (var i = 0; i < signal.Length; i++) 
            {
                if (signal[i] > envelope[i])
                    envelope[i] = _lambdaA* envelope[i] + (1 - _lambdaA)* signal[i];
                else
                    envelope[i] = _lambdaB* envelope[i] + (1 - _lambdaB)* signal[i];
            }
        }

        private void InitStatistics(double[] input, int length) 
        {
            /* no previous data, initialize the statistics */
            _power = Arrays.copyOf(input, length);
            _noise = Arrays.copyOf(input, length);
            _floor = new double[length];
            _peak = new double[length];
            for (var i = 0; i < length; i++) 
            {
                _floor[i] = input[i] / _maxGain;
            }
        }
    }
}
