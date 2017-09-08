using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Common;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// Manager for pruned hypothesis
    /// </summary>
    public class AlternateHypothesisManager
    {

        private Dictionary<Token, List<Token>> viterbiLoserMap = new Dictionary<Token, List<Token>>();
        private int maxEdges;


        /**
         * Creates an alternate hypotheses manager
         *
         * @param maxEdges the maximum edges allowed
         */
        public AlternateHypothesisManager(int maxEdges)
        {
            this.maxEdges = maxEdges;
        }


        /**
         * Collects adds alternate predecessors for a token that would have lost because of viterbi.
         *
         * @param token       - a token that has an alternate lower scoring predecessor that still might be of interest
         * @param predecessor - a predecessor that scores lower than token.getPredecessor().
         */

        public void addAlternatePredecessor(Token token, Token predecessor)
        {
            Trace.Assert(predecessor != token.getPredecessor());
            List<Token> list = viterbiLoserMap[token];
            if (list == null)
            {
                list = new List<Token>();
                viterbiLoserMap.Add(token, list);
            }
            list.Add(predecessor);
        }


        /**
         * Returns a list of alternate predecessors for a token.
         *
         * @param token - a token that may have alternate lower scoring predecessor that still might be of interest
         * @return A list of predecessors that scores lower than token.getPredecessor().
         */
        public List<Token> getAlternatePredecessors(Token token)
        {
            return viterbiLoserMap[token];
        }


        /** Purge all but max number of alternate preceding token hypotheses. */
        public void purge()
        {

            int max = maxEdges - 1;

            foreach (var entry in viterbiLoserMap)
            {
                List<Token> list = entry.Value;
                list.Sort(new ScoreableComparator());
                List<Token> newList = list.GetRange(0, list.Count > max ? max : list.Count);
                viterbiLoserMap.Add(entry.Key, newList);
            }
        }



        public bool hasAlternatePredecessors(Token token)
        {
            return viterbiLoserMap.ContainsKey(token);
        }
    }

}
