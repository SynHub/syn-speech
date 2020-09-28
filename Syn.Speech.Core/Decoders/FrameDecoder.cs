using System.Collections.Generic;
using Syn.Speech.Decoders.Search;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Results;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders
{
    public class FrameDecoder : AbstractDecoder, IDataProcessor
    {
        private bool _isRecognizing;
        private Result _result;

        public FrameDecoder(ISearchManager searchManager, bool fireNonFinalResults, bool autoAllocate,
            List<IResultListener> listeners)
            : base(searchManager, fireNonFinalResults, autoAllocate, listeners)
        {

        }

        public FrameDecoder() { }

        public override Result Decode(string referenceText)
        {
            return SearchManager.Recognize(1);
        }

        public IData GetData()
        {
            IData d = Predecessor.GetData();
            if (_isRecognizing && (d is FloatData || d is DoubleData || d is SpeechEndSignal))
            {
                _result = Decode(null);
                if (_result != null)
                {
                    FireResultListeners(_result);
                    _result = null;
                }
            }

            // we also trigger recogntion on a DataEndSignal to allow threaded scorers to shut down correctly
            if (d is DataEndSignal)
            {
                SearchManager.StopRecognition();
            }

            if (d is SpeechStartSignal)
            {
                SearchManager.StartRecognition();
                _isRecognizing = true;
                _result = null;
            }

            if (d is SpeechEndSignal)
            {
                SearchManager.StopRecognition();

                //fire results which were not yet final
                if (_result != null)
                    FireResultListeners(_result);

                _isRecognizing = false;
            }

            return d;
        }

        public IDataProcessor Predecessor { get; set; }

        public void Initialize() {}
    }
}
