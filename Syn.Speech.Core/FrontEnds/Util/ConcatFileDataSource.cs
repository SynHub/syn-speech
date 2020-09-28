using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syn.Speech.Helper;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Util
{

    /**
     * Concatenates a list raw headerless audio files as one continuous audio stream. A {@link
     * edu.cmu.sphinx.frontend.DataStartSignal DataStartSignal} will be placed before the start of the first file, and a
     * {@link edu.cmu.sphinx.frontend.DataEndSignal DataEndSignal} after the last file. No DataStartSignal or DataEndSignal
     * will be placed between them. Optionally, silence can be added in-between the audio files by setting the property:
     * <pre>edu.cmu.sphinx.frontend.util.ConcatFileDataSource.silenceFile</pre>
     * to a audio file for silence. By default, no silence is added. Moreover, one can also specify how many files to skip
     * for every file read.
     * <p/>
     * You can also specify the name of a transcript file to write the transcription to. The transcription will be written
     * in HUB-4 style. A sample HUB-4 transcript looks like:
     * <pre>
     * bn99en_1 1 peter_jennings 0.806084 7.079850 <o,f4,male> Tonight this
     * Thursday big pressure on the Clinton administration to do something about
     * the latest killing in Yugoslavia
     * bn99en_1 1 peter_jennings 7.079850 14.007608 <o,fx,male> Airline passengers
     * and outrageous behavior at thirty thousand feet What can an airline do
     * ...
     * bn99en_1 1 inter_segment_gap 23.097000 28.647000 <o,fx,>
     * ...
     * </pre>
     * The format of each line is:
     * <pre>
     * test_set_name category speaker_name start_time_in_seconds
     * end_time_in_seconds <category,hub4_focus_conditions,speaker_sex> transcript
     * </pre>
     * In our example above,
     * <pre>
     * test_set_name is "bn99en_1"
     * category is "1"
     * speaker_name is "peter_jennings"
     * start_time_in_seconds is "0.806084"
     * end_time_in_seconds is "7.079850"
     * category is "o" for "Overall"
     * hub4_focus_conditions is:
     *     "f0" for "Baseline//Broadcast//Speech"
     *     "f1" for "Spontaneous//Broadcast//Speech"
     *     "f2" for "Speech Over//Telephone//Channels"
     *     "f3" for "Speech in the//Presence of//Background Music"
     *     "f4" for "Speech Under//Degraded//Acoustic Conditions"
     *     "f5" for "Speech from//Non-Native//Speakers"
     *     "fx" for "All other speech"
     * speaker_sex is "male"
     * transcript is "Tonight this Thursday big pressure on the Clinton
     * administration to do something about the latest killing in Yugoslavia
     * </pre>
     * The ConcatFileDataSource will produce such a transcript if the name of the file to write to is supplied in the
     * constructor. This transcript file will be used in detected gap insertion errors, because it accurately describes the
     * "correct" sequence of speech and silences in the concatenated version of the audio files.
     */
    public class ConcatFileDataSource : StreamDataSource, IReferenceSource
    {
        /// <summary>
        /// TThe property that specifies which file to start at.
        /// </summary>
        [S4Integer(DefaultValue = 1)]
        public static string PropStartFile = "startFile";

        /// <summary>
        /// The property that specifies the number of files to skip for every file read.
        /// </summary>
        [S4Integer(DefaultValue = 0)]
        public static string PropSkip = "skip";

        /// <summary>
        /// The property that specifies the total number of files to read. The default value should be no limit.
        /// </summary>
        [S4Integer(DefaultValue = -1)]
        public static string PropTotalFiles = "totalFiles";

        /// <summary>
        /// The property that specifies the silence audio file, if any. If this property is null, then no silences are added in between files.
        /// </summary>
        [S4String]
        public static string PropSilenceFile = "silenceFile";

        /// <summary>
        /// The property that specifies whether to add random silence.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropAddRandomSilence = "addRandomSilence";

        /// <summary>
        /// The property that specifies the maximum number of times the silence file is added  between files. If
        /// PROP_ADD_RANDOM_SILENCE is set to true, the number of times the silence file is added is between 1 and this
        /// value. If PROP_ADD_RANDOM_SILENCE is set to false, this value will be the number of times the silence file is
        /// added. So if PROP_MAX_SILENCE is set to 3, then the silence file will be added three times between files.
        /// </summary>
        [S4Integer(DefaultValue = 3)]
        public static string PropMaxSilence = "maxSilence";

        /// <summary>
        /// The property that specifies the name of the transcript file. 
        /// If this property is set, a transcript file will be created. No transcript file will be created if this property is not set.
        /// </summary>
        [S4String]
        public static string PropTranscriptFile = "transcriptFile";

        /// <summary>
        /// The property for the file containing a list of audio files to read from.
        /// </summary>
        [S4String]
        public static string PropBatchFile = "batchFile";


        internal static string GapLabel = "inter_segment_gap";
        internal bool AddRandomSilence;
        internal int Skip;
        internal int MaxSilence;
        internal int SilenceCount;
        private int _bytesPerSecond;
        internal long TotalBytes;
        internal long SilenceFileLength;
        internal string SilenceFileName;
        internal string NextFile;
        internal string Context;
        internal StreamWriter Transcript;
        private int _startFile;
        private int _totalFiles;
        private string _batchFile;

        public ConcatFileDataSource(int sampleRate, int bytesPerRead, int bitsPerSample, bool bigEndian, bool signedData,
            bool addRandomSilence,
            int maxSilence,
            int skip,
            string silenceFileName,
            int startFile,
            int totalFiles,
            string transcriptFile,
            string batchFile)
            : base(sampleRate, bytesPerRead, bitsPerSample, bigEndian, signedData)
        {

            _bytesPerSecond = sampleRate * (bitsPerSample / 8);
            AddRandomSilence = addRandomSilence;
            MaxSilence = maxSilence;
            Skip = skip;
            SilenceFileName = silenceFileName;
            _startFile = startFile;
            _totalFiles = totalFiles;
            TranscriptFile = transcriptFile;
            _batchFile = batchFile;
        }

        public ConcatFileDataSource()
        {

        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _bytesPerSecond = SampleRate * (BitsPerSample / 8);
            AddRandomSilence = ps.GetBoolean(PropAddRandomSilence);
            MaxSilence = ps.GetInt(PropMaxSilence);
            Skip = ps.GetInt(PropSkip);
            SilenceFileName = ps.GetString(PropSilenceFile);
            _startFile = ps.GetInt(PropStartFile);
            _totalFiles = ps.GetInt(PropTotalFiles);
            TranscriptFile = ps.GetString(PropTranscriptFile);
            _batchFile = ps.GetString(PropBatchFile);
        }


        /// <summary>
        /// Initializes a ConcatFileDataSource.
        /// </summary>
        /// <exception cref="System.Exception">BatchFile cannot be null!</exception>
        public override void Initialize()
        {
            base.Initialize();

            try
            {
                var silenceFile = new FileInfo(SilenceFileName);
                SilenceFileLength = silenceFile.Length;

                if (TranscriptFile != null)
                {
                    Transcript = new StreamWriter(TranscriptFile);
                }
                if (_batchFile == null)
                {
                    throw new Exception("BatchFile cannot be null!");
                }
                SetInputStream
                        (new SequenceInputStream
                                (new InputStreamEnumeration
                                        (this, _batchFile, _startFile, _totalFiles)));
                References = new List<string>();
            }
            catch (IOException e)
            {
                Trace.TraceError(e.Message); //TODO fix this
            }
        }

        /// <summary>
        /// Gets a list of all reference text. Implements the getReferences() method of ReferenceSource.
        /// </summary>
        /// <returns>A list of all reference text</returns>
        public IList<string> References { get; internal set; }

        /// <summary>
        /// Gets the name of the transcript file.
        /// </summary>
        /// <value>
        /// The name of the transcript file.
        /// </value>
        public string TranscriptFile { get; private set; }

        /// <summary>
        /// Returns the audio time in seconds represented by the given number of bytes.
        /// </summary>
        /// <param name="bytes">The number of bytes.</param>
        /// <returns>The audio time.</returns>
        internal float GetSeconds(long bytes)
        {
            return ((float)bytes / _bytesPerSecond);
        }
    }
}
