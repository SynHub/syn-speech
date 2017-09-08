using System;
using System.IO;
using Syn.Logging;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Util
{
    /// <summary>
    /// A StreamDataSource converts data from an InputStream into Data objects. One
    /// would call {@link #setInputStream(InputStream,String) setInputStream} to set
    /// the input stream, and call {@link #getData} to obtain the Data object. The
    /// InputStream can be an arbitrary stream, for example a data from the network
    /// or from a pipe.
    ///
    /// StreamDataSource is not aware about incoming data format and assumes
    /// that incoming data matches StreamDataSource configuration. By default it's configured
    /// to read 16 kHz little-endian 16-bit signed raw data. If data has wrong format
    /// the result of the recognition is undefined. Also note that the sample rate of the
    /// data must match the sample required by the the acoustic model. If your
    /// model decodes 16 kHz files you can't recognize 8kHz data using it.
    ///
    /// You can use AudioFileDataSource instead to read the file headers and
    /// to convert incoming data to the required format automatically.
    /// </summary>
    public class StreamDataSource : BaseDataProcessor
    {
        /// <summary>
        /// The property for the sample rate. 
        /// </summary>
        [S4Integer(DefaultValue = 16000)]
        public static string PropSampleRate = "sampleRate";

        /// <summary>
        /// The property for the number of bytes to read from the InputStream each
        /// time.
        /// </summary>
        [S4Integer(DefaultValue = 3200)]
        public static string PropBytesPerRead = "bytesPerRead";

        /// <summary>
        /// The property for the number of bits per value. 
        /// </summary>
        [S4Integer(DefaultValue = 16)]
        public static string PropBitsPerSample = "bitsPerSample";

        /// <summary>
        /// The property specifying whether the input data is big-endian. 
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropBigEndianData = "bigEndianData";

        /// <summary>
        /// The property specifying whether the input data is signed. 
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropSignedData = "signedData";

        private Stream _dataStream;
        protected int SampleRate;
        private int _bytesPerRead;
        private int _bytesPerValue;
        private long _totalValuesRead;
        private Boolean _bigEndian;
        private Boolean _signedData;
        private Boolean _streamEndReached;
        private Boolean _utteranceEndSent;
        private Boolean _utteranceStarted;
        protected int BitsPerSample;

        private TimeFrame _timeFrame = TimeFrame.Infinite;

        public StreamDataSource(int sampleRate, int bytesPerRead,
                int bitsPerSample, Boolean bigEndian, Boolean signedData) 
        {
            init(sampleRate, bytesPerRead, bitsPerSample, bigEndian, signedData);
        }

        public StreamDataSource() {

        }

        /// <summary>
        /// @see Sphincs.util.props.Configurable#newProperties(Sphincs.util.props.PropertySheet)
        /// </summary>
        /// <param name="ps"></param>
        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            init(
                 ps.GetInt(PropSampleRate),
                 ps.GetInt(PropBytesPerRead),
                 ps.GetInt(PropBitsPerSample),
                 ps.GetBoolean(PropBigEndianData),
                 ps.GetBoolean(PropSignedData));
        }

        private void init(int sampleRate,
                          int bytesPerRead,
                          int bitsPerSample,
                          Boolean bigEndian,
                          Boolean signedData) 
        {
            SampleRate = sampleRate;
            _bytesPerRead = bytesPerRead;
            BitsPerSample = bitsPerSample;

            if (BitsPerSample % 8 != 0)
                throw new ArgumentOutOfRangeException("bits per sample must be a multiple of 8");

            _bytesPerValue = bitsPerSample / 8;
            _bigEndian = bigEndian;
            _signedData = signedData;
            _bytesPerRead += bytesPerRead % 2;
        }

        /// <summary>
        /// Sphincs.FrontEnd.DataProcessor#initialize(sphincs.FrontEnd.CommonConfig)
        /// </summary>
        public override void Initialize() 
        {
            base.Initialize();
        }

        public void SetInputStream(Stream inputStream) 
        {
            SetInputStream(inputStream, TimeFrame.Infinite);
        }


        /// <summary>
        /// Sets the InputStream from which this StreamDataSource reads.
        /// </summary>
        /// <param name="inputStream">The InputStream from which audio data comes.</param>
        /// <param name="timeFrame">The time frame.</param>
        public void SetInputStream(Stream inputStream, TimeFrame timeFrame) 
        {
            _dataStream = inputStream;
            _timeFrame = timeFrame;
            _streamEndReached = false;
            _utteranceEndSent = false;
            _utteranceStarted = false;
            _totalValuesRead = 0;
        }

        /// <summary>
        /// Reads and returns the next Data from the InputStream of
        /// StreamDataSource, return null if no data is read and end of file is
        /// reached.
        /// </summary>
        /// <returns>the next Data or <code>null</code> if none is available</returns>
        public override IData GetData()
        {
            IData output = null;
            if (_streamEndReached) 
            {
                if (!_utteranceEndSent) 
                {
                    // since 'firstSampleNumber' starts at 0, the last
                    // sample number should be 'totalValuesRead - 1'
                    output = new DataEndSignal(Duration);
                    _utteranceEndSent = true;
                }
            } 
            else 
            {
                if (!_utteranceStarted) 
                {
                    _utteranceStarted = true;
                    output = new DataStartSignal(SampleRate);
                } 
                else 
                {
                    if (_dataStream != null) 
                    {
                        do 
                        {
                            output = ReadNextFrame();
                        } 
                        while (output != null && Duration < _timeFrame.Start);

                        if ((output == null || Duration > _timeFrame.End)
                                && !_utteranceEndSent) 
                        {
                            output = new DataEndSignal(Duration);
                            _utteranceEndSent = true;
                            _streamEndReached = true;
                        }
                    } 
                    else 
                    {
                        this.LogInfo("Input stream is not set");
                        if (!_utteranceEndSent) 
                        {
                            output = new DataEndSignal(Duration);
                            _utteranceEndSent = true;
                        }
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Returns the next Data from the input stream, or null if there is none
        /// available
        /// </summary>
        /// <returns>a Data or null</returns>
        private DoubleData ReadNextFrame()
        {
            // read one frame's worth of bytes
            var read=0;
            var totalRead = 0;
            var bytesToRead = _bytesPerRead;
            var samplesBuffer = new byte[_bytesPerRead];
            var firstSample = _totalValuesRead;
            try {
                do 
                {
                    
                    read = _dataStream.Read(samplesBuffer, totalRead, bytesToRead - totalRead);
                    if (read > 0) 
                    {
                        totalRead += read;
                    }
                } 
                while (read != 0 && totalRead < bytesToRead);
                if (totalRead <= 0) 
                {
                    CloseDataStream();
                    return null;
                }
                // shrink incomplete frames
                _totalValuesRead += (totalRead / _bytesPerValue);
                if (totalRead < bytesToRead) 
                {
                    totalRead = (totalRead % 2 == 0)
                            ? totalRead + 2
                            : totalRead + 3;
                    var shrinkedBuffer = new byte[totalRead];
                    Array.Copy(samplesBuffer, 0, shrinkedBuffer, 0, totalRead);
                    samplesBuffer = shrinkedBuffer;
                    CloseDataStream();
                }
            } 
            catch (IOException ioe) 
            {
                throw new Exception("Error reading data", ioe);
            }
            // turn it into an Data object
            double[] doubleData;
            if (_bigEndian) 
            {
                doubleData = DataUtil.BytesToValues(samplesBuffer, 0, totalRead,
                                                    _bytesPerValue, _signedData);
            } 
            else 
            {
                doubleData = DataUtil.LittleEndianBytesToValues(samplesBuffer,
                                                                0,
                                                                totalRead,
                                                                _bytesPerValue,
                                                                _signedData);
            }
            return new DoubleData(doubleData, SampleRate, firstSample);
        }

        private void CloseDataStream()
        {
            _streamEndReached = true;
            if (_dataStream != null) 
            {
                _dataStream.Close();
            }
        }

        /// <summary>
        /// Returns the duration of the current data stream in milliseconds.
        /// </summary>
        /// <value>the duration of the current data stream in milliseconds</value>
        private long Duration
        {
            get { return (long) ((_totalValuesRead/(double) SampleRate)*1000.0); }
        }
    }
}
