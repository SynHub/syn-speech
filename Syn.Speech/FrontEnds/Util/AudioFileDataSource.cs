using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PARTIAL-PATROLLED
using Syn.Speech.Wave;
//REFACTORED
namespace Syn.Speech.FrontEnds.Util
{

    /**
     * An AudioFileDataSource generates a stream of audio data from a given audio file. All required information concerning
     * the audio format are read directly from the file . One would need to call {@link #setAudioFile(java.io.File,String)}
     * to set the input file, and call {@link #getData} to obtain the Data frames.
     * <p/>
     * Using JavaSound as backend this class is able to handle all sound files supported by JavaSound. Beside the built-in
     * support for .wav, .au and .aiff. Using plugins (cf.  http://www.jsresources.org/ ) it can be extended to support
     * .ogg, .mp3, .speex and others.
     *
     * @author Holger Brandl
     */
    public class AudioFileDataSource : BaseDataProcessor
    {

        /// <summary>
        /// The property for the number of bytes to read from the InputStream each time.
        /// </summary>
        [S4Integer(DefaultValue = 3200)]
        public static string PropBytesPerRead = "bytesPerRead";

        [S4ComponentList(Type = typeof(IConfigurable))]
        public static string AudioFileListeners = "audioFileListners";
        protected List<IAudioFileProcessListener> FileListeners = new List<IAudioFileProcessListener>();

        protected Stream DataStream;
        protected int BytesPerRead;
        protected int BytesPerValue;
        private long _totalValuesRead;
        protected bool SignedData;
        private bool _streamEndReached;
        private bool _utteranceEndSent;
        private bool _utteranceStarted;

        private FileInfo _curAudioFile;

        public AudioFileDataSource(int bytesPerRead, List<IAudioFileProcessListener> listeners)
        {

            Create(bytesPerRead, listeners);
        }

        public AudioFileDataSource()
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            //logger = ps.getLogger();
            Create(ps.GetInt(PropBytesPerRead), ps.GetComponentList<IAudioFileProcessListener>(AudioFileListeners));
        }

        private void Create(int bytesPerRead, IList<IAudioFileProcessListener> listeners)
        {
            BytesPerRead = bytesPerRead;

            if (listeners != null)
            {
                // attach all pool-listeners
                foreach (var configurable in listeners)
                {
                    AddNewFileListener(configurable);
                }
            }

            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            // reset all stream tags
            _streamEndReached = false;
            _utteranceEndSent = false;
            _utteranceStarted = false;

            if (BytesPerRead % 2 == 1)
            {
                BytesPerRead++;
            }
        }

        /**
         * Sets the audio file from which the data-stream will be generated of.
         *
         * @param audioFile  The location of the audio file to use
         * @param streamName The name of the InputStream. if <code>null</code> the complete path of the audio file will be
         *                   uses as stream name.
         */
        public void SetAudioFile(string audioFile, string streamName)
        {
            try
            {
                SetAudioFile(new URL(audioFile), streamName);
            }
            catch (FileLoadException e)
            {
                Trace.TraceError(e.Message);
            }
        }

        /**
         * Sets the audio file from which the data-stream will be generated of.
         *
         * @param audioFileURL The location of the audio file to use
         * @param streamName   The name of the InputStream. if <code>null</code> the complete path of the audio file will be
         *                     uses as stream name.
         */
        public virtual void SetAudioFile(URL audioFileUrl, string streamName)
        {
            // first close the last stream if there's such a one
            if (DataStream != null)
            {
                try
                {
                    DataStream.Close();
                }
                catch (IOException e)
                {
                    Trace.TraceError(e.Message);
                }

                DataStream = null;
            }

            Debug.Assert(audioFileUrl != null);
            if (streamName != null)
                streamName = audioFileUrl.File.FullName;

            WaveFile audioStream = null;
            try
            {
                audioStream = new WaveFile(audioFileUrl.Path);
            }
            catch (Exception e)
            {
                this.LogInfo("Audio file format not supported: " + e);
                Trace.TraceError(e.Message);
            }

            _curAudioFile = new FileInfo(audioFileUrl.Path);
            foreach (var fileListener in FileListeners)
                fileListener.AudioFileProcStarted(_curAudioFile);

            SetInputStream(audioStream, streamName);
        }

