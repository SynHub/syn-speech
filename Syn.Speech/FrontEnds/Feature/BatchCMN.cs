using System;
using System.Collections.Generic;
using System.Text;
using Syn.Speech.Logging;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /// <summary>
    ///  Applies cepstral mean normalization (CMN), sometimes called channel mean normalization, to incoming cepstral data.
    ///
    /// Its goal is to reduce the distortion caused by the transmission channel.  The output is mean normalized cepstral
    /// data.
    /// <p/>
    /// The CMN processing subtracts the mean from all the {@link Data} objects between a {@link
    /// edu.cmu.sphinx.frontend.DataStartSignal} and a {@link DataEndSignal} or between a {@link
    /// edu.cmu.sphinx.frontend.endpoint.SpeechStartSignal} and a {@link SpeechEndSignal}.  BatchCMN will read in all the {@link Data}
    /// objects, calculate the mean, and subtract this mean from all the {@link Data} objects. For a given utterance, it will
    /// only produce an output after reading all the incoming data for the utterance. As a result, this process can introduce
    /// a significant processing delay, which is acceptable for batch processing, but not for live mode. In the latter case,
    /// one should use the {@link LiveCMN}.
    /// <p/>
    /// CMN is a technique used to reduce distortions that are introduced by the transfer function of the transmission
    /// channel (e.g., the microphone). Using a transmission channel to transmit the input speech translates to multiplying
    /// the spectrum of the input speech with the transfer function of the channel (the distortion).  Since the cepstrum is
    /// the Fourier Transform of the log spectrum, the logarithm turns the multiplication into a summation. Averaging over
    /// time, the mean is an estimate of the channel, which remains roughly constant. The channel is thus removed from the
    /// cepstrum by subtracting the mean cepstral vector. Intuitively, the mean cepstral vector approximately describes the
    /// spectral characteristics of the transmission channel (e.g., microphone).
    ///
    /// @see LiveCMN
    /// </summary>
    public class BatchCMN : BaseDataProcessor
    {
        private double[] _sums;           // array of current sums
        private List<IData> _cepstraList;
        private int _numberDataCepstra;
        //private DecimalFormat formatter = new DecimalFormat("0.00;-0.00", new DecimalFormatSymbols(Locale.US));;

        public BatchCMN()
        {
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
            _sums = null;
            _cepstraList = new List<IData>();
        }


        /// <summary>
        /// Initializes the sums array and clears the cepstra list. 
        /// </summary>
        private void Reset()
        {
            _sums = null; // clears the sums array
            _cepstraList.Clear();
            _numberDataCepstra = 0;
        }

        /// <summary>
        /// Returns the next Data object, which is a normalized cepstrum. Signal objects are returned unmodified.
        /// </summary>
        /// <returns>the next available Data object, returns null if no Data object is available</returns>
        public override IData GetData()
        {
            IData output = null;

            if (_cepstraList.Count != 0)
            {
                output = _cepstraList[0];
                _cepstraList.RemoveAt(0);
            }
            else
            {
                Reset();
                // read the cepstra of the entire utterance, calculate
                // and apply the cepstral mean
                if (ReadUtterance() > 0)
                {
                    NormalizeList();
                    output = _cepstraList[0];
                    _cepstraList.RemoveAt(0);//getData();
                }
            }

            return output;
        }

        /// <summary>
        /// Reads the cepstra of the entire Utterance into the cepstraList.
        /// </summary>
        /// <returns>the number cepstra (with Data) read</returns>
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
                        if (_sums == null)
                        {
                            _sums = new double[cepstrumData.Length];
                        }
                        else
                        {
                            if (_sums.Length != cepstrumData.Length)
                            {
                                throw new
                                    Exception
                                        ("Inconsistent cepstrum lengths: sums: " +
                                                _sums.Length + ", cepstrum: " +
                                                cepstrumData.Length);
                            }
                        }
                        // add the cepstrum data to the sums
                        for (int j = 0; j < cepstrumData.Length; j++)
                        {
                            _sums[j] += cepstrumData[j];
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
            StringBuilder cmn = new StringBuilder();
            // calculate the mean first
            for (int i = 0; i < _sums.Length; i++)
            {
                _sums[i] /= _numberDataCepstra;
                cmn.Append(String.Format("{0}", _sums[i]));
                cmn.Append(' ');
            }
            this.LogInfo(cmn.ToString());

            foreach (IData data in _cepstraList)
            {
                if (data is DoubleData)
                {
                    double[] cepstrum = ((DoubleData)data).Values;
                    for (int j = 0; j < cepstrum.Length; j++)
                    {
                        cepstrum[j] -= _sums[j]; // sums[] is now the means[]
                    }
                }
            }
        }
    }
}
