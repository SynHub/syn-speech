using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Util;
//PATROLLED
using Syn.Speech.Wave;
//REFACTORED
namespace Syn.Speech.FrontEnds.Util
{
    /// <summary>
    /// Concatenates a list of audio files as one continuous audio stream.
    /// @author Holger Brandl
    /// </summary>
    public class ConcatAudioFileDataSource : AudioFileDataSource, IReferenceSource
    {
        private FileInfo _nextFile;
        private bool _isInitialized;

        List<FileInfo> _batchFiles;

        public ConcatAudioFileDataSource(int bytesPerRead, List<IAudioFileProcessListener> listeners) : base(bytesPerRead, listeners)
        {

        }

        public ConcatAudioFileDataSource()
        {

        }

        /// <summary>
        /// Initializes a ConcatFileDataSource.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (_batchFiles == null)
                return;

            try
            {
                References = new List<String>();
                DataStream = new SequenceInputStream(new InputStreamEnumeration(this, _batchFiles));
            }
            catch (IOException e)
            {
                Trace.TraceError(e.Message);
            }
        }

        public void SetBatchFile(FileInfo file)
        {
            SetBatchUrls(ReadDriver(file.FullName));
        }

        public void SetBatchFiles(List<FileInfo> files)
        {
            var urls = new List<FileInfo>();

            try
            {
                foreach (var file in files)
                    urls.Add(file);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }

            SetBatchUrls(urls);
        }

        public void SetBatchUrls(List<FileInfo> urls)
        {
            _batchFiles = new List<FileInfo>(urls);
            Initialize();
        }

        /// <summary>
        /// Reads and verifies a driver file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private static List<FileInfo> ReadDriver(String fileName)
        {
            var inputFile = new FileInfo(fileName);
            List<FileInfo> driverFiles = null;

            try
            {
                var bf = new StreamReader(inputFile.FullName);
                driverFiles = new List<FileInfo>();

                string line;
                while ((line = bf.ReadLine()) != null && line.Trim().Length != 0)
                {
                    var file = new FileInfo(line);
                    driverFiles.Add(file);
                }

                bf.Close();
            }
            catch (IOException e)
            {
                Trace.TraceError(e.Message);
            }

            Debug.Assert(driverFiles != null);
            return driverFiles;
        }

        public override void SetAudioFile(URL audioFileUrl, string streamName)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets a list of all reference text. Implements the getReferences() method of ReferenceSource.
        /// </summary>
        ///
        public IList<string> References { get; private set; }

        /// <summary>
        /// The work of the concatenating of the audio files are done here. The idea
        /// here is to turn the list of audio files into an Enumeration, and then
        /// fed it to a SequenceInputStream, giving the illusion that the audio
        /// files are concatenated, but only logically.
        /// </summary>
       public class InputStreamEnumeration : IEnumerable<Stream>
        {
            private FileInfo _lastFile;
            readonly IEnumerator<FileInfo> _fileIt;

            private readonly ConcatAudioFileDataSource _parent;

            internal InputStreamEnumeration(ConcatAudioFileDataSource parent, List<FileInfo> files)
            {
                _parent = parent;

                _fileIt = new List<FileInfo>(files).GetEnumerator();
            }

            /// <summary>
            /// Tests if this enumeration contains more elements.
            /// </summary>
            /// <returns>True if and only if this enumeration object contains at least one more element to provide; false otherwise.</returns>
            public bool HasMoreElements()
            {
                if (_parent._nextFile == null)
                {
                    _parent._nextFile = ReadNext();
                }
                return (_parent._nextFile != null);
            }

            /// <summary>
            /// Returns the next element of this enumeration if this enumeration object has at least one more element to provide.
            /// </summary>
            /// <returns>The next element of this enumeration.</returns>
            /// <exception cref="RuntimeException">format mismatch for subsequent files</exception>
            /// <exception cref="System.Exception">Cannot convert  + _parent._nextFile
            ///                                 +  to a FileInputStream</exception>
            public WaveFile NextElement()
            {
                WaveFile stream = null;
                if (_lastFile == null)
                {
                    _parent._nextFile = ReadNext();
                }

                if (_parent._nextFile != null)
                {
                    try
                    {
                        var ais = new WaveFile(_parent._nextFile.FullName);

                        // test whether all files in the stream have the same
                        // format
                        var format = ais.Format;
                        if (!_parent._isInitialized)
                        {
                            _parent._isInitialized = true;
                            //TODO: CHECK SEMANTICS
                            _parent.IsBigEndian = false;
                            _parent.SampleRate = format.SampleRate;
                            _parent.SignedData = true; //format.getEncoding().equals(AudioFormat.Encoding.PCM_SIGNED);
                            _parent.BytesPerValue = format.BitsPerSample / 8;
                        }

                        if (format.SampleRate != _parent.SampleRate|| format.Channels != 1)
                        //|| format.isBigEndian() != Parent.bigEndian)
                        {
                            throw new RuntimeException("format mismatch for subsequent files");
                        }

                        stream = ais;
                        this.LogInfo("Strating processing of '" + _lastFile.FullName + '\'');
                        foreach (var fl in _parent.FileListeners)
                            fl.AudioFileProcStarted(new FileInfo(_parent._nextFile.FullName));

                        _lastFile = _parent._nextFile;
                        _parent._nextFile = null;
                    }
                    catch (IOException ioe)
                    {
                        Trace.TraceError(ioe.Message);
                        throw new Exception("Cannot convert " + _parent._nextFile
                                + " to a FileInputStream");
                    }
                    catch (Exception e)
                    {
                        this.LogInfo("UnsupportedAudioFileException: " + e.Message);
                    }
                }

                return stream;
            }

            /// <summary>
            ///  Returns the name of next audio file
            /// </summary>
            /// <returns>The name of the appropriate audio file.</returns>
            public FileInfo ReadNext()
            {
                if (_lastFile != null)
                {
                    this.LogInfo("Finished processing of '" + _lastFile.FullName
                            + '\'');
                    foreach (var fl in _parent.FileListeners)
                        fl.AudioFileProcFinished(new FileInfo(_lastFile.FullName));

                    _lastFile = null;
                }

                if (_fileIt.MoveNext())
                    _lastFile = _fileIt.Current;

                return _lastFile;
            }

            public IEnumerator<Stream> GetEnumerator()
            {
                //TODO: CHECK SEMANTICS
                var enumerator = new List<Stream>();
                while (HasMoreElements())
                {
                    enumerator.Add(NextElement().Stream);
                }
                return enumerator.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
