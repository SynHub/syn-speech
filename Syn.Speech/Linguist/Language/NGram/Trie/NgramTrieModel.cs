using System;
using System.Collections.Generic;
using System.IO;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Util;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    public class NgramTrieModel : LanguageModel
    {
        [S4String(Mandatory = false)]
        public const string PROP_QUERY_LOG_FILE = "queryLogFile";

        [S4Integer(DefaultValue = 100000)]
        public const string PROP_NGRAM_CACHE_SIZE = "ngramCacheSize";

        [S4Boolean(DefaultValue = false)]
        public const string PROP_CLEAR_CACHES_AFTER_UTTERANCE = "clearCachesAfterUtterance";

        [S4Double(DefaultValue = 1.0f)]
        public const string PROP_LANGUAGE_WEIGHT = "languageWeight";

        [S4Boolean(DefaultValue = false)]
        public const string PROP_APPLY_LANGUAGE_WEIGHT_AND_WIP = "applyLanguageWeightAndWip";

        [S4Double(DefaultValue = 1.0f)]
        public const string PROP_WORD_INSERTION_PROBABILITY = "wordInsertionProbability";

        // ------------------------------
        // Configuration data
        // ------------------------------

        URL location;
        protected LogMath logMath;
        protected int curDepth;
        protected int[] counts;

        protected int ngramCacheSize;
        protected bool clearCacheAfterUtterance;

        protected IDictionary dictionary;
        protected string format;
        protected bool applyLanguageWeightAndWip;
        protected float languageWeight;
        protected float unigramWeight;
        protected float logWip;

        // -------------------------------
        // Statistics
        // -------------------------------

        protected string ngramLogFile;

        // -------------------------------
        // subcomponents
        // --------------------------------
        private BinaryLoader _loader;
        private StreamWriter logFile;

        //-----------------------------
        // Trie structure
        //-----------------------------
        protected TrieUnigram[] unigrams;
        protected string[] words;
        protected NgramTrieQuant quant;
        protected NgramTrie trie;

        //-----------------------------
        // Working data
        //-----------------------------
        protected HashMap<Word, Integer> unigramIDMap;
        private LRUCache<WordSequence, Float> ngramProbCache;

        public NgramTrieModel(string format, URL location, string ngramLogFile,
           int maxNGramCacheSize, bool clearCacheAfterUtterance,
           int maxDepth, IDictionary dictionary,
           bool applyLanguageWeightAndWip, float languageWeight,
           double wip, float unigramWeight)
        {

            this.format = format;
            this.location = location;
            this.ngramLogFile = ngramLogFile;
            this.ngramCacheSize = maxNGramCacheSize;
            this.clearCacheAfterUtterance = clearCacheAfterUtterance;
            this.MaxDepth = maxDepth;
            logMath = LogMath.GetLogMath();
            this.dictionary = dictionary;
            this.applyLanguageWeightAndWip = applyLanguageWeightAndWip;
            this.languageWeight = languageWeight;
            this.logWip = logMath.LinearToLog(wip);
            this.unigramWeight = unigramWeight;
        }

        public override void NewProperties(PropertySheet ps)
        {
            logMath = LogMath.GetLogMath();
            location = ConfigurationManagerUtils.GetResource(PropLocation, ps);
            ngramLogFile = ps.GetString(PROP_QUERY_LOG_FILE);
            MaxDepth = ps.GetInt(LanguageModel.PropMaxDepth);
            ngramCacheSize = ps.GetInt(PROP_NGRAM_CACHE_SIZE);
            clearCacheAfterUtterance = ps.GetBoolean(PROP_CLEAR_CACHES_AFTER_UTTERANCE);
            dictionary = (IDictionary)ps.GetComponent(PropDictionary);
            applyLanguageWeightAndWip = ps.GetBoolean(PROP_APPLY_LANGUAGE_WEIGHT_AND_WIP);
            languageWeight = ps.GetFloat(PROP_LANGUAGE_WEIGHT);
            logWip = logMath.LinearToLog(ps.GetDouble(PROP_WORD_INSERTION_PROBABILITY));
            unigramWeight = ps.GetFloat(PropUnigramWeight);
        }

        private void BuildUnigramIDMap()
        {
            int missingWords = 0;
            if (unigramIDMap == null)
                unigramIDMap = new HashMap<Word, Integer>();
            for (int i = 0; i < words.Length; i++)
            {
                Word word = dictionary.GetWord(words[i]);
                if (word == null)
                {
                    //logger.warning("The dictionary is missing a phonetic transcription for the word '" + words[i] + "'");
                    missingWords++;
                }

                unigramIDMap.Put(word, i);

                //if (logger.isLoggable(Level.FINE)) logger.fine("Word: " + word);
            }

            if (missingWords > 0) Logger.LogWarning<NgramTrieModel>("Dictionary is missing " + missingWords + " words that are contained in the language model.");
        }

        public override void Allocate()
        {
            TimerPool.GetTimer(this, "Load LM").Start();

            this.LogInfo("Loading n-gram language model from: " + location);

            // create the log file if specified
            if (ngramLogFile != null)
                logFile = new StreamWriter(ngramLogFile);

            BinaryLoader loader;
            if (location.Path == null || location.Path.Equals("file"))
            {
                try
                {
                    loader = new BinaryLoader(new FileInfo(location.Path));
                }
                catch (Exception ex)
                {
                    loader = new BinaryLoader(new FileInfo(location.Path));
                }
            }
            else
            {
                loader = new BinaryLoader(location);
            }
            loader.verifyHeader();
            counts = loader.readCounts();
            if (MaxDepth <= 0 || MaxDepth > counts.Length)
                MaxDepth = counts.Length;
            if (MaxDepth > 1)
            {
                quant = loader.readQuant(MaxDepth);
            }
            unigrams = loader.readUnigrams(counts[0]);
            if (MaxDepth > 1)
            {
                trie = new NgramTrie(counts, quant.getProbBoSize(), quant.getProbSize());
                loader.readTrieByteArr(trie.getMem());
            }
            //string words can be read here
            words = loader.readWords(counts[0]);
            BuildUnigramIDMap();
            ngramProbCache = new LRUCache<WordSequence, Float>(ngramCacheSize);
            loader.close();
            TimerPool.GetTimer(this, "Load LM").Stop();
        }

        public override void Deallocate()
        {
            if (logFile != null)
            {
                logFile.Flush();
            }
        }

        private float GetAvailableProb(WordSequence wordSequence, TrieRange range, float prob)
        {
            if (!range.isSearchable()) return prob;
            for (int reverseOrderMinusTwo = wordSequence.Size - 2; reverseOrderMinusTwo >= 0; reverseOrderMinusTwo--)
            {
                int orderMinusTwo = wordSequence.Size - 2 - reverseOrderMinusTwo;
                if (orderMinusTwo + 1 == MaxDepth) break;
                int wordId = unigramIDMap.Get(wordSequence.GetWord(reverseOrderMinusTwo));
                float updatedProb = trie.readNgramProb(wordId, orderMinusTwo, range, quant);
                if (!range.getFound()) break;
                prob = updatedProb;
                curDepth++;
                if (!range.isSearchable()) break;
            }
            return prob;
        }

        private float GetAvailableBackoff(WordSequence wordSequence)
        {
            float backoff = 0.0f;
            int wordsNum = wordSequence.Size;
            int wordId = unigramIDMap.Get(wordSequence.GetWord(wordsNum - 2));
            TrieRange range = new TrieRange(unigrams[wordId].next, unigrams[wordId + 1].next);
            if (curDepth == 1)
            {
                backoff += unigrams[wordId].backoff;
            }
            int sequenceIdx, orderMinusTwo;
            for (sequenceIdx = wordsNum - 3, orderMinusTwo = 0; sequenceIdx >= 0; sequenceIdx--, orderMinusTwo++)
            {
                int tmpWordId = unigramIDMap.Get(wordSequence.GetWord(sequenceIdx));
                float tmpBackoff = trie.readNgramBackoff(tmpWordId, orderMinusTwo, range, quant);
                if (!range.getFound()) break;
                backoff += tmpBackoff;
                if (!range.isSearchable()) break;
            }
            return backoff;
        }

        private float GetProbabilityRaw(WordSequence wordSequence)
        {
            int wordsNum = wordSequence.Size;
            int wordId = unigramIDMap.Get(wordSequence.GetWord(wordsNum - 1));
            TrieRange range = new TrieRange(unigrams[wordId].next, unigrams[wordId + 1].next);
            float prob = unigrams[wordId].prob;
            curDepth = 1;
            if (wordsNum == 1)
                return prob;
            //find prob of ngrams of higher order if any
            prob = GetAvailableProb(wordSequence, range, prob);
            if (curDepth < wordsNum)
            {
                //use backoff for rest of ngram
                prob += GetAvailableBackoff(wordSequence);
            }
            return prob;
        }

        private float ApplyWeights(float score)
        {
            //TODO ignores unigram weight. Apply or remove from properties
            if (applyLanguageWeightAndWip)
                return score * languageWeight + logWip;
            return score;
        }

        public override float GetProbability(WordSequence wordSequence)
        {
            int numberWords = wordSequence.Size;
            if (numberWords > MaxDepth)
            {
                throw new Error("Unsupported NGram: " + wordSequence.Size);
            }

            if (numberWords == MaxDepth)
            {
                Float probabilityA = ngramProbCache.Get(wordSequence);

                if (probabilityA != null)
                {
                    NGramHits++;
                    return probabilityA;
                }
                NGramMisses++;
            }
            float probability = ApplyWeights(GetProbabilityRaw(wordSequence));
            if (numberWords == MaxDepth)
                ngramProbCache.Put(wordSequence, probability);
            if (logFile != null)
                logFile.WriteLine(wordSequence.ToString().Replace("][", " ") + " : " + Float.ToString(probability));
            return probability;
        }

        public override float GetSmear(WordSequence wordSequence)
        {
            return 0;
        }

        public override HashSet<string> Vocabulary
        {
            get
            {
                //var words = _loader.Words;
                var vocabulary = new HashSet<string>(words);

                //var toReturn = new IReadOnlyList<string>(loader.getWords());
                return vocabulary; //TODO: should be a ReadOnlyCollection ( but has been set as such for performance )
            }
        }

        public int NGramMisses { get; private set; }

        public int NGramHits{ get; private set; }

        public override int MaxDepth { get; set; }

        private void clearCache()
        {
            Logger.LogInfo<NgramTrieModel>("LM Cache Size: " + ngramProbCache.Count + " Hits: " + NGramHits + " Misses: " + NGramMisses);
            if (clearCacheAfterUtterance)
            {
                ngramProbCache = new LRUCache<WordSequence, Float>(ngramCacheSize);
            }
        }

        public void onUtteranceEnd()
        {
            clearCache();
            if (logFile != null)
            {
                logFile.WriteLine("<END_UTT>");
                logFile.Flush();
            }
        }
    }
}
