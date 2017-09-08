using System;
using System.Collections.Generic;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    ///<summary>
    /// Applies cepstral variance normalization (CVN), so that each coefficient
    /// will have unit variance. You need to put this element after the means
    /// normalizer in frontend pipeline.
    /// <p/>
    /// CVN is sited to improve the stability of the decoding with the additive
    /// noise, so it might be useful in some situations.
    /// <see cref="LiveCMN"/>
    ///</summary>
    public class BatchVarNorm : BaseDataProcessor
    {

        private double[] _variances; // array of current sums
        private List<IData> _cepstraList;
        private int _numberDataCepstra;

        public BatchVarNorm()
        {
            //initLogger();
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
        }
        /// <summary>
        /// Initializes this BatchCMN.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _variances = null;
            _cepstraList = new List<IData>();
        }

        /// <summary>
        /// Initializes the sums array and clears the cepstra list.
        /// </summary>
        private void Reset()
        {
            _variances = null; // clears the sums array
            _cepstraList.Clear();
            _numberDataCepstra = 0;
        }

        /// <summary>
        /// Returns the next Data object, which is a normalized cepstrum. Signal objects are returned unmodified.
        /// </summary>
        /// <returns>
        /// The next available Data object, returns null if no Data object is available.
        /// </returns>
        public override IData GetData()
        {

            IData output = null;

            if (_cepstraList.Count != 0)
            {
                output = _cepstraList.Remove(0);
            }
            else
            {
                Reset();
                // read the cepstra of the entire utterance, calculate
                // and apply variance normalization
                if (ReadUtterance() > 0)
                {
                    NormalizeList();
                    output = _cepstraList.Remove(0); //getData();
                }
            }

            return output;
        }

        /// <summary>
        /// Reads the cepstra of the entire Utterance into the cepstraList.
        /// </summary>
        /// <returns>The number cepstra (with Data) read.</returns>
        private int ReadUtterance()
        {
            IData input;
            do
            {
                input = Predecessor.GetData();
                if (input != null)
                {
                    if (input is DoubleData)
                    {
                        _numberDataCepstra++;
                        double[] cepstrumData = ((DoubleData)input).Values;
                        if (_variances == null)
                        {
                            _variances = new double[cepstrumData.Length];
                        }
                        else
                        {
                            if (_variances.Length != cepstrumData.Length)
                            {
                                throw new Error
                                        ("Inconsistent cepstrum lengths: sums: " +
                                                _variances.Length + ", cepstrum: " +
                                                cepstrumData.Length);
                            }
                        }
                        // add the cepstrum data to the sums
                        for (int j = 0; j < cepstrumData.Length; j++)
                        {
                            _variances[j] += cepstrumData[j] * cepstrumData[j];
                        }
                        _cepstraList.Add(input);

                    }
                    else if (input is DataEndSignal || input is SpeechEndSignal)
                    {
                        _cepstraList.Add(input);
                        break;
                    }
                    else
                    { // DataStartSignal or other Signal
                        _cepstraList.Add(input);
                    }
                }
            } while (input != null);

            return _numberDataCepstra;
        }

        /// <summary>
        /// Normalizes the list of Data.
        /// </summary>
        private void NormalizeList()
        {

            // calculate the variance first
            for (int i = 0; i < _variances.Length; i++)
            {
                _variances[i] = Math.Sqrt(_numberDataCepstra / _variances[i]);
            }

            foreach (IData data in _cepstraList)
            {
                if (data is DoubleData)
                {
                    double[] cepstrum = ((DoubleData)data).Values;
                    for (int j = 0; j < cepstrum.Length; j++)
                    {
                        cepstrum[j] *= _variances[j];
                    }
                }
            }
        }
    }
}