        /**
         * Sets the InputStream from which this StreamDataSource reads.
         *
         * @param inputStream the InputStream from which audio data comes
         * @param streamName  the name of the InputStream
         */
        public void SetInputStream(WaveFile inputStream, string streamName)
        {
            DataStream = inputStream.Stream;
            _streamEndReached = false;
            _utteranceEndSent = false;
            _utteranceStarted = false;

            var format = inputStream.Format;
            SampleRate = format.SampleRate;
            //TODO: CHECK SEMANTICS - WAV files are always little-endian
            IsBigEndian = false;

            var s = format.ToString();
            this.LogInfo("input format is " + s);

            if (format.BitsPerSample % 8 != 0)
                throw new Exception("StreamDataSource: bits per sample must be a multiple of 8.");
            BytesPerValue = format.BitsPerSample / 8;

            // test whether all files in the stream have the same format

           // var encoding = format.Encoding;
            //WaveFormatEncoding.pcm
            SignedData = true; //TODO: CHECK SEMANTICS
            //if (encoding.Equals(AudioFormat.Encoding.PCM_SIGNED))
            //    signedData = true;
            //else if (encoding.Equals(AudioFormat.Encoding.PCM_UNSIGNED))
            //    signedData = false;
            //else
            //    throw new RuntimeException("used file encoding is not supported");

            _totalValuesRead = 0;
        }

        /**
         * Reads and returns the next Data from the InputStream of StreamDataSource, return null if no data is read and end
         * of file is reached.
         *
         * @return the next Data or <code>null</code> if none is available
         * @throws edu.cmu.sphinx.frontend.DataProcessingException
         *          if there is a data processing error
         */

        public override IData GetData()
        {
            IData output = null;
            if (_streamEndReached)
            {
                if (!_utteranceEndSent)
                {
                    // since 'firstSampleNumber' starts at 0, the last
                    // sample number should be 'totalValuesRead - 1'
                    output = CreateDataEndSignal();
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
                    if (DataStream != null)
                    {
                        output = ReadNextFrame();
                        if (output == null)
                        {
                            if (!_utteranceEndSent)
                            {
                                output = CreateDataEndSignal();
                                _utteranceEndSent = true;
                            }
                        }
                    }
                }
            }
            return output;
        }

        private DataEndSignal CreateDataEndSignal()
        {
            if (!(this is ConcatAudioFileDataSource))
                foreach (var fileListener in FileListeners)
                    fileListener.AudioFileProcFinished(_curAudioFile);

            return new DataEndSignal(Duration);
        }

        /**
         * Returns the next Data from the input stream, or null if there is none available
         *
         * @return a Data or null
         * @throws edu.cmu.sphinx.frontend.DataProcessingException
         */
        private IData ReadNextFrame()
        {
            // read one frame's worth of bytes
            int read;
            var totalRead = 0;
            var bytesToRead = BytesPerRead;
            var samplesBuffer = new byte[BytesPerRead];
            var firstSample = _totalValuesRead;
            try
            {
                do
                {
                    read = DataStream.Read(samplesBuffer, totalRead, bytesToRead
                            - totalRead);
                    if (read > 0)
                    {
                        totalRead += read;
                    }
                } while (read != 0 && totalRead < bytesToRead);
                if (totalRead <= 0)
                {
                    CloseDataStream();
                    return null;
                }
                // shrink incomplete frames
                _totalValuesRead += (totalRead / BytesPerValue);
                if (totalRead < bytesToRead)
                {
                    totalRead = (totalRead % 2 == 0)
                            ? totalRead + 2
                            : totalRead + 3;
                    var shrinkedBuffer = new byte[totalRead];
                    Array.Copy(samplesBuffer, 0, shrinkedBuffer, 0,
                                    totalRead);
                    samplesBuffer = shrinkedBuffer;
                    CloseDataStream();
                }
            }
            catch (IOException ioe)
            {
                throw new DataProcessingException("Error reading data", ioe);
            }
            // turn it into an Data object
            double[] doubleData;
            if (IsBigEndian)
            {
                doubleData = DataUtil.BytesToValues(samplesBuffer, 0, totalRead, BytesPerValue, SignedData);
            }
            else
            {
                doubleData = DataUtil.LittleEndianBytesToValues(samplesBuffer, 0, totalRead, BytesPerValue, SignedData);
            }

            return new DoubleData(doubleData, SampleRate, firstSample);
        }

        private void CloseDataStream()
        {
            _streamEndReached = true;
            if (DataStream != null)
            {
                DataStream.Close();
            }
        }

        /// <summary>
        /// Returns the duration of the current data stream in milliseconds.
        /// </summary>
        /// <value>
        /// The duration of the current data stream in milliseconds.
        /// </value>
        private long Duration
        {
            get { return (long) ((_totalValuesRead/(double) SampleRate)*1000.0); }
        }

        public int SampleRate { get; protected set; }

        public bool IsBigEndian { get; protected set; }

        /// <summary>
        /// Adds a new listener for new file events.
        /// </summary>
        public void AddNewFileListener(IAudioFileProcessListener l)
        {
            if (l == null)
                return;

            FileListeners.Add(l);
        }

        /// <summary>
        /// Removes a listener for new file events.
        /// </summary>
        public void RemoveNewFileListener(IAudioFileProcessListener l)
        {
            if (l == null)
                return;

            FileListeners.Remove(l);
        }
    }
}
