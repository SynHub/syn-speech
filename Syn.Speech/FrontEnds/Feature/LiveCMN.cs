using System;
using System.Collections.Generic;
using System.Text;
using Syn.Logging;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /// <summary>
    /// Subtracts the mean of all the input so far from the Data objects.
    /// Unlike the {@link BatchCMN}, it does not read in the entire stream of Data
    /// objects before it calculates the mean. It estimates the mean from already
    /// seen data and subtracts the mean from the Data objects on the fly. Therefore,
    /// there is no delay introduced by LiveCMN in general. The only real issue is an
    /// initial CMN estimation, for that some amount of frames are read initially
    /// and cmn estimation is calculated from them.
    /// <p/>
    /// The properties that affect this processor are defined by the fields
    /// {@link #PROP_INITIAL_CMN_WINDOW}, {@link #PROP_CMN_WINDOW}, and
    /// {@link #PROP_CMN_SHIFT_WINDOW}. Please follow the link
    /// "Constant Field Values" below to see the actual name of the Sphinx
    /// properties.
    /// <p/>
    /// <p>
    /// The mean of all the input cepstrum so far is not reestimated for each
    /// cepstrum. This mean is recalculated after every
    /// {@link #PROP_CMN_SHIFT_WINDOW} cepstra. This mean is estimated by dividing
    /// the sum of all input cepstrum so far. After obtaining the mean, the sum is
    /// exponentially decayed by multiplying it by the ratio:
    /// 
    /// <pre>
    /// cmnWindow/(cmnWindow + number of frames since the last recalculation)
    /// </pre>
    /// <p/>
    /// 
    /// <see cref="BatchCMN"/> 
    ///</summary>
    public class LiveCMN:BaseDataProcessor
    {
        private string formatter;
        //= new DecimalFormat("0.00;-0.00", new DecimalFormatSymbols(Locale.US));;

        /// <summary>
        /// The property for the live CMN initial window size.
        /// </summary>
        [S4Integer(DefaultValue = 200)]
        public static string PropInitialCMNWindow = "initialCmnWindow";
        private int _initialCmnWindow;

        /// <summary>
        /// The property for the live CMN window size.
        /// </summary>
        [S4Integer(DefaultValue = 300)]
        public static string PropCMNWindow = "cmnWindow";
        private int _cmnWindow;

        /// <summary>
        /// The property for the CMN shifting window. The shifting window specifies
        /// how many cepstrum after which we re-calculate the cepstral mean.
        /// </summary>
        [S4Integer(DefaultValue = 400)]
        public static string PropCMNShiftWindow = "shiftWindow";
        private int _cmnShiftWindow; // # of Cepstrum to recalculate mean

        private double[] _currentMean; // array of current means
        private double[] _sum; // array of current sums
        private int _numberFrame; // total number of input Cepstrum

        List<IData> _initialList;

        public LiveCMN(double initialMean, int cmnWindow, int cmnShiftWindow, int initialCmnWindow) 
        {
            _cmnWindow = cmnWindow;
            _cmnShiftWindow = cmnShiftWindow;
            _initialCmnWindow = initialCmnWindow;
        }

        public LiveCMN() {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _cmnWindow = ps.GetInt(PropCMNWindow);
            _cmnShiftWindow = ps.GetInt(PropCMNShiftWindow);
            _initialCmnWindow = ps.GetInt(PropInitialCMNWindow);
        }

        /// <summary>
        /// Initializes this LiveCMN.
        /// </summary>
        public override void Initialize() 
        {
            base.Initialize();
        }

        /// <summary>
        /// Initializes the currentMean and sum arrays with the given cepstrum
        /// length.
        /// </summary>
        private void InitMeansSums() {
            int size = -1;
        
            foreach (IData data in _initialList) 
            {
                if (!(data is DoubleData))
                    continue;
        
                double[] cepstrum = ((DoubleData) data).Values;
            
                // Initialize arrays if needed
                if (size < 0) {
                    size = cepstrum.Length;
                    _sum = new double[size];
                    _numberFrame = 0;
                }

                // Accumulate cepstrum, avoid counting zero energy in CMN
                if (cepstrum[0] >= 0)
                {
                    for (int j = 0; j < size; j++)
                    {
                        _sum[j] += cepstrum[j];
                    }
                    _numberFrame++;
                }
            }

            // If we didn't meet any data, do nothing
            if (size < 0)
                return;

            _currentMean = new double[size];
            for (int j = 0; j < size; j++) {
                _currentMean[j] = _sum[j] / _numberFrame;
            }
        }

        /// <summary>
        /// Returns the next Data object, which is a normalized Data produced by this
        /// class. Signals are returned unmodified.
        /// </summary>
        /// <returns>
        /// The next available Data object, returns null if no Data object is available.
        /// </returns>
        public override IData GetData()
        {

            IData input, output;

             if (_initialList == null) 
            {
                _initialList = new List<IData>();
                // Collect initial data for estimation
                while (_initialList.Count < _initialCmnWindow) 
                {
                    input = Predecessor.GetData();
                    _initialList.Add(input);
                    if (input is SpeechEndSignal
                            || input is DataEndSignal)
                        break;
                }
                InitMeansSums();
                output = _initialList.Remove(0);
            } 
            else if (_initialList.Count!=0) {
                // Return the previously collected data
                output = _initialList.Remove(0);
            } else {
                // Process normal frame
                output = Predecessor.GetData();
            }

            Normalize(output);
            this.LogDebug("getData value: {0}", output);
            return output;
        }

        /// <summary>
        /// Normalizes the given Data with using the currentMean array. Updates the
        /// sum array with the given Data.
        /// </summary>
        /// <param name="data">The Data object to normalize.</param>
        private void Normalize(IData data) 
        {
            if (!(data is DoubleData))
                return;

            double[] cepstrum = ((DoubleData) data).Values;

            if (cepstrum.Length != _sum.Length) {
                throw new Exception("Data length (" + cepstrum.Length + ") not equal sum array length (" + _sum.Length + ')');
            }

            // Accumulate cepstrum, avoid counting zero energy in CMN
            if (cepstrum[0] >= 0)
            {
                for (int j = 0; j < cepstrum.Length; j++)
                {
                    _sum[j] += cepstrum[j];
                }
                _numberFrame++;
            }

            // Subtract current mean
            for (int j = 0; j < cepstrum.Length; j++)
            {
                cepstrum[j] -= _currentMean[j];
            }

            if (_numberFrame > _cmnShiftWindow)
            {

                var cmn = new StringBuilder();
                // calculate the mean first
                for (int i = 0; i < _currentMean.Length; i++)
                {
                    cmn.Append(_currentMean[i].ToString("#.##;-#.##"));
                    cmn.Append(' ');
                }
                this.LogInfo(cmn.ToString());

                UpdateMeanSumBuffers();
            }
        }

        /// <summary>
        /// Updates the currentMean buffer with the values in the sum buffer. Then
        /// decay the sum buffer exponentially, i.e., divide the sum with
        /// numberFrames.
        /// </summary>
        private void UpdateMeanSumBuffers() {

            // update the currentMean buffer with the sum buffer
            double sf = 1.0 / _numberFrame;

            Array.Copy(_sum, 0, _currentMean, 0, _sum.Length);

            MultiplyArray(_currentMean, sf);

            // decay the sum buffer exponentially
            if (_numberFrame >= _cmnShiftWindow) {
                MultiplyArray(_sum, (sf* _cmnWindow));
                _numberFrame = _cmnWindow;
            }
        }


        /// <summary>
        /// Multiplies each element of the given array by the multiplier.
        /// </summary>
        /// <param name="array">The array to multiply.</param>
        /// <param name="multiplier">The amount to multiply by.</param>
        private static void MultiplyArray(double[] array, double multiplier) {
            for (int i = 0; i < array.Length; i++) {
                array[i] *= multiplier;
            }
        }
    }
}
