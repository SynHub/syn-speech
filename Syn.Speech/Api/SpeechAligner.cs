using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syn.Speech.Logging;
using Syn.Speech.Alignment;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Language.Grammar;
using Syn.Speech.Linguist.Language.NGram;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Api
{
    public class SpeechAligner
    {
        //private Logger logger = Logger.getLogger(typeof(SpeechAligner).Name);

        private const int TupleSize = 3;
        private const int MinLmAlignSize = 20;

        private readonly Context _context;
        private readonly Recognizer _recognizer;

        private readonly AlignerGrammar _grammar;
        private readonly DynamicTrigramModel _languageModel;

        public SpeechAligner(string amPath, string dictPath, string g2PPath)
        {
            var configuration = new Configuration();
            configuration.AcousticModelPath = amPath;
            configuration.DictionaryPath = dictPath;

            _context = new Context(configuration);
            if (g2PPath != null)
            {
                _context.SetLocalProperty("dictionary->allowMissingWords", "true");
                _context.SetLocalProperty("dictionary->createMissingWords", "true");
                _context.SetLocalProperty("dictionary->g2pModelPath", g2PPath);
                _context.SetLocalProperty("dictionary->g2pMaxPron", "2");
            }
            _context.SetLocalProperty("lexTreeLinguist->languageModel","dynamicTrigramModel");
            _recognizer = (Recognizer)_context.GetInstance(typeof(Recognizer));
            _grammar = (AlignerGrammar)_context.GetInstance(typeof(AlignerGrammar));
            _languageModel = (DynamicTrigramModel)_context.GetInstance(typeof(DynamicTrigramModel));
            Tokenizer = new UsEnglishTokenizer();
        }

        public List<WordResult> Align(FileInfo audioUrl, string transScript)
        {
            return Align(audioUrl, Tokenizer.Expand(transScript));
        }

        public List<WordResult> Align(FileInfo audioUrl, List<string> sentenceTranscript)
        {
            var transcript = SentenceToWords(sentenceTranscript);

            var aligner = new LongTextAligner(transcript, TupleSize);
            var alignedWords = new Dictionary<int, WordResult>();
            var ranges = new LinkedList<Range>();
            //var texts = new ArrayDeque();
            //var timeFrames = new ArrayDeque();
            var texts = new LinkedList<List<string>>();
            var timeFrames = new LinkedList<TimeFrame>();

            ranges.AddLast(new Range(0, transcript.Count));
            texts.Offer(transcript);
            TimeFrame totalTimeFrame = TimeFrame.Infinite;
            timeFrames.Offer(totalTimeFrame);
            long lastFrame = TimeFrame.Infinite.End;

            for (int i = 0; i < 4; i++)
            {
                if (i == 3)
                {
                    _context.SetLocalProperty("decoder->searchManager", "alignerSearchManager");
                }

                while (texts.Count != 0)
                {
                    Debug.Assert(texts.Count == ranges.Count);
                    Debug.Assert(texts.Count == timeFrames.Count);

                    var text = texts.Poll();
                    var frame = timeFrames.Poll();
                    var range = ranges.Poll();

                    if (i < 3 && texts.Count < MinLmAlignSize)
                    {
                        continue;
                    }

                    this.LogInfo("Aligning frame " + frame + " to text " + text + " range " + range);

                    if (i < 3)
                    {
                        _languageModel.SetText(text);
                    }

                    _recognizer.Allocate();

                    if (i == 3)
                    {
                        _grammar.SetWords(text);
                    }

                    _context.SetSpeechSource(audioUrl.OpenRead(), frame);

                    var hypothesis = new List<WordResult>();
                    Result speechResult;
                    while (null != (speechResult = _recognizer.Recognize()))
                    {
                        hypothesis.AddRange(speechResult.GetTimedBestResult(false));
                    }

                    if (i == 0)
                    {
                        if (hypothesis.Count > 0)
                        {
                            lastFrame = hypothesis[hypothesis.Count - 1].TimeFrame.End;
                        }
                    }

                    var words = new List<string>();
                    foreach (WordResult wr in hypothesis)
                    {
                        words.Add(wr.Word.Spelling);
                    }
                    int[] alignment = aligner.Align(words, range);
                    List<WordResult> results = hypothesis;
                    this.LogInfo("Decoding result is " + results);

                    // dumpAlignment(transcript, alignment, results);
                    DumpAlignmentStats(transcript, alignment, results);

                    for (int j = 0; j < alignment.Length; j++)
                    {
                        if (alignment[j] != -1)
                        {
                            alignedWords.Add(alignment[j], hypothesis[j]);
                        }
                    }

                    _recognizer.Deallocate();
                }
                ScheduleNextAlignment(transcript, alignedWords, ranges, texts, timeFrames, lastFrame);


            }
            return new List<WordResult>(alignedWords.Values);
        }

        public List<string> SentenceToWords(List<string> sentenceTranscript)
        {
            var transcript = new List<string>();
            foreach (var sentence in sentenceTranscript)
            {
                string[] words = sentence.Split("\\s+");
                foreach (var word in words)
                {
                    if (word.Length > 0) { transcript.Add(word);}
                }
            }
            return transcript;
        }

        private void DumpAlignmentStats(List<String> transcript, int[] alignment, List<WordResult> results)
        {
            int insertions = 0;
            int deletions = 0;
            int size = transcript.Count;
            int[] aid = alignment;
            int lastId = -1;
            for (int ij = 0; ij < aid.Length; ++ij)
            {
                if (aid[ij] == -1)
                {
                    insertions++;
                }
                else
                {
                    if (aid[ij] - lastId > 1)
                    {
                        deletions += aid[ij] - lastId;
                    }
                    lastId = aid[ij];
                }
            }
            if (lastId >= 0 && transcript.Count - lastId > 1)
            {
                deletions += transcript.Count - lastId;
            }
            this.LogInfo(String.Format("Size {0} deletions {1} insertions {2} error rate {3}", size, insertions, deletions,(insertions + deletions) / ((float)size) * 100f));
        }

        private void ScheduleNextAlignment(List<string> transcript, Dictionary<int, WordResult> alignedWords, LinkedList<Range> ranges, LinkedList<List<string>> texts, LinkedList<TimeFrame> timeFrames, long lastFrame)
        {
            int prevKey = -1;
            long prevEnd = 0;
            foreach (var e in alignedWords)
            {
                if (e.Key - prevKey > 1)
                {
                    CheckedOffer(transcript, texts, timeFrames, ranges,
                            prevKey + 1, e.Key, prevEnd, e.Value.TimeFrame.Start);
                }
                prevKey = e.Key;
                prevEnd = e.Value.TimeFrame.End;
            }
            if (transcript.Count - prevKey > 1)
            {
                CheckedOffer(transcript, texts, timeFrames, ranges,
                        prevKey + 1, transcript.Count, prevEnd, lastFrame);
            }
        }

        private void DumpAlignment(List<string> transcript, int[] alignment, List<WordResult> results)
        {
            this.LogInfo("Alignment");
            int[] aid = alignment;
            int lastId = -1;
            for (int ij = 0; ij < aid.Length; ++ij)
            {
                if (aid[ij] == -1)
                {
                    this.LogInfo(string.Format("+ {0}", results[ij]));
                }
                else
                {
                    if (aid[ij] - lastId > 1)
                    {
                        foreach (string result1 in transcript.GetRange(lastId + 1, aid[ij]))
                        {
                            this.LogInfo(string.Format("- %-25s", result1));
                        }
                    }
                    else
                    {
                        this.LogInfo(string.Format("  %-25s", transcript[aid[ij]]));
                    }
                    lastId = aid[ij];
                }
            }

            if (lastId >= 0 && transcript.Count - lastId > 1)
            {
                foreach (string result1 in transcript.GetRange(lastId + 1,
                        transcript.Count))
                {
                    this.LogInfo(string.Format("- %-25s", result1));
                }
            }
        }

        private void CheckedOffer(List<string> transcript, LinkedList<List<string>> texts, LinkedList<TimeFrame> timeFrames, LinkedList<Range> ranges, int start, int end, long timeStart, long timeEnd)
        {

            var wordDensity = ((double)(timeEnd - timeStart)) / (end - start);

            // Skip range if it's too short, average word is less than 10
            // milliseconds
            if (wordDensity < 10.0)
            {
                this.LogInfo("Skipping text range due to a high density " + transcript.GetRange(start, end));
                return;
            }

            texts.Offer(transcript.GetRange(start, end));
            timeFrames.Offer(new TimeFrame(timeStart, timeEnd));
            ranges.Offer(new Range(start, end - 1));
        }

        public ITextTokenizer Tokenizer { get; set; }
    }
}
