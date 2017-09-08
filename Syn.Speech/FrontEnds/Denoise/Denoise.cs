using System;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Util.Props;

namespace Syn.Speech.FrontEnd.Denoise
{
    /**
    /// The noise filter, same as implemented in sphinxbase/sphinxtrain/pocketsphinx.
    /// 
    /// Noise removal algorithm is inspired by the following papers Computationally
    /// Efficient Speech Enchancement by Spectral Minina Tracking by G. Doblinger
    /// 
    /// Power-Normalized Cepstral Coefficients (PNCC) for Robust Speech Recognition
    /// by C. Kim.
    /// 
    /// For the recent research and state of art see papers about IMRCA and A
    /// Minimum-Mean-Square-Error Noise Reduction Algorithm On Mel-Frequency Cepstra
    /// For Robust Speech Recognition by Dong Yu and others
    /// 
     */
    public class Denoise:BaseDataProcessor
    {
        double[] power;
        double[] noise;
        double[] floor;
        double[] peak;

        [S4Double(defaultValue = 0.7)]
        public static String LAMBDA_POWER = "lambdaPower";
        double lambdaPower;

        [S4Double(defaultValue = 0.999)]
        public static String LAMBDA_A = "lambdaA";
        double lambdaA;

        [S4Double(defaultValue = 0.5)]
        public static String LAMBDA_B = "lambdaB";
        double lambdaB;

        [S4Double(defaultValue = 0.85)]
        public static String LAMBDA_T = "lambdaT";
        double lambdaT;

        [S4Double(defaultValue = 0.2)]
        public static String MU_T = "muT";
        double muT;

        [S4Double(defaultValue = 2.0)]
        public static String EXCITATION_THRESHOLD = "excitationThreshold";
        double excitationThreshold;

        [S4Double(defaultValue = 20.0)]
        public static String MAX_GAIN = "maxGain";
        double maxGain;

        [S4Integer(defaultValue = 4)]
        public static String SMOOTH_WINDOW = "smoothWindow";
        int smoothWindow;

        static double EPS = 1e-10;

        public Denoise(double lambdaPower, double lambdaA, double lambdaB,
                double lambdaT, double muT, double excitationThreshold,
                double maxGain, int smoothWindow) {
            this.lambdaPower = lambdaPower;
            this.lambdaA = lambdaA;
            this.lambdaB = lambdaB;
            this.lambdaT = lambdaT;
            this.muT = muT;
            this.excitationThreshold = excitationThreshold;
            this.maxGain = maxGain;
            this.smoothWindow = smoothWindow;
        }

        public Denoise() {
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see
        /// edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util
        /// .props.PropertySheet)
         */
        override
        public void newProperties(PropertySheet ps)
        {
            base.newProperties(ps);
            lambdaPower = ps.getDouble(LAMBDA_POWER);
            lambdaA = ps.getDouble(LAMBDA_A);
            lambdaB = ps.getDouble(LAMBDA_B);
            lambdaT = ps.getDouble(LAMBDA_T);
            muT = ps.getDouble(MU_T);
            excitationThreshold = ps.getDouble(EXCITATION_THRESHOLD);
            maxGain = ps.getDouble(MAX_GAIN);
            smoothWindow = ps.getInt(SMOOTH_WINDOW);
        }


        public IData getData()
        {
            IData inputData = getPredecessor().getData();
            int i;

            if (inputData is DataStartSignal) {
                power = null;
                noise = null;
                floor = null;
                peak = null;
                return inputData;
            }
            if (!(inputData is DoubleData)) {
                return inputData;
            }

            DoubleData inputDoubleData = (DoubleData) inputData;
            double[] input = inputDoubleData.getValues();
            int length = input.Length;

            if (power == null)
                initStatistics(input, length);

            updatePower(input);

            estimateEnvelope(power, noise);

            double[] signal = new double[length];
            for (i = 0; i < length; i++) {
                signal[i] = Math.Max(power[i] - noise[i], 0.0);
            }

            estimateEnvelope(signal, floor);

            tempMasking(signal);

            powerBoosting(signal);

            double[] gain = new double[length];
            for (i = 0; i < length; i++) {
                gain[i] = signal[i] / (power[i] + EPS);
                gain[i] = Math.Min(Math.Max(gain[i], 1.0 / maxGain), maxGain);
            }
            double[] smoothGain = smooth(gain);

            for (i = 0; i < length; i++) {
                input[i] *= smoothGain[i];
            }

            return inputData;
        }

        private double[] smooth(double[] gain) 
        {
            double[] result = new double[gain.Length];
            for (int i = 0; i < gain.Length; i++) {
                int start = Math.Max(i - smoothWindow, 0);
                int end = Math.Min(i + smoothWindow + 1, gain.Length);
                double sum = 0.0;
                for (int j = start; j < end; j++) {
                    sum += gain[j];
                }
                result[i] = sum / (end - start);
            }
            return result;
        }

        private void powerBoosting(double[] signal) 
        {
            for (int i = 0; i < signal.Length; i++) 
            {
                if (signal[i] < floor[i])
                    signal[i] = floor[i];
                if (signal[i] < excitationThreshold* noise[i])
                    signal[i] = floor[i];
            }
        }

        private void tempMasking(double[] signal) 
        {
            for (int i = 0; i < signal.Length; i++) 
            {
                double _in = signal[i];

                peak[i] *= lambdaT;
                if (signal[i] < lambdaT* peak[i])
                    signal[i] = peak[i]* muT;

                if (_in > peak[i])
                    peak[i] = _in;
            }
        }

        private void updatePower(double[] input) 
        {
            for (int i = 0; i < input.Length; i++) 
            {
                power[i] = lambdaPower* power[i] + (1 - lambdaPower)* input[i];
            }
        }

        private void estimateEnvelope(double[] signal, double[] envelope) 
        {
            for (int i = 0; i < signal.Length; i++) 
            {
                if (signal[i] > envelope[i])
                    envelope[i] = lambdaA* envelope[i] + (1 - lambdaA)* signal[i];
                else
                    envelope[i] = lambdaB* envelope[i] + (1 - lambdaB)* signal[i];
            }
        }

        private void initStatistics(double[] input, int length) 
        {
            /* no previous data, initialize the statistics */
            input.CopyTo(power, length);
            input.CopyTo(noise, length);
            floor = new double[length];
            peak = new double[length];
            for (int i = 0; i < length; i++) 
            {
                floor[i] = input[i] / maxGain;
            }
        }

    }
}
