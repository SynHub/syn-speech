using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// A confusion set is a set of words with their associated posteriors. In Java terms it's a SortedMap from posteriors to
    /// sets of WordResults, but the class is called a set because that's what this beast is known as in the literature.
    /// @author P. Gorniak
    /// </summary>
    public class ConfusionSet : SortedDictionary<Double, HashSet<WordResult>>
    {
        /// <summary>
        /// Add a word hypothesis to this confusion set.
        /// </summary>
        /// <param name="word">The hypothesis to add.</param>
        public void AddWordHypothesis(WordResult word)
        {
            var wordSet = GetWordSet(word.GetConfidence());
            if (wordSet == null)
            {
                wordSet = new HashSet<WordResult>();
                Java.Put(this, word.GetConfidence(), wordSet);
            }
            wordSet.Add(word);
        }


        /// <summary>
        /// Get the word set with this confidence.
        /// </summary>
        /// <param name="posterior">The confidence (posterior).</param>
        /// <returns>A set of hypotheses with this confidence, null if no such hypotheses</returns>
        public HashSet<WordResult> GetWordSet(double posterior)
        {
            return this.Get(posterior);
        }

        /// <summary>
        /// Return the set of best hypothesis. This will usually contain one hypothesis, but might contain more case there
        /// are some that have exactly equal confidences.
        /// </summary>
        /// <returns>A set of best hypotheses</returns>
        public HashSet<WordResult> GetBestHypothesisSet()
        {
            return this.Get(Keys.Last());
        }

        /// <summary>
        /// Return the single best hypothesis. Breaks ties arbitrarily.
        /// </summary>
        /// <returns>The best hypothesis stored in this confusion set (by confidence).</returns>
        public WordResult GetBestHypothesis()
        {
            var s = GetBestHypothesisSet();
            var enumerator = s.GetEnumerator();
            if (enumerator.Current == null) enumerator.MoveNext();
            return s.GetEnumerator().Current;
        }

        /// <summary>
        /// Get the highest posterior (confidence) stored in this set.
        /// </summary>
        /// <returns>The highest posterior.</returns>
        public double GetBestPosterior()
        {
            return Keys.Last();
        }

        /// <summary>
        /// Check whether this confusion set contains the given word
        /// </summary>
        /// <param name="word">The word to look for.</param>
        /// <returns>true if word is in this set</returns>
        public bool ContainsWord(String word)
        {
            return GetWordResult(word) != null;
        }

        /// <summary>
        /// Check whether this confusion set contains any fillers.
        /// </summary>
        /// <returns>Whether fillers are found.</returns>
        public bool ContainsFiller()
        {
            foreach (HashSet<WordResult> wordSet in Values)
            {
                foreach (WordResult wordResult in wordSet)
                {
                    if (wordResult.IsFiller())
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Returns the WordResult in this ConfusionSet for the given word.
        /// </summary>
        /// <param name="word">The word to look for.</param>
        /// <returns>The WordResult for the given word, or null if no WordResult for the given word is found.</returns>
        public WordResult GetWordResult(String word)
        {
            foreach (var wordSet in Values)
            {
                foreach (WordResult wordResult in wordSet)
                {
                    String resultSpelling = wordResult.GetPronunciation().Word.Spelling;
                    if (resultSpelling.Equals(word))
                    {
                        return wordResult;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Dumps out the contents of this ConfusionSet.
        /// </summary>
        /// <param name="name">The name of the confusion set.</param>
        public void Dump(String name)
        {
            Console.Write(name + @" :");
            foreach (var wordSet in Values)
            {
                foreach (WordResult wordResult in wordSet)
                {
                    Console.Write(' ' + wordResult.GetPronunciation().Word.Spelling);
                }
            }
            Console.WriteLine();
        }



        public override String ToString()
        {
            var b = new StringBuilder();
            foreach (var entry in this)
            {
                b.Append(entry.Key).Append(':');
                foreach (var wordResult in entry.Value)
                    b.Append(wordResult).Append(',');
                if (entry.Value.Count != 0)
                    b.Length = (b.Length - 1);
                b.Append(' ');
            }
            return b.ToString();
        }
    }

}
