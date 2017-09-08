using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    ///<summary>
    /// Converts a stream of SpeechClassifiedData objects, marked as speech and
    /// non-speech, and mark out the regions that are considered speech. This is done
    /// by inserting SPEECH_START and SPEECH_END signals into the stream.
    /// <p/>
    /// <p>
    /// The algorithm for inserting the two signals is as follows.
    /// <p/>
    /// <p>
    /// The algorithm is always in one of two states: 'in-speech' and
    /// 'out-of-speech'. If 'out-of-speech', it will read in audio until we hit audio
    /// that is speech. If we have read more than 'startSpeech' amount of
    /// <i>continuous</i> speech, we consider that speech has started, and insert a
    /// SPEECH_START at 'speechLeader' time before speech first started. The state of
    /// the algorithm changes to 'in-speech'.
    /// <p/>
    /// <p>
    /// Now consider the case when the algorithm is in 'in-speech' state. If it read
    /// an audio that is speech, it is scheduled for output. If the audio is non-speech, we read
    /// ahead until we have 'endSilence' amount of <i>continuous</i> non-speech. At
    /// the point we consider that speech has ended. A SPEECH_END signal is inserted
    /// at 'speechTrailer' time after the first non-speech audio. The algorithm
    /// returns to 'out-of-speech' state. If any speech audio is encountered
    /// in-between, the accounting starts all over again.
    /// 
    /// While speech audio is processed delay is lowered to some minimal amount. This helps
    /// to segment both slow speech with visible delays and fast speech when delays are minimal.
    ///</summary>
    public class SpeechMarker : BaseDataProcessor
    {
        /// <summary>
        /// The property for the minimum amount of time in speech (in milliseconds) to be considered as utterance start.
        /// </summary>
        [S4Integer(DefaultValue = 200)]
        public static string PropStartSpeech = "startSpeech";
        private int _startSpeechTime;

        /// <summary>
        /// The property for the amount of time in silence (in milliseconds) to be considered as utterance end.
        /// </summary>
        [S4Integer(DefaultValue = 200)]//Todo: This value should be changed whenever large silence is to be accepted. Old value was 500
        public static string PropEndSilence = "endSilence";
        private int _endSilenceTime;

        /// <summary>
        ///The property for the amount of time (in milliseconds) before speech start to be included as speech data.
        /// </summary>
        [S4Integer(DefaultValue = 50)]
        public static string PropSpeechLeader = "speechLeader";
        private int _speechLeader;



        private LinkedList<IData> _inputQueue; // Audio objects are added to the end
        private LinkedList<IData> _outputQueue;
 
        private int _speechCount;
        private int _silenceCount;
        private int _startSpeechFrames;
        private int _endSilenceFrames;
        private int _speechLeaderFrames;

        public SpeechMarker(int startSpeechTime, int endSilenceTime, int speechLeader)
        {
            _startSpeechTime = startSpeechTime;
            _endSilenceTime = endSilenceTime;
            _speechLeader = speechLeader;
        }

        public SpeechMarker()
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            _startSpeechTime = ps.GetInt(PropStartSpeech);
            _endSilenceTime = ps.GetInt(PropEndSilence);
            _speechLeader = ps.GetInt(PropSpeechLeader);

        }

        /// <summary>
        ///  Initializes this SpeechMarker.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            Reset();
        }

        /// <summary>
        /// Resets this SpeechMarker to a starting state.
        /// </summary>
        private void Reset()
        {
            InSpeech = false;
            _speechCount = 0;
            _silenceCount = 0;
            _startSpeechFrames = _startSpeechTime / 10;
            _endSilenceFrames = _endSilenceTime / 10;
            _speechLeaderFrames = _speechLeader/10;
            _inputQueue = new LinkedList<IData>();
            _outputQueue = new LinkedList<IData>();
        }

        /// <summary>
        /// Returns the next Data object.
        /// </summary>
        /// <returns>
        /// The next Data object, or null if none available.
        /// </returns>
        public override IData GetData()
        {
            while (_outputQueue.IsEmpty())
            {
                IData data = Predecessor.GetData();

                if (data == null)
                    break;

                if (data is DataStartSignal)
                {
                    Reset();
                    _outputQueue.Add(data);
                    break;
                }

                if (data is DataEndSignal)
                {
                    if (InSpeech)
                    {
                        _outputQueue.Add(new SpeechEndSignal());
                    }
                    #region Extra
                    //TODO: THIS HAS BEEN ADDED BECAUSE RESIDUE DATAENDSIGNALS ARE NOT CLEARED UPON NEW SPEECH RECOGNITION SESSION
                    //TODO: REPORT THIS BEHAVIOUR TO CMU SPHINX DEVELOPERS
                    else
                    {
                        _outputQueue.Add(data);
                        break;
                    }
                    #endregion
                    #region Actual
                    //_outputQueue.Add(data);
                    //break;
                    #endregion
                }

                if (data is SpeechClassifiedData)
                {
                    var cdata = (SpeechClassifiedData) data;

                    if (cdata.IsSpeech)
                    {
                        _speechCount++;
                        _silenceCount = 0;
                    }
                    else
                    {
                        _speechCount = 0;
                        _silenceCount++;
                    }

                    if (InSpeech)
                    {
                        _outputQueue.Add(data);
                    }
                    else
                    {
                        _inputQueue.Add(data);
                        if (_inputQueue.Count > _startSpeechFrames + _speechLeaderFrames)
                        {
                            _inputQueue.Remove(0);
                        }
                    }

                    if (!InSpeech && _speechCount == _startSpeechFrames)
                    {
                        InSpeech = true;
                        _outputQueue.Add(new SpeechStartSignal(cdata.CollectTime-_speechLeader - _startSpeechFrames));
                        _outputQueue.AddAll(_inputQueue.SubList(
                            Math.Max(0, _inputQueue.Count - _startSpeechFrames - _speechLeaderFrames), _inputQueue.Count));
                        _inputQueue.Clear();
                    }

                    if (InSpeech && _silenceCount == _endSilenceFrames)
                    {
                        InSpeech = false;
                        _outputQueue.Add(new SpeechEndSignal(cdata.CollectTime));
                    }
                }
            }

            //if we have something left, return that
            if (!_outputQueue.IsEmpty())
            {
                IData audio = _outputQueue.Remove(0);
                if (audio is SpeechClassifiedData)
                {
                    var data = (SpeechClassifiedData) audio;
                    audio = data.DoubleData;
                }
                return audio;
            }
            return null;
        }


        public bool InSpeech { get; private set; }
    }
}
