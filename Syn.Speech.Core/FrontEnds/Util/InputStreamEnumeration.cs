using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syn.Speech.Helper;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.FrontEnds.Util
{
    public class InputStreamEnumeration : IEnumerable<Stream>
    {

        private readonly int _totalFiles;
        private bool _inSilence;
        private readonly Random _silenceRandom;
        private readonly StreamReader _reader;


        private readonly ConcatFileDataSource _owner;


        public InputStreamEnumeration(ConcatFileDataSource owner, string batchFile, int startFile,
                               int totalFiles)
        {
            _owner = owner;


            _totalFiles = totalFiles;
            _reader = new StreamReader(batchFile);
            if (owner.SilenceFileName != null)
            {
                _inSilence = true;
                _silenceRandom = new Random((int)Java.CurrentTimeMillis());

                _owner.SilenceCount = GetSilenceCount();
            }
            // go to the start file
            for (var i = 1; i < startFile; i++)
            {
                _reader.ReadLine();
            }
        }


        /// <summary>
        /// Tests if this enumeration contains more elements.
        /// </summary>
        /// <returns>true if and only if this enumeration object contains at least one more element to provide; false otherwise.</returns>
        public bool HasMoreElements()
        {
            if (_owner.NextFile == null)
            {
                _owner.NextFile = ReadNext();
            }
            return (_owner.NextFile != null);
        }

        /// <summary>
        /// Returns the next element of this enumeration if this enumeration object has at least one more element to provide.
        /// </summary>
        /// <returns>The next element of this enumeration.</returns>
        /// <exception cref="System.Exception">Cannot convert  + _owner.NextFile +
        ///                              to a FileInputStream</exception>
        public Stream NextElement()
        {
            Stream stream = null;
            if (_owner.NextFile == null)
            {
                _owner.NextFile = ReadNext();
            }
            if (_owner.NextFile != null)
            {
                try
                {
                    stream = new FileStream(_owner.NextFile, FileMode.Open);
                    // System.out.println(nextFile);
                    _owner.NextFile = null;
                }
                catch (IOException ioe)
                {
                    Trace.TraceError(ioe.Message);
                    throw new Exception("Cannot convert " + _owner.NextFile +
                            " to a FileInputStream");
                }
            }

            // close the transcript file no more files
            if (stream == null && _owner.Transcript != null)
            {
                try
                {
                    _owner.Transcript.Close();
                }
                catch (IOException ioe)
                {
                    Trace.TraceError(ioe.Message);
                }
            }
            return stream;
        }

        /// <summary>
        /// Returns the name of next audio file, taking into account file skipping and the adding of silence.
        /// </summary>
        /// <returns>The name of the appropriate audio file.</returns>
        public string ReadNext()
        {
            if (!_inSilence)
            {
                return ReadNextDataFile();
            }
            // return the silence file
            string next = null;
            if (_owner.SilenceCount > 0)
            {
                next = _owner.SilenceFileName;
                if (_owner.Transcript != null)
                {
                    WriteSilenceToTranscript();
                }
                _owner.SilenceCount--;
                if (_owner.SilenceCount <= 0)
                {
                    _inSilence = false;
                }
            }
            return next;
        }

        /// <summary>
        /// Returns the next audio file.
        /// </summary>
        /// <returns>The name of the next audio file.</returns>
        /// <exception cref="System.Exception">Problem reading from batch file</exception>
        private string ReadNextDataFile()
        {
            try
            {
                if (0 <= _totalFiles &&
                        _totalFiles <= _owner.References.Count)
                {
                    return null;
                }
                var next = _reader.ReadLine();
                if (next != null)
                {
                    var reference = BatchFile.GetReference(next);
                    _owner.References.Add(reference);
                    next = BatchFile.GetFilename(next);
                    for (var i = 1; i < _owner.Skip; i++)
                    {
                        _reader.ReadLine();
                    }
                    if (_owner.SilenceFileName != null && _owner.MaxSilence > 0)
                    {
                        _owner.SilenceCount = GetSilenceCount();
                        _inSilence = true;
                    }
                    if (_owner.Transcript != null)
                    {
                        WriteTranscript(next, reference);
                    }
                }
                return next;
            }
            catch (IOException ioe)
            {
                Trace.TraceError(ioe.Message);
                throw new Exception("Problem reading from batch file");
            }
        }

        /// <summary>
        /// Writes the transcript file.
        /// </summary>
        /// <param name="fileName">The name of the decoded file.</param>
        /// <param name="reference">The reference text.</param>
        private void WriteTranscript(String fileName, string reference)
        {
            try
            {
                var file = new FileInfo(fileName);
                var start = _owner.GetSeconds(_owner.TotalBytes);
                _owner.TotalBytes += file.Length;
                var end = _owner.GetSeconds(_owner.TotalBytes);
                _owner.Transcript.Write(_owner.Context + " 1 " + fileName + ' ' + start +
                    ' ' + end + "  " + reference + '\n');
                _owner.Transcript.Flush();
            }
            catch (IOException ioe)
            {
                Trace.TraceError(ioe.Message);
            }
        }

        /// <summary>
        /// Writes silence to the transcript file.
        /// </summary>
        private void WriteSilenceToTranscript()
        {
            try
            {
                var start = _owner.GetSeconds(_owner.TotalBytes);
                _owner.TotalBytes += _owner.SilenceFileLength;
                var end = _owner.GetSeconds(_owner.TotalBytes);
                _owner.Transcript.Write(_owner.Context + " 1 " + ConcatFileDataSource.GapLabel + ' ' +
                        start + ' ' + end + " \n");
                _owner.Transcript.Flush();
            }
            catch (IOException ioe)
            {
                Trace.TraceError(ioe.Message);
            }
        }

        /// <summary>
        /// Gets how many times the silence file should be added between utterances.
        /// </summary>
        /// <returns>The number of times the silence file should be added between utterances.</returns>
        private int GetSilenceCount()
        {
            if (_owner.AddRandomSilence)
            {
                return _silenceRandom.Next(_owner.MaxSilence) + 1;
            }
            return _owner.MaxSilence;
        }

        public IEnumerator<Stream> GetEnumerator()
        {
            var enumerator = new List<Stream>();
            while (HasMoreElements())
            {
                enumerator.Add(NextElement());
            }
            return enumerator.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
