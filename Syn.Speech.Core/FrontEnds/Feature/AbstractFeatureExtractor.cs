using System;
using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Api;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Logging;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /// <summary>
    /// Abstract base class for windowed feature extractors like DeltasFeatureExtractor, ConcatFeatureExtractor
    /// or S3FeatureExtractor. The main purpose of this it to collect window size cepstra frames in a buffer
    /// and let the extractor compute the feature frame with them.
    /// </summary>
    public abstract class AbstractFeatureExtractor: BaseDataProcessor
    {
        /** The property for the window of the DeltasFeatureExtractor. */
        [S4Integer(DefaultValue = 3)]
        public static string PropFeatureWindow = "windowSize";

        private int _bufferPosition;
        private Signal _pendingSignal;
        private List<IData> _outputQueue = new List<IData>();

        protected int CepstraBufferEdge;
        protected int Window;
        protected int CurrentPosition;
        protected int CepstraBufferSize;
        protected DoubleData[] CepstraBuffer;

        public AbstractFeatureExtractor( int window ) 
        {
            Window = window;
        }

        public AbstractFeatureExtractor() {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            Window = ps.GetInt(PropFeatureWindow);
        }

        /// <summary>
        /// @see Sphincs.frontend.DataProcessor#initialize(Sphinx.frontend.CommonConfig)
        /// </summary>
        public override void Initialize() 
        {
            base.Initialize();
            CepstraBufferSize = 256;
            CepstraBuffer  = new DoubleData[CepstraBufferSize];
            CepstraBufferEdge = CepstraBufferSize - (Window* 2 + 2);
            _outputQueue = new List<IData>();
            Reset();
        }


        /// <summary>
        /// Resets the DeltasFeatureExtractor to be ready to read the next segment of data.
        /// </summary>
        private void Reset() 
        {
            _bufferPosition = 0;
            CurrentPosition = 0;
        }

        /// <summary>
        /// Returns the next Data object produced by this DeltasFeatureExtractor.
        /// </summary>
        /// <returns>the next available Data object, returns null if no Data is available</returns>
        public override IData GetData()
        {
            if (_outputQueue.Count==0) 
            {
                IData input = GetNextData();
                if (input != null) 
                {
                    if (input is DoubleData) 
                    {
                        AddCepstrum((DoubleData) input);
                        ComputeFeatures(1);
                    } 
                    else if (input is DataStartSignal) 
                    {
                        _pendingSignal = null;
                        _outputQueue.Add(input);
                        IData start = GetNextData();
                        int n = ProcessFirstCepstrum(start);
                        ComputeFeatures(n);
                        if (_pendingSignal != null) 
                        {
                            _outputQueue.Add(_pendingSignal);
                        }
                    } 
                    else if (input is SpeechEndSignal) 
                    {
                        // when the DataEndSignal is right at the boundary
                        int n = ReplicateLastCepstrum();
                        ComputeFeatures(n);
                        _outputQueue.Add(input);
                    }
                    else if (input is DataEndSignal)
                    {
                        _outputQueue.Add(input);
                    }
                }
            }
            if(_outputQueue.Count==0)
                return null;
            IData ret = _outputQueue[0];
            _outputQueue.RemoveAt(0);
            this.LogDebug("getData value: {0}", ret);
            return ret;
        }

        private IData GetNextData() 
        {
            IData d = Predecessor.GetData();
            while (d != null && !(d is DoubleData || d is DataEndSignal || d is DataStartSignal || d is SpeechEndSignal)) {
                _outputQueue.Add(d);
                d = Predecessor.GetData();
            }

            return d;
        }

        /// <summary>
        /// Replicate the given cepstrum Data object into the first window+1 number of frames in the cepstraBuffer. 
        /// This is the first cepstrum in the segment.
        /// </summary>
        /// <param name="cepstrum">The Data to replicate.</param>
        /// <returns>The number of Features that can be computed.</returns>
        /// <exception cref="System.Exception">
        /// Too many UTTERANCE_START
        /// or
        /// Too many UTTERANCE_START
        /// </exception>
        private int ProcessFirstCepstrum(IData cepstrum)
        {
            if (cepstrum is DataEndSignal) 
            {
                _outputQueue.Add(cepstrum);
                return 0;
            }
            if (cepstrum is DataStartSignal) 
            {
                throw new Exception("Too many UTTERANCE_START");
            } 
            else {
                // At the start of an utterance, we replicate the first frame
                // into window+1 frames, and then read the next "window" number
                // of frames. This will allow us to compute the delta-
                // double-delta of the first frame.
                //Arrays.fill(cepstraBuffer, 0, window + 1, cepstrum);
                for(int i=0;i<Window + 1;i++)
                    CepstraBuffer[i]=(DoubleData)cepstrum;
                _bufferPosition = Window + 1;
                _bufferPosition %= CepstraBufferSize;
                CurrentPosition = Window;
                CurrentPosition %= CepstraBufferSize;
                int numberFeatures = 1;
                _pendingSignal = null;
                for (int i = 0; i < Window; i++) {
                    IData next = GetNextData();
                    if (next != null) {
                        if (next is DoubleData) {
                            // just a cepstra
                            AddCepstrum((DoubleData) next);
                        } else if (next is DataEndSignal || next is SpeechEndSignal) {
                            // end of segment cepstrum
                            _pendingSignal = (Signal) next;
                            ReplicateLastCepstrum();
                            numberFeatures += i;
                            break;
                        } else if (next is DataStartSignal) {
                            throw new Exception("Too many UTTERANCE_START");
                        }
                    }
                }
                return numberFeatures;
            }
        }

        /// <summary>
        /// Adds the given DoubleData object to the cepstraBuffer.
        /// </summary>
        /// <param name="cepstrum">The DoubleData object to add.</param>
        private void AddCepstrum(DoubleData cepstrum) 
        {
            CepstraBuffer[_bufferPosition++] = cepstrum;
            _bufferPosition %= CepstraBufferSize;
        }

        /// <summary>
        /// Replicate the last frame into the last window number of frames in the cepstraBuffer.
        /// </summary>
        /// <returns>The number of replicated Cepstrum.</returns>
        /// <exception cref="System.Exception">BufferPosition</exception>
        private int ReplicateLastCepstrum() {
            DoubleData last;
            if (_bufferPosition > 0) {
                last = CepstraBuffer[_bufferPosition - 1];
            } else if (_bufferPosition == 0) {
                last = CepstraBuffer[CepstraBuffer.Length - 1];
            } else {
                throw new Exception("BufferPosition < 0");
            }
            for (int i = 0; i < Window; i++) {
                AddCepstrum(last);
            }
            return Window;
        }

        /// <summary>
        /// Converts the Cepstrum data in the cepstraBuffer into a FeatureFrame.
        /// </summary>
        /// <param name="totalFeatures">The number of Features that will be produced.</param>
        private void ComputeFeatures(int totalFeatures) {
            if (totalFeatures == 1) {
                ComputeFeature();
            } else {
                // create the Features
                for (int i = 0; i < totalFeatures; i++) {
                    ComputeFeature();
                }
            }
        }

        /// <summary>
        /// Computes the next Feature.
        /// </summary>
        private void ComputeFeature() {
            IData feature = ComputeNextFeature();
            _outputQueue.Add(feature);
        }

        /// <summary>
        /// Computes the next feature. Advances the pointers as well.
        /// </summary>
        /// <returns>The feature Data computed.</returns>
        public abstract IData ComputeNextFeature();
    }
}
