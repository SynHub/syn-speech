using System;
using System.Collections.Generic;
using System.Linq;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Fsts.Utils;
using Syn.Speech.Helper;

//REFACTORED
namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// N-shortest paths operation.
    /// 
    /// See: M. Mohri, M. Riley,
    /// "An Efficient Algorithm for the n-best-strings problem", Proceedings of the
    /// International Conference on Spoken Language Processing 2002 (ICSLP '02).
    /// 
    /// See: M. Mohri,
    /// "Semiring Framework and Algorithms for Shortest-Distance Problems", Journal
    /// of Automata, Languages and Combinatorics, 7(3), pp. 321-350, 2002.
    /// 
    /// @author John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class NShortestPaths
    {
        private NShortestPaths() {
        }

        /**
        /// Calculates the shortest distances from each state to the final
        /// 
        /// @param fst
        ///            the fst to calculate the shortest distances
        /// @return the array containing the shortest distances
         */
        public static float[] ShortestDistance(Fst fst) 
        {
            var reversed = Reverse.Get(fst);

            var d = new float[reversed.GetNumStates()];
            var r = new float[reversed.GetNumStates()];

            var semiring = reversed.Semiring;

            Arrays.Fill(d, semiring.Zero);
            Arrays.Fill(r, semiring.Zero);

            var queue = new List<State>();//TODO: Find LinkedHashSet Implementation

            queue.Add(reversed.Start);

            d[reversed.Start.GetId()] = semiring.One;
            r[reversed.Start.GetId()] = semiring.One;

            while (!queue.IsEmpty()) 
            {
                var q = queue.First();
                queue.Remove(q);

                var rnew = r[q.GetId()];
                r[q.GetId()] = semiring.Zero;

                for (var i = 0; i < q.GetNumArcs(); i++) {
                    var a = q.GetArc(i);
                    var nextState = a.NextState;
                    var dnext = d[a.NextState.GetId()];
                    var dnextnew = semiring.Plus(dnext,
                            semiring.Times(rnew, a.Weight));
                    if (dnext != dnextnew) {
                        d[a.NextState.GetId()] = dnextnew;
                        r[a.NextState.GetId()] = semiring.Plus(r[a.NextState.GetId()], semiring.Times(rnew,
                                a.Weight));
                        if (!queue.Contains(nextState)) 
                        {
                            queue.Add(nextState);
                        }
                    }
                }
            }
            return d;
        }

        /**
        /// Calculates the n-best shortest path from the initial to the final state.
        /// 
        /// @param fst
        ///            the fst to calculate the nbest shortest paths
        /// @param n
        ///            number of best paths to return
        /// @param determinize
        ///            if true the input fst will bwe determinized prior the
        ///            operation
        /// @return an fst containing the n-best shortest paths
         */
        public static Fst Get(Fst fst, int n, Boolean determinize) 
        {
            if (fst == null) 
            {
                return null;
            }

            if (fst.Semiring == null) {
                return null;
            }
            var fstdet = fst;
            if (determinize) {
                fstdet = Determinize.Get(fst);
            }
            var semiring = fstdet.Semiring;
            var res = new Fst(semiring);
            res.Isyms = fstdet.Isyms;
            res.Osyms = fstdet.Osyms;

            var d = ShortestDistance(fstdet);

            ExtendFinal.Apply(fstdet);

            var r = new int[fstdet.GetNumStates()];

            var queue = new PriorityQueue<Pair<State, float>>(new CustomComparer(d, semiring));

            var previous = new HashMap<Pair<State, float>, Pair<State, float>>(fst.GetNumStates());
            var stateMap = new HashMap<Pair<State, float>, State>(fst.GetNumStates());

            var start = fstdet.Start;
            var item = new Pair<State, float>(start, semiring.One);
            queue.Add(item);
            previous.Put(item, null);

            while (queue.Count!=0) 
            {
                var pair = queue.Remove();
                var p = pair.GetLeft();
                var c = pair.GetRight();

                var s = new State(p.FinalWeight);
                res.AddState(s);
                stateMap.Put(pair, s);
                if (previous[pair] == null) {
                    // this is the start state
                    res.SetStart(s);
                } else {
                    // add the incoming arc from previous to current
                    var previouState = stateMap.Get(previous.Get(pair));
                    var previousOldState = previous.Get(pair).GetLeft();
                    for (var j = 0; j < previousOldState.GetNumArcs(); j++) {
                        var a = previousOldState.GetArc(j);
                        if (a.NextState.Equals(p)) 
                        {
                            previouState.AddArc(new Arc(a.Ilabel, a.Olabel, a.Weight, s));
                        }
                    }
                }

                var stateIndex = p.GetId();
                r[stateIndex]++;

                if ((r[stateIndex] == n) && (p.FinalWeight != semiring.Zero)) {
                    break;
                }

                if (r[stateIndex] <= n) {
                    for (var j = 0; j < p.GetNumArcs(); j++) {
                        var a = p.GetArc(j);
                        var cnew = semiring.Times(c, a.Weight);
                        var next = new Pair<State, float>( a.NextState, cnew);
                        previous.Put(next, pair);
                        queue.Add(next);
                    }
                }
            }

            return res;
        }
    }

    #region Extra
    public class CustomComparer : IComparer<Pair<State, float>>
    {
        private readonly float[] _distance;
        private readonly Semiring _semiring;
        public CustomComparer(float[] shortestDistance, Semiring semiring)
        {
            _distance = shortestDistance;
            _semiring = semiring;
        }
        public int Compare(Pair<State, float> o1, Pair<State, float> o2)
        {
            float previous = o1.GetRight();
            float d1 = _distance[o1.GetLeft().GetId()];

            float next = o2.GetRight();
            float d2 = _distance[o2.GetLeft().GetId()];

            float a1 = _semiring.Times(next, d2);
            float a2 = _semiring.Times(previous, d1);

            if (_semiring.NaturalLess(a1, a2))
                return 1;

            if (a1 == a2)
                return 0;

            return -1;
        }
    }

    #endregion
}
