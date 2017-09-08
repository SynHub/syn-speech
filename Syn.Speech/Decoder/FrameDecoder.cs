using System.Collections.Generic;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Decoder.Search;
using Syn.Speech.FrontEnd;
using Syn.Speech.FrontEnd.EndPoint;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder
{
    public class FrameDecoder : AbstractDecoder, IDataProcessor
    {
        private IDataProcessor predecessor;

        private bool isRecognizing;
        private Results.Result result;

        public FrameDecoder(ISearchManager searchManager, bool fireNonFinalResults, bool autoAllocate,
            List<IResultListener> listeners)
            : base(searchManager, fireNonFinalResults, autoAllocate, listeners)
        {

        }

        public FrameDecoder() { }

        public override Results.Result decode(string referenceText)
        {
            return searchManager.recognize(1);
        }

        public IData getData()
        {
            IData d = getPredecessor().getData();
            if (isRecognizing && (d is FloatData || d is DoubleData || d is SpeechEndSignal))
            {
                result = decode(null);
                if (result != null)
                {
                    fireResultListeners(result);
                    result = null;
                }
            }

            // we also trigger recogntion on a DataEndSignal to allow threaded scorers to shut down correctly
            if (d is DataEndSignal)
            {
                searchManager.stopRecognition();
            }

            if (d is SpeechStartSignal)
            {
                searchManager.startRecognition();
                isRecognizing = true;
                result = null;
            }

            if (d is SpeechEndSignal)
            {
                searchManager.stopRecognition();

                //fire results which were not yet final
                if (result != null)
                    fireResultListeners(result);

                isRecognizing = false;
            }

            return d;
        }

        public override void newProperties(PropertySheet ps)
        {
            //TODO: THIS DOESN'T EXIST in SPHINX4
        }

        public IDataProcessor getPredecessor()
        {
            return predecessor;
        }

        public void setPredecessor(IDataProcessor predecessor)
        {
            this.predecessor = predecessor;
        }

        public void initialize() {}
    }
}
