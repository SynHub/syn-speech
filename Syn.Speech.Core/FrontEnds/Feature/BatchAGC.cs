using System.Collections.Generic;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Util.Props;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{

    /// <summary>
    /// Applies automatic gain control (CMN)
    /// </summary>
    public class BatchAGC : BaseDataProcessor
    {
        private List<IData> _cepstraList;
        private double _agc;

        public BatchAGC()
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
            _cepstraList = new List<IData>();
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
                _agc = 0.0;
                _cepstraList.Clear();
                // read the cepstra of the entire utterance, calculate and substract gain
                if (ReadUtterance() > 0)
                {
                    NormalizeList();
                    output = _cepstraList.Remove(0);
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

            IData input = null;
            int numFrames = 0;

            while (true)
            {
                input = Predecessor.GetData();
                if (input == null)
                {
                    break;
                }
                else if (input is DataEndSignal || input is SpeechEndSignal)
                {
                    _cepstraList.Add(input);
                    break;
                }
                else if (input is DoubleData)
                {
                    _cepstraList.Add(input);
                    double c0 = ((DoubleData)input).Values[0];
                    if (_agc < c0)
                        _agc = c0;
                }
                else
                { // DataStartSignal or other Signal
                    _cepstraList.Add(input);
                }
                numFrames++;
            }

            return numFrames;
        }

        /// <summary>
        /// Normalizes the list of Data.
        /// </summary>
        private void NormalizeList()
        {
            foreach (IData data in _cepstraList)
            {
                if (data is DoubleData)
                {
                    ((DoubleData)data).Values[0] -= _agc;
                }
            }
        }
    }

}
