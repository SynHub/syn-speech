using System;
using System.Collections.Generic;
using System.Text;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// An implementation of a result Path that computes scores and confidences on the fly.
    /// @author P. Gorniak
    /// </summary>
    public class WordResultPath : IPath
    {

        private readonly List<WordResult> _path = new List<WordResult>();


        /**
         * Constructs a WordResultPath with the given list of WordResults and LogMath.
         *
         * @param wordResults the list of WordResults
         */
        public WordResultPath(List<WordResult> wordResults)
        {
            _path.AddRange(wordResults);
        }


        /** Constructs an empty WordResultPath. */
        public WordResultPath()
        {
        }


        /** @see edu.cmu.sphinx.result.Path#getScore() */
        public double GetScore()
        {
            double score = LogMath.LogOne;
            foreach (WordResult wr in _path)
            {
                score += wr.Score;
            }
            return score;
        }


        /** @see edu.cmu.sphinx.result.Path#getConfidence() */
        public double GetConfidence()
        {
            double confidence = LogMath.LogOne;
            foreach (WordResult wr in _path)
            {
                confidence += wr.GetConfidence();
            }
            return confidence;
        }

        /** @see edu.cmu.sphinx.result.Path#getWords() */
        public WordResult[] GetWords()
        {
            return _path.ToArray();
        }


        /** @see edu.cmu.sphinx.result.Path#getTranscription() */
        public String GetTranscription()
        {
            StringBuilder sb = new StringBuilder();
            foreach (WordResult wr in _path)
                sb.Append(wr).Append(' ');
            return sb.ToString().Trim();
        }

        /** @see edu.cmu.sphinx.result.Path#getTranscriptionNoFiller() */
        public String GetTranscriptionNoFiller()
        {
            StringBuilder sb = new StringBuilder();
            foreach (WordResult wordResult in _path)
            {
                Word word = wordResult.GetPronunciation().Word;
                if (!word.IsFiller && !word.Spelling.Equals("<unk>"))
                {
                    sb.Append(word.Spelling).Append(' ');
                }
            }
            return sb.ToString().Trim();
        }

        public void Add(WordResult wr)
        {
            _path.Add(wr);
        }

    }

}
