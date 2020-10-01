using System;
using System.Collections.Generic;
using Syn.Speech.Alignment.Comparer;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment
{
    public class Alignment
    {
        internal readonly List<Integer> Shifts;
        internal readonly List<string> Query;
        public readonly List<Integer> Indices;
        private readonly List<Node> _alignment;
        private LongTextAligner _aligner;

        public Alignment(LongTextAligner longTextAligner, List<string> query, Helper.Range range)
        {
            _aligner = longTextAligner;
            Query = query;
            Indices = new List<Integer>();
            var shiftSet = new SortedSet<Integer>();
            for (var i = 0; i < query.Count; i++)
            {
                if (_aligner.TupleIndex.ContainsKey(query[i]))
                {
                    Indices.Add(i);
                    foreach (var shift in _aligner.TupleIndex.Get(query[i]))
                    {
                        if (range.contains(shift))
                            shiftSet.Add(shift);
                    }
                }
            }

            Shifts = new List<Integer>(shiftSet);

            var cost = new HashMap<Node, Integer>();
            var openSet = new PriorityQueue<Node>(1, new NodeComparer(cost));
            var closedSet = new HashSet<Node>();
            var parents = new HashMap<Node, Node>();
            var startNode = new Node(_aligner, this, 0, 0);
            cost.Put(startNode, 0);
            openSet.Add(startNode);

            while (openSet.Count !=0)
            {
                Node q = openSet.Dequeue();
                if (closedSet.Contains(q))
                    continue;

                if (q.IsTarget) {
                    var backtrace = new List<Node>();
                    while (parents.ContainsKey(q)) {
                        if (!q.IsBoundary && q.HasMatch)
                            backtrace.Add(q);
                        q = parents.Get(q);
                    }
                    _alignment = new List<Node>(backtrace);
                    _alignment.Reverse();
                    //Collections.reverse(alignment);
                    return;
                }

                closedSet.Add(q);
                foreach (Node nb in q.Adjacent()) {

                    if (closedSet.Contains(nb))
                        continue;

                    // FIXME: move to appropriate location
                    int l = Math.Abs(Indices.Count - Shifts.Count - q.QueryIndex +
                                q.DatabaseIndex) -
                            Math.Abs(Indices.Count - Shifts.Count -
                                nb.QueryIndex +
                                nb.DatabaseIndex);


                    Integer oldScore = cost.Get(nb);
                    Integer qScore = cost.Get(q);
                    if (oldScore == null)
                        oldScore = Integer.MAX_VALUE;
                    if (qScore == null)
                        qScore = Integer.MAX_VALUE;

                    int newScore = qScore + nb.GetValue() - l;
                    if (newScore < oldScore) {
                        cost.Put(nb, newScore);
                        openSet.Add(nb);
                        parents.Put(nb, q);
                
                    }
                }
            }

            _alignment = new List<Node>();
        }

        public List<Node> GetIndices()
        {
            return _alignment;
        }
    }
}
