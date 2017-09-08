using System;
using System.IO;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.FrontEnds.Util
{
    /// <summary>
    /// Produces Mel-cepstrum data from an InputStream. To set the inputstream with cepstral data, use the {@link
    /// #setInputStream(InputStream,Boolean) setInputStream} method, and then call {@link #getData} to obtain the Data
    /// objects that have cepstra data in it.
    /// </summary>
    public class StreamCepstrumSource: BaseDataProcessor
    {
        /// <summary>
        /// The property specifying whether the input is in binary.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropBinary = "binary";

        /// <summary>
        ///  The property  name for frame size in milliseconds.
        /// </summary>
        [S4Double(DefaultValue = 25.625)]
        public static string PropFrameSizeMs = "frameSizeInMs";

        /// <summary>
        /// The property  name for frame shift in milliseconds, which has a default value of 10F.
        /// </summary>
        [S4Double(DefaultValue = 10.0)]
        public static string PropFrameShiftMs = "frameShiftInMs";

        /// <summary>
        /// The property  specifying the length of the cepstrum data.
        /// </summary>
        [S4Integer(DefaultValue = 13)]
        public static string PropCepstrumLength = "cepstrumLength";

        /// <summary>
        /// The property specifying whether the input data is big-endian.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropBigEndianData = "bigEndianData";

        /// <summary>
        /// The property that defines the sample rate.
        /// </summary>
        [S4Integer(DefaultValue = 16000)]
        public static string PropSampleRate = "sampleRate";

        private Boolean _binary;
        private ExtendedStreamTokenizer _est; // for ASCII files
        private BufferedStream _binaryStream; // for binary files
        private int _numPoints;
        private int _curPoint;
        private int _cepstrumLength;
        private int _frameShift;
        private int _frameSize;
        private int _sampleRate;
        private long _firstSampleNumber;
        private Boolean _bigEndian;

        public StreamCepstrumSource( int cepstrumLength, Boolean binary, float frameShiftMs, float frameSizeMs, int sampleRate ) 
        {
            _cepstrumLength = cepstrumLength;
            _binary = binary;
            _sampleRate = sampleRate;
            _frameShift = DataUtil.GetSamplesPerWindow(sampleRate, frameShiftMs);
            _frameSize = DataUtil.GetSamplesPerShift(sampleRate, frameSizeMs);
        }

        public StreamCepstrumSource( ) 
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _cepstrumLength = ps.GetInt(PropCepstrumLength);
            _binary = ps.GetBoolean(PropBinary);
            _bigEndian = ps.GetBoolean(PropBigEndianData);
            var frameShiftMs = ps.GetFloat(PropFrameShiftMs);
            var frameSizeMs = ps.GetFloat(PropFrameSizeMs);
            _sampleRate = ps.GetInt(PropSampleRate);
            _frameShift = DataUtil.GetSamplesPerWindow(_sampleRate, frameShiftMs);
            _frameSize = DataUtil.GetSamplesPerShift(_sampleRate, frameSizeMs);        
        }

        /// <summary>
        /// Constructs a StreamCepstrumSource that reads MelCepstrum data from the given path.
        /// </summary>
        public override void Initialize() 
        {
            base.Initialize();
            _curPoint = -1;
            _firstSampleNumber = 0;
            _bigEndian = false;
        }


        /// <summary>
        /// Sets the InputStream to read cepstral data from.
        /// </summary>
        /// <param name="_is">The InputStream to read cepstral data from.</param>
        /// <param name="bigEndian">true if the InputStream data is in big-endian, false otherwise.</param>
        public void SetInputStream(Stream _is, Boolean bigEndian)
        {
            _bigEndian = bigEndian;
            if (_binary)
            {
                _binaryStream = new BufferedStream(_is, 8192);
                if (_bigEndian)
                {
                    _numPoints = _binaryStream.ReadInt();
                    this.LogInfo("BigEndian");
                } 
                else {
                    _numPoints = Utilities.ReadLittleEndianInt(_binaryStream);
                    this.LogInfo("LittleEndian");
                }
                this.LogInfo("Frames: " + _numPoints / _cepstrumLength);
            } else {
                _est = new ExtendedStreamTokenizer(_is, false);
                _numPoints = _est.GetInt("num_frames");
                _est.ExpectString("frames");
            }
            _curPoint = -1;
            _firstSampleNumber = 0;
        }


        /// <summary>
        /// Returns the next Data object, which is the mel cepstrum of the input frame. However, it can also be other Data objects like DataStartSignal.
        /// </summary>
        /// <returns>
        /// The next available Data object, returns null if no Data object is available.
        /// </returns>
        /// <exception cref="System.Exception">
        /// IOException closing cepstrum stream
        /// or
        /// IOException reading from cepstrum stream
        /// </exception>
        public override IData GetData()
        {

            IData data;

            if (_curPoint == -1) {
                data = new DataStartSignal(_sampleRate);
                _curPoint++;
            } else if (_curPoint == _numPoints) {
                if (_numPoints > 0) {
                    _firstSampleNumber =
                            (_firstSampleNumber - _frameShift + _frameSize - 1);
                }
                // send a DataEndSignal
                var numberFrames = _curPoint / _cepstrumLength;
                var totalSamples = (numberFrames - 1)* _frameShift + _frameSize;
                var duration = (long)
                        ((totalSamples / (double) _sampleRate)* 1000.0);

                data = new DataEndSignal(duration);

                try {
                    if (_binary) {
                        _binaryStream.Close();
                    } else {
                        _est.Close();
                    }
                    _curPoint++;
                } catch (IOException ioe) {
                    throw new Exception("IOException closing cepstrum stream", ioe);
                }
            } else if (_curPoint > _numPoints) {
                data = null;
            } else {
                var vectorData = new double[_cepstrumLength];

                for (var i = 0; i < _cepstrumLength; i++) {
                    try {
                        if (_binary) {
                            if (_bigEndian)
                            {
                                vectorData[i] = _binaryStream.ReadFloat();
                            } else {
                                vectorData[i] = Utilities.ReadLittleEndianFloat(_binaryStream);
                            }
                        } else {
                            vectorData[i] = _est.GetFloat("cepstrum data");
                        }
                        _curPoint++;
                    } catch (IOException ioe) {
                        throw new Exception("IOException reading from cepstrum stream", ioe);
                    }
                }

                // System.out.println("Read: " + curPoint);
                data = new DoubleData
                        (vectorData, _sampleRate, _firstSampleNumber);
                _firstSampleNumber += _frameShift;
                // System.out.println(data);
            }
            return data;
        }
    }
}
