using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
using IDictionary = Syn.Speech.Linguist.Dictionary.IDictionary;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.NGram
{
    public class DynamicTrigramModel : LanguageModel
    {
        private IDictionary _dictionary;
        private readonly HashSet<string> _vocabulary;
        private float _unigramWeight;

        private List<string> _sentences;
        private readonly HashMap<WordSequence, Float> _logProbs;
        private readonly HashMap<WordSequence, Float> _logBackoffs;


        public DynamicTrigramModel()
        {
            _vocabulary = new HashSet<string>();
            _logProbs = new HashMap<WordSequence, Float>();
            _logBackoffs = new HashMap<WordSequence, Float>();
        }

        public DynamicTrigramModel(IDictionary dictionary)
            : this()
        {
            _dictionary = dictionary;
        }

        public override void NewProperties(PropertySheet ps)
        {
            _dictionary = (IDictionary)ps.GetComponent(PropDictionary);
            MaxDepth = ps.GetInt(PropMaxDepth);
            _unigramWeight = ps.GetFloat(PropUnigramWeight);
        }

        public override void Allocate()
        {
           _vocabulary.Clear();
        _logProbs.Clear();
        _logBackoffs.Clear();
        HashMap<WordSequence, Integer> unigrams = new HashMap<WordSequence, Integer>();
        HashMap<WordSequence, Integer> bigrams = new HashMap<WordSequence, Integer>();
        HashMap<WordSequence, Integer> trigrams = new HashMap<WordSequence, Integer>();
        int wordCount = 0;

        foreach (string sentence in _sentences) {
            string[] textWords = sentence.Split("\\s+");
            var words = new List<Word>();
            words.Add(_dictionary.GetSentenceStartWord());
            foreach (String wordString in textWords) {
        	if (wordString.Length == 0) {
        	    continue;
        	}
                _vocabulary.Add(wordString);
                Word word = _dictionary.GetWord(wordString);
                if (word == null) {
                    words.Add(Word.Unknown);
                } else {
                    words.Add(word);
                }
            }
            words.Add(_dictionary.GetSentenceEndWord());

            if (words.Count > 0) {
                AddSequence(unigrams, new WordSequence(words[0]));
                wordCount++;
            }

            if (words.Count > 1) {
                wordCount++;
                AddSequence(unigrams, new WordSequence(words[1]));
                AddSequence(bigrams, new WordSequence(words[0], words[1]));
            }

            for (int i = 2; i < words.Count; ++i) {
                wordCount++;
                AddSequence(unigrams, new WordSequence(words[i]));
                AddSequence(bigrams, new WordSequence(words[i - 1], words[i]));
                AddSequence(trigrams, new WordSequence(words[i - 2], words[i - 1], words[i]));
            }
        }

        float discount = .5f;
        float deflate = 1 - discount;
        var uniprobs = new HashMap<WordSequence, Float>();
        foreach (var e in unigrams) {
            uniprobs.Put(e.Key, e.Value * deflate / wordCount);
        }

        LogMath lmath = LogMath.GetLogMath();
        float logUnigramWeight = lmath.LinearToLog(_unigramWeight);
        float invLogUnigramWeight = lmath.LinearToLog(1 - _unigramWeight);
        float logUniformProb = -lmath.LinearToLog(uniprobs.Count);

        var sorted1Grams = new SortedSet<WordSequence>(unigrams.Keys);
        var iter = new SortedSet<WordSequence>(bigrams.KeySet()).GetEnumerator();
        WordSequence ws = iter.MoveNext() ? iter.Current : null;
        foreach (WordSequence unigram in sorted1Grams) {
            float p = lmath.LinearToLog(uniprobs.Get(unigram));
            p += logUnigramWeight;
            p = lmath.AddAsLinear(p, logUniformProb + invLogUnigramWeight);
            _logProbs.Put(unigram, p);

            float sum = 0f;
            while (ws != null) {
                int cmp = ws.GetOldest().CompareTo(unigram);
                if (cmp > 0) {
                    break;
                }
                if (cmp == 0) {
                    sum += uniprobs.Get(ws.GetNewest());
                }
                ws = iter.MoveNext() ? iter.Current : null;
            }

            _logBackoffs.Put(unigram, lmath.LinearToLog(discount / (1 - sum)));
        }

        var biprobs = new HashMap<WordSequence, Float>();
        foreach (var entry in bigrams) {
            int unigramCount = unigrams.Get(entry.Key.GetOldest());
            biprobs.Put(entry.Key, entry.Value * deflate / unigramCount);
        }

        var sorted2Grams = new SortedSet<WordSequence>(bigrams.KeySet());
        iter = new SortedSet<WordSequence>(trigrams.KeySet()).GetEnumerator();
        ws = iter.MoveNext() ? iter.Current : null;
        foreach (WordSequence biword in sorted2Grams) {
            _logProbs.Put(biword, lmath.LinearToLog(biprobs.Get(biword)));

            float sum = 0f;
            while (ws != null) {
                int cmp = ws.GetOldest().CompareTo(biword);
                if (cmp > 0) {
                    break;
                }
                if (cmp == 0) {
                    sum += biprobs.Get(ws.GetNewest());
                }
                ws = iter.MoveNext() ? iter.Current : null;
            }
            _logBackoffs.Put(biword, lmath.LinearToLog(discount / (1 - sum)));
        }

        foreach (var e in trigrams) {
            float p = e.Value * deflate;
            p /= bigrams.Get(e.Key.GetOldest());
            _logProbs.Put(e.Key, lmath.LinearToLog(p));
        }
        }

        private void AddSequence(HashMap<WordSequence, Integer> grams, WordSequence wordSequence)
        {
            Integer count = grams.Get(wordSequence);
            if (count != null)
            {
                grams.Put(wordSequence, count + 1);
            }
            else
            {
                grams.Put(wordSequence, 1);
            }
        }

        public override void Deallocate() { }

        public override float GetProbability(WordSequence wordSequence)
        {
            float prob;
            if (_logProbs.ContainsKey(wordSequence))
            {
                prob = _logProbs[wordSequence];
            }
            else if (wordSequence.Size > 1)
            {
                Float backoff = _logBackoffs[wordSequence.GetOldest()];
                if (backoff == null)
                {
                    prob = LogMath.LogOne + GetProbability(wordSequence.GetNewest());
                }
                else
                {
                    prob = backoff + GetProbability(wordSequence.GetNewest());
                }
            }
            else
            {
                prob = LogMath.LogZero;
            }
            return prob;
        }

        public override float GetSmear(WordSequence wordSequence)
        {
            return 0;
        }

        public override HashSet<string> Vocabulary
        {
            get { return _vocabulary; }
        }

        public override int MaxDepth { get; set; }

        public void SetText(List<string> textWords)
        {
            _sentences = textWords;
        }
    }
}
