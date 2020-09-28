using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syn.Speech.Decoders.Search.Comparator;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// Manager for pruned hypothesis
    /// </summary>
    public class AlternateHypothesisManager
    {

        private readonly HashMap<Token, List<Token>> _viterbiLoserMap = new HashMap<Token, List<Token>>();
        private readonly int _maxEdges;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateHypothesisManager"/> class.
        /// </summary>
        /// <param name="maxEdges">The maximum edges allowed.</param>
        public AlternateHypothesisManager(int maxEdges)
        {
            _maxEdges = maxEdges;
        }

        /// <summary>
        /// Collects adds alternate predecessors for a token that would have lost because of viterbi.
        /// </summary>
        /// <param name="token">A token that has an alternate lower scoring predecessor that still might be of interest.</param>
        /// <param name="predecessor">A predecessor that scores lower than token.getPredecessor()..</param>
        public void AddAlternatePredecessor(Token token, Token predecessor)
        {
            Debug.Assert(predecessor != token.Predecessor);
            List<Token> list = _viterbiLoserMap.Get(token);
            if (list == null)
            {
                list = new List<Token>();
                _viterbiLoserMap.Add(token, list);
            }
            list.Add(predecessor);
        }


        /// <summary>
        /// Returns a list of alternate predecessors for a token..
        /// </summary>
        /// <param name="token">A token that may have alternate lower scoring predecessor that still might be of interest.</param>
        /// <returns>A list of predecessors that scores lower than token.getPredecessor().</returns>
        public List<Token> GetAlternatePredecessors(Token token)
        {
            return _viterbiLoserMap.Get(token);
        }


        /// <summary>
        ///Purge all but max number of alternate preceding token hypotheses.
        /// </summary>
        public void Purge()
        {

            int max = _maxEdges - 1;

            foreach (var entry in _viterbiLoserMap.ToList())
            {
                List<Token> list = entry.Value;
                list.Sort(new ScoreableComparator());
                List<Token> newList = list.GetRange(0, list.Count > max ? max : list.Count);
                Java.Put(_viterbiLoserMap, entry.Key,newList);
            }
        }

        public bool HasAlternatePredecessors(Token token)
        {
            return _viterbiLoserMap.ContainsKey(token);
        }
    }
}
