using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// A <code>DataProcessor</code> which wraps incoming <code>DoubleData</code>-objects into equally size blocks of defined
    /// length.
    /// </summary>
    public class DataBlocker: BaseDataProcessor
    {
        /// <summary>
        /// The property for the block size of generated data-blocks in milliseconds.
        /// </summary>
        [S4Double(DefaultValue = 10)]
        public static string PropBlockSizeMs = "blockSizeMs";

        private int _blockSizeSamples = Integer.MAX_VALUE;

        private int _curFirstSamplePos;
        private int _sampleRate = -1;

        private readonly List<DoubleData> _inBuffer = new List<DoubleData>();

        private int _curInBufferSize;

        public DataBlocker() 
        {
        }

        public DataBlocker(double blockSizeMs) 
        {
            this.BlockSizeMs = blockSizeMs;
        }

        public override void NewProperties(PropertySheet propertySheet)
        {
            base.NewProperties(propertySheet);
            BlockSizeMs = propertySheet.GetDouble(PropBlockSizeMs);
        }

        public double BlockSizeMs { get; private set; }

        public override IData GetData() 
        {
            while (_curInBufferSize < _blockSizeSamples || _curInBufferSize == 0) 
            {
                IData data = Predecessor.GetData();

                if (data is DataStartSignal) {
                    _sampleRate = ((DataStartSignal) data).SampleRate;
                    _blockSizeSamples = (int) Math.Round(_sampleRate* BlockSizeMs / 1000);

                    _curInBufferSize = 0;
                    _curFirstSamplePos = 0;
                
                    _inBuffer.Clear();
                }

                if (!(data is DoubleData)) {
                    return data;
                }

                DoubleData dd = (DoubleData) data;

                _inBuffer.Add(dd);
                _curInBufferSize += dd.Values.Length;
            }

            // now we are ready to merge all data blocks into one
            double[] newSampleBlock = new double[_blockSizeSamples];

            int copiedSamples = 0;

            long firstSample = _inBuffer[0].FirstSampleNumber + _curFirstSamplePos;

            while (_inBuffer.Count!=0)
            {
                DoubleData dd = _inBuffer.Remove(0);
                double[] values = dd.Values;
                int copyLength = Math.Min(_blockSizeSamples - copiedSamples, values.Length - _curFirstSamplePos);

                Array.Copy(values, _curFirstSamplePos, newSampleBlock, copiedSamples, copyLength);

                // does the current data-object contains more samples than necessary? -> keep the rest for the next block
                if (copyLength < (values.Length - _curFirstSamplePos)) 
                {
                    Debug.Assert(_inBuffer.Count==0);

                    _curFirstSamplePos += copyLength;
                    //inBuffer.add(0, dd);
                    _inBuffer.Add(dd);
                    break;
                } else {
                    copiedSamples += copyLength;
                    _curFirstSamplePos = 0;
                }
            }

            _curInBufferSize = _inBuffer.Count==0 ? 0 : _inBuffer[0].Values.Length - _curFirstSamplePos;

    //        for (int i = 0; i < newSampleBlock.length; i++) {
    //            newSampleBlock[i] *= 10;
    //        }
            return new DoubleData(newSampleBlock, _sampleRate, firstSample);
        }

    }
}
