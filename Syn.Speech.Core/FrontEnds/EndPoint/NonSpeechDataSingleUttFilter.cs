using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    ///  Given a sequence of Data, filters out the non-speech regions. The sequence of
    ///  Data should have the speech and non-speech regions marked out by the
    /// SpeechStartSignal and SpeechEndSignal, using the <see cref="SpeechMarker"/>
    /// </summary>
    public class NonSpeechDataSingleUttFilter : BaseDataProcessor
    {
        private bool _inSpeech;

        public NonSpeechDataSingleUttFilter()
        {
            //initLogger();
        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
        }

        /// <summary>
        /// Initializes this DataProcessor.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _inSpeech = false;
        }


        public override IData GetData()
        {
            IData data;
            while (true)
            {
                data = ReadData();
                if (data == null || data is DataEndSignal || data is DataStartSignal || _inSpeech)
                {
                    break;
                }
            }

            return data;
        }

        private IData ReadData()
        {
            IData data = Predecessor.GetData();

            if (data is SpeechStartSignal)
            {
                _inSpeech = true;
            }

            if (data is SpeechEndSignal)
            {
                _inSpeech = false;
            }

            return data;
        }
    }
}
