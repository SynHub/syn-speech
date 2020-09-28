using System;
//REFACTORED
namespace Syn.Speech.Linguist.Dictionary
{
    /// <summary>
    /// Represents a word, its spelling and its pronunciation.
    /// </summary>
    public class Word : IComparable<Word>
    {
        /// <summary>
        /// The Word representing the unknown word. 
        /// </summary>
        public static Word Unknown;

         static Word() {
            Pronunciation[] pros = {Pronunciation.Unknown};
            Unknown = new Word("<unk>", pros, false);
            Pronunciation.Unknown.SetWord(Unknown);
        }

        /// <summary>
        /// pronunciations of this word
        /// </summary>
        private readonly Pronunciation[] _pronunciations = { Pronunciation.Unknown };

        /// <summary>
        /// Creates a Word
        /// </summary>
        /// <param name="spelling">the spelling of this word</param>
        /// <param name="pronunciations">the pronunciations of this word</param>
        /// <param name="isFiller">true if the word is a filler word</param>
        public Word(String spelling, Pronunciation[] pronunciations,
                Boolean isFiller)
        {
            Spelling = spelling;
            _pronunciations = pronunciations;
            IsFiller = isFiller;
        }

        /// <summary>
        /// Returns the spelling of the word.
        /// </summary>
        /// <value></value>
        public string Spelling { get; private set; }

        /// <summary>
        /// Determines if this is a filler word
        /// </summary>
        /// <value>&lt;code&gt;true&lt;/code&gt; if this word is a filler word, otherwise it returns &lt;code&gt;false&lt;/code&gt;</value>
        public bool IsFiller { get; private set; }

        /// <summary>
        /// Returns true if this word is an end of sentence word
        /// </summary>
        /// <value>true if the word matches Dictionary.SENTENCE_END_SPELLING</value>
        public bool IsSentenceEndWord
        {
            get { return Spelling.Equals(IDictionary.SentenceEndSpelling); }
        }

        /// <summary>
        /// Returns true if this word is a start of sentence word
        /// </summary>
        /// <value>true if the word matches Dictionary.SENTENCE_START_SPELLING</value>
        public bool IsSentenceStartWord
        {
            get { return IDictionary.SentenceStartSpelling.Equals(Spelling); }
        }

        /// <summary>
        /// Retrieves the pronunciations of this word
        /// </summary>
        /// <param name="wordClassification">the classification of the word (typically part
        ///        of speech classification) or null if all word classifications are
        ///        acceptable. The word classification must be one of the set
        ///        returned by
        ///        <code>Dictionary.getPossibleWordClassifications</code></param>
        /// <returns>the pronunciations of this word</returns>
        public Pronunciation[] GetPronunciations(WordClassification wordClassification)
        {
            return _pronunciations;
        }

        /// <summary>
        /// Retrieves the pronunciations of this word
        /// </summary>
        /// <returns>the pronunciations of this word</returns>
        public Pronunciation[] GetPronunciations()
        {
            return _pronunciations;
        }
        /// <summary>
        /// Get the highest probability pronunciation for a word
        /// </summary>
        /// <returns>the highest probability pronunciation</returns>
        public Pronunciation GetMostLikelyPronunciation()
        {
            float bestScore = float.NegativeInfinity;
            Pronunciation best = null;
            foreach (Pronunciation pronunciation in _pronunciations)
            {
                if (pronunciation.Probability > bestScore)
                {
                    bestScore = pronunciation.Probability;
                    best = pronunciation;
                }
            }
            return best;
        }

        public override int GetHashCode()
        {
            return Spelling.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            return obj is Word && Spelling.Equals(((Word)obj).Spelling);;
        }
        /// <summary>
        /// Returns a string representation of this word, which is the spelling
        /// </summary>
        /// <returns>the spelling of this word</returns>
        public override string ToString()
        {
            return Spelling;
        }

        public int CompareTo(Word other)
        {
            return String.Compare(Spelling, other.Spelling, StringComparison.Ordinal);
        }


        int IComparable<Word>.CompareTo(Word other)
        {
            return CompareTo(other);
        }
    }
}
