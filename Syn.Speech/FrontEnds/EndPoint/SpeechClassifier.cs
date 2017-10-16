using System;
using System.Diagnostics;
using Syn.Speech.Logging;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.EndPoint
{
    /// <summary>
    ////// Implements a level tracking endpointer invented by Bent Schmidt Nielsen.
    /// </summary>
    public class SpeechClassifier : AbstractVoiceActivityDetector
    {
        /// <summary>
        /// The property specifying the endpointing frame length in milliseconds.
        /// </summary>
        [S4Integer(DefaultValue = 10)]
        public static string PropFrameLengthMs = "frameLengthInMs";

        /// <summary>
        /// The property specifying the minimum signal level used to update the background signal level.
        /// </summary>
        [S4Double(DefaultValue = 0)]
        public static string PropMinSignal = "minSignal";

        ///<summary>
        /// The property specifying the threshold. If the current signal level is greater than the background level by
        /// this threshold, then the current signal is marked as speech. Therefore, a lower threshold will make the
        /// endpointer more sensitive, that is, mark more audio as speech. A higher threshold will make the endpointer less
        /// sensitive, that is, mark less audio as speech.
        ///</summary>
        [S4Double(DefaultValue = 10)]
        public static string PropThreshold = "threshold";

        /// <summary>
        /// The property specifying the adjustment.
        /// </summary>
        [S4Double(DefaultValue = 0.003)]
        public static string PropAdjustment = "adjustment";

        protected double AverageNumber = 1;
        protected double Adjustment;
        protected double Level;               // average signal level
        protected double Background;          // background signal level
        protected double MinSignal;           // minimum valid signal level
        protected double Threshold;
        protected float FrameLengthSec;
        private Boolean _isSpeech;

        // Statistics
        protected long SpeechFrames;
        protected long BackgroundFrames;
        protected double TotalBackgroundLevel;
        protected double TotalSpeechLevel;
    
        public SpeechClassifier(int frameLengthMs, double adjustment, double threshold, double minSignal ) 
        {
            FrameLengthSec = frameLengthMs / 1000.0f;

            Adjustment = adjustment;
            Threshold = threshold;
            MinSignal = minSignal;

            Initialize();
        }

        public SpeechClassifier() {
        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            int frameLengthMs = ps.GetInt(PropFrameLengthMs);
            FrameLengthSec = frameLengthMs / 1000.0f;

            Adjustment = ps.GetDouble(PropAdjustment);
            Threshold = ps.GetDouble(PropThreshold);
            MinSignal = ps.GetDouble(PropMinSignal);
 
            Initialize();
        }


        /// <summary>
        /// Initializes this LevelTracker endpointer and DataProcessor predecessor.
        /// </summary>
        public override sealed void Initialize() 
        {
            base.Initialize();
            Reset();
        }


        /// <summary>
        /// Resets this LevelTracker to a starting state.
        /// </summary>
        protected void Reset() 
        {
            Level = 0;
            Background = 300;
            ResetStats();
        }


        /// <summary>
        /// Returns the logarithm base 10 of the root mean square of the given samples.
        /// </summary>
        /// <param name="samples">The samples.</param>
        /// <returns>The calculated log root mean square in log 10.</returns>
        public static double LogRootMeanSquare(double[] samples)
        {
            Debug.Assert(samples.Length > 0);
            double sumOfSquares = 0.0f;
            foreach (double sample in samples) 
            {
                sumOfSquares += sample* sample;
            }
            double rootMeanSquare = Math.Sqrt(sumOfSquares / samples.Length);
            rootMeanSquare = Math.Max(rootMeanSquare, 1);
            return (LogMath.Log10((float) rootMeanSquare)* 20);
        }

        /// <summary>
        ///  Classifies the given audio frame as speech or not, and updates the endpointing parameters.
        /// </summary>
        /// <param name="audio">The audio frame.</param>
        public SpeechClassifiedData Classify(DoubleData audio) 
        {
            double current = LogRootMeanSquare(audio.Values);
            // System.out.println("current: " + current);
            _isSpeech = false;
            if (current >= MinSignal) 
            {
                Level = ((Level* AverageNumber) + current) / (AverageNumber + 1);
                if (current < Background) {
                    Background = current;
                } else {
                    Background += (current - Background)* Adjustment;
                }
                if (Level < Background) {
                    Level = Background;
                }
                _isSpeech = (Level - Background > Threshold);
            }

            var labeledAudio = new SpeechClassifiedData(audio, _isSpeech);


            string speech = "";
            if (labeledAudio.IsSpeech)
                speech = "*";

            
            this.LogDebug("Bkg: " + Background + ", level: " + Level + ", current: " + current + ' ' + speech);


            CollectStats (_isSpeech);
        
            return labeledAudio;
        }

        /// <summary>
        /// Reset statistics.
        /// </summary>
        private void ResetStats () 
        {
            BackgroundFrames = 1;
            SpeechFrames = 1;
            TotalSpeechLevel = 0;
            TotalBackgroundLevel = 0;
        }
    
        /// <summary>
        /// Collects the statistics to provide information about signal to noise ratio in channel.
        /// </summary>
        /// <param name="isSpeech">If the current frame is classified as speech.</param>
        private void CollectStats(Boolean isSpeech) 
        {
            if (isSpeech) {
                TotalSpeechLevel = TotalSpeechLevel + Level;
                SpeechFrames = SpeechFrames + 1;
            } else {
                TotalBackgroundLevel = TotalBackgroundLevel + Background;
                BackgroundFrames = BackgroundFrames + 1;
            }        
        }

        /// <summary>
        /// Returns the next Data object.
        /// </summary>
        /// <returns>
        /// The next Data object, or null if none available.
        /// </returns>
        public override IData GetData()
        {
            IData audio = Predecessor.GetData();

            if (audio is DataStartSignal)
                Reset();

            if (audio is DoubleData) 
            {
                DoubleData data = (DoubleData) audio;
                audio = Classify(data);
            }
            return audio;
        }
    

        /// <summary>
        /// Method that returns if current returned frame contains speech. 
        /// It could be used by noise filter for example to adjust noise 
        /// spectrum estimation.
        /// </summary>
        /// <value>
        /// If current frame is speech.
        /// </value>
        public override bool IsSpeech
        {
            get { return _isSpeech; }
        }

        /// <summary>
        ///  Retrieves accumulated signal to noise ratio in dbScale.
        /// </summary>
        /// <returns>Signal to noise ratio.</returns>
        public double GetSnr () 
        {
            double snr = (TotalBackgroundLevel / BackgroundFrames - TotalSpeechLevel / SpeechFrames);
            this.LogInfo("Background " + TotalBackgroundLevel / BackgroundFrames);
            this.LogInfo("Speech " + TotalSpeechLevel / SpeechFrames);
            this.LogInfo("SNR is " + snr);
            return snr;
        }
 

        /// <summary>
        /// Return the estimation if input data was noisy enough to break
        /// recognition. The audio is counted noisy if signal to noise ratio
        /// is less then -20dB.
        /// </summary>
        /// <returns>Estimation of data being noisy.</returns>
        public Boolean GetNoisy () 
        {
            return (GetSnr() > -20);
        }
    }
}
