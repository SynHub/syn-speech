using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist
{
    /// <summary>
    /// This class can be used to keep track of a word sequence. This class is an
    /// immutable class. It can never be modified once it is created (except,
    /// perhaps for transient, cached things such as a precalculated hashcode).
    /// </summary>
    public class WordSequence : IComparable<WordSequence>
    {
        int IComparable<WordSequence>.CompareTo(WordSequence other)
        {
            return GetOldest().CompareTo(other.GetOldest()); 
        }

        /// <summary>
        /// an empty word sequence, that is, it has no words.
        /// </summary>
        public static WordSequence Empty = new WordSequence(0);

        public static WordSequence AsWordSequence(IDictionary dictionary, params String[] words) 
        {
            var dictWords = new Word[words.Length];
            for (var i = 0; i < words.Length; i++) 
            {
                dictWords[i] = dictionary.GetWord(words[i]);
            }
            return new WordSequence(dictWords);
        }

        private readonly Word[] _words;
        private int _hashCode = -1;

        /// <summary>
        /// Constructs a word sequence with the given depth. 
        /// </summary>
        /// <param name="size">the maximum depth of the word history</param>
        private WordSequence(int size) 
        {
            _words = new Word[size];
        }
        /// <summary>
        /// Constructs a word sequence with the given word IDs
        /// </summary>
        /// <param name="words">the word IDs of the word sequence</param>
        public WordSequence(params Word[] words): this(words.ToList())
        {
            
        }
        /// <summary>
        /// Constructs a word sequence from the list of words
        /// </summary>
        /// <param name="list">the list of words</param>
        public WordSequence(IEnumerable<Word> list) 
        {
            _words = list.ToArray();
            Check();
        }

        public void Check() 
        {
            foreach(var word in _words)
                if (word == null)
                    throw new Exception("WordSequence should not have null Words.");
        }
        /// <summary>
        /// Returns a new word sequence with the given word added to the sequence 
        /// </summary>
        /// <param name="word">the word to add to the sequence</param>
        /// <param name="maxSize">the maximum size of the generated sequence</param>
        /// <returns>a new word sequence with the word added (but trimmed to maxSize)</returns>
        public WordSequence AddWord(Word word, int maxSize) 
        {
            if (maxSize <= 0) 
            {
                return Empty;
            }
            var nextSize = ((Size + 1) > maxSize) ? maxSize : (Size + 1);
            var next = new WordSequence(nextSize);
            var nextIndex = nextSize - 1;
            var thisIndex = Size - 1;
            next._words[nextIndex--] = word;

            while (nextIndex >= 0 && thisIndex >= 0) {
                next._words[nextIndex--] = _words[thisIndex--];
            }
            next.Check();

            return next;
        }
        /// <summary>
        /// Returns the oldest words in the sequence (the newest word is omitted) 
        /// </summary>
        /// <returns>the oldest words in the sequence, with the newest word omitted</returns>
        public WordSequence GetOldest() 
        {
            var next = Empty;

            if (Size >= 1) 
            {
                next = new WordSequence(_words.Length - 1);
                Array.Copy(_words,next._words,next._words.Count());
                //System.arraycopy(this.words, 0, next.words, 0, next.words.Length);
            }
            return next;
        }
        /// <summary>
        /// Returns the newest words in the sequence (the old word is omitted)
        /// </summary>
        /// <returns>the newest words in the sequence with the oldest word omitted</returns>
        public WordSequence GetNewest() 
        {
            var next = Empty;

            if (Size >= 1) 
            {
                next = new WordSequence(_words.Length - 1);
                Array.Copy(_words,1, next._words, 0, next._words.Length);
               // words.CopyTo(next.words,1);
                //System.arraycopy(this.words, 1, next.words, 0, next.words.length);
            }
            return next;
        }

        /// <summary>
        /// Returns a word sequence that is no longer than the given size, that is
        /// filled in with the newest words from this sequence 
        /// </summary>
        /// <param name="maxSize">the maximum size of the sequence</param>
        /// <returns>a new word sequence, trimmed to maxSize.</returns>
        public WordSequence Trim(int maxSize)
        {
            if (maxSize <= 0 || Size == 0) 
            {
                return Empty;
            }
            if (maxSize == Size) 
            {
                return this;
            }
            if (maxSize > Size) 
            {
                maxSize = Size;
            }
            var next = new WordSequence(maxSize);
            var thisIndex = _words.Length - 1;
            var nextIndex = next._words.Length - 1;

            for (var i = 0; i < maxSize; i++) 
            {
                next._words[nextIndex--] = _words[thisIndex--];
            }
            return next;
        }

        /// <summary>
        /// Returns the n-th word in this sequence
        /// </summary>
        /// <param name="n">which word to return</param>
        /// <returns>the n-th word in this sequence</returns>
        public Word GetWord(int n) 
        {
            Debug.Assert( n < _words.Length);
            return _words[n];
        }

        /// <summary>
        /// Returns the number of words in this sequence
        /// </summary>
        /// <value></value>
        public int Size
        {
            get { return _words.Length; }
        }

        /// <summary>
        /// Returns a string representation of this word sequence. The format is:
        /// [ID_0][ID_1][ID_2].
        /// </summary>
        /// <returns></returns>
        public override string ToString() 
        {
            var sb = new StringBuilder();
            foreach (var word in _words)
                sb.Append('[').Append(word).Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Calculates the hashcode for this object
        /// </summary>
        /// <returns></returns>

        public override int GetHashCode()
        {
            //TODO: Performance Critical - Computed 1000s of timers per session
            if (_hashCode == -1)
            {
                var code = 123;
                for (var i = 0; i < _words.Length; i++)
                {
                    code += _words[i].GetHashCode() * (2 * i + 1);
                }
                _hashCode = code;
            }
            return _hashCode;
        }

        /// <summary>
        /// compares the given object to see if it is identical to this WordSequence 
        /// </summary>
        /// <param name="objectparam">the object to compare this to</param>
        /// <returns>true if the given object is equal to this object</returns>

        #region Original Code
        public override bool Equals(Object objectparam)
        {
            //TODO: Performance critical - Computed 1000s of times per session (But caching this method gives only 1 second advantage) - No Cache Benefits
            if (this == objectparam)
                return true;
            if (!(objectparam is WordSequence))
                return false;
            return Arrays.AreEqual(_words, ((WordSequence)objectparam)._words);
        }
        #endregion
       
     

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="stopIndex"></param>
        /// <returns>a subsequence with both <code>startIndex</code> and  <code>stopIndex</code> exclusive.</returns>
        public WordSequence GetSubSequence(int startIndex, int stopIndex) 
        {
            var subseqWords = new List<Word>();

            for (var i = startIndex; i < stopIndex; i++) {
                subseqWords.Add(GetWord(i));
            }

            return new WordSequence(subseqWords);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>the words of the <code>WordSequence</code>.</returns>
        public Word[] GetWords()
        {
            return GetSubSequence(0, Size)._words; // create a copy to keep the
            // class immutable
        }

        public int CompareTo(WordSequence other) 
        {
            var n = Math.Min(_words.Length, other._words.Length);
            for (var i = 0; i < n; ++i) 
            {
                if (!_words[i].Equals(other._words[i])) 
                {
                    return _words[i].CompareTo(other._words[i]);
                }
            }

            return _words.Length - other._words.Length;
        }
    }
}
