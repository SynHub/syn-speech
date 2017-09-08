using System;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    ///<summary>
    /// Drops certain feature frames, usually to speed up decoding. For example, if you 'dropEveryNthFrame' is set to 2, it
    /// will drop every other feature frame. If you set 'replaceNthWithPrevious' to 3, then you replace with 3rd frame with
    /// the 2nd frame, the 6th frame with the 5th frame, etc..
    /// </summary>
    public class FrameDropper : BaseDataProcessor
    {

        /// <summary>
        /// The property that specifies dropping one in every Nth frame. If N=2, we drop every other frame. If N=3, we drop every third frame, etc..
        /// </summary>
        [S4Integer(DefaultValue = -1)]
        public static string PropDropEveryNthFrame = "dropEveryNthFrame";

        /// <summary>
        /// The property that specifies whether to replace the Nth frame with the previous frame.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropReplaceNthWithPrevious = "replaceNthWithPrevious";

        private IData _lastFeature;
        private bool _replaceNthWithPrevious;
        private int _dropEveryNthFrame;
        private int _id; //first frame has ID "0", second "1", etc.

        public FrameDropper(int dropEveryNthFrame, bool replaceNthWithPrevious)
        {
            //initLogger();
            InitVars(dropEveryNthFrame, replaceNthWithPrevious);
        }

        public FrameDropper()
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            InitVars(ps.GetInt(PropDropEveryNthFrame), ps.GetBoolean(PropReplaceNthWithPrevious));
        }

        protected void InitVars(int dropEveryNthFrame, bool replaceNthWithPrevious)
        {
            _dropEveryNthFrame = dropEveryNthFrame;
            if (dropEveryNthFrame <= 1)
            {
                throw new ArgumentException(PropDropEveryNthFrame +
                        "must be greater than one");
            }

            _replaceNthWithPrevious = replaceNthWithPrevious;
        }

        /// <summary>
        /// Initializes this FrameDropper.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _id = -1;
        }

        /// <summary>
        /// Returns the next Data object from this FrameDropper. The Data objects belonging to a single Utterance should be preceded by a DataStartSignal and ended by a DataEndSignal.
        /// </summary>
        /// <returns>
        /// The next available Data object, returns null if no Data object is available.
        /// </returns>
        public override IData GetData()
        {
            IData feature = ReadData();
            if (feature != null)
            {
                if (!(feature is Signal))
                {
                    if ((_id % _dropEveryNthFrame) == (_dropEveryNthFrame - 1))
                    {
                        // should drop the feature
                        if (_replaceNthWithPrevious)
                        {
                            // replace the feature
                            if (feature is FloatData)
                            {
                                FloatData floatLastFeature = (FloatData)
                                        _lastFeature;
                                feature = new FloatData
                                        (floatLastFeature.Values,
                                                floatLastFeature.SampleRate,
                                                floatLastFeature.FirstSampleNumber);
                            }
                            else
                            {
                                DoubleData doubleLastFeature = (DoubleData)
                                        _lastFeature;
                                feature = new DoubleData
                                        (doubleLastFeature.Values,
                                                doubleLastFeature.SampleRate,
                                                doubleLastFeature.FirstSampleNumber);
                            }
                        }
                        else
                        {
                            // read the next feature
                            feature = ReadData();
                        }
                    }
                }
                if (feature != null)
                {
                    if (feature is DataEndSignal)
                    {
                        _id = -1;
                    }
                    if (feature is FloatData)
                    {
                        _lastFeature = feature;
                    }
                    else
                    {
                        _lastFeature = null;
                    }
                }
                else
                {
                    _lastFeature = null;
                }
            }

            return feature;
        }

        /// <summary>
        /// Read a Data object from the predecessor DataProcessor, and increment the ID count appropriately.
        /// </summary>
        /// <returns>The read Data object.</returns>
        private IData ReadData()
        {
            IData frame = Predecessor.GetData();
            if (frame != null)
            {
                _id++;
            }
            return frame;
        }
    }
}
