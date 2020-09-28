using System;
using System.Collections.Generic;
using System.Text;

//REFACTORED
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Fsts.Utils;
using Syn.Speech.Helper;

namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// Determize operation.
    /// 
    /// See: M. Mohri, "Finite-State Transducers in Language and Speech Processing",
    /// Computational Linguistics, 23:2, 1997.
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Determinize
    {
        /**
       /// Default constructor
        */
        private Determinize()
        {

        }

        private static Pair<State, float> GetPair( List<Pair<State, float>> queue, State state, float zero) 
        {
            Pair<State, float> res = null;
            foreach (Pair<State, float> tmp in queue) 
            {
                if (state.GetId() == tmp.GetLeft().GetId()) 
                {
                    res = tmp;
                    break;
                }
            }

            if (res == null) 
            {
                res = new Pair<State, float>(state, zero);
                queue.Add(res);
            }

            return res;
        }

        private static List<int> GetUniqueLabels(Fst fst, List<Pair<State, float>> pa) 
        {
            List<int> res = new List<int>();

            foreach (Pair<State, float> p in pa) 
            {
                State s = p.GetLeft();

                int numArcs = s.GetNumArcs();
                for (int j = 0; j < numArcs; j++) {
                    Arc arc = s.GetArc(j);
                    if (!res.Contains(arc.Ilabel)) 
                    {
                        res.Add(arc.Ilabel);
                    }
                }
            }
            return res;
        }

        private static State GetStateLabel(List<Pair<State, float>> pa, Dictionary<String, State> stateMapper) 
        {
            StringBuilder sb = new StringBuilder();

            foreach (Pair<State, float> p in pa) 
            {
                if (sb.Length > 0) 
                {
                    sb.Append(",");
                }
                sb.Append("(" + p.GetLeft() + "," + p.GetRight() + ")");
            }
            return stateMapper[sb.ToString()];
        }

        /**
        /// Determinizes an fst. The result will be an equivalent fst that has the
        /// property that no state has two transitions with the same input label. For
        /// this algorithm, epsilon transitions are treated as regular symbols.
        /// 
        /// @param fst the fst to determinize
        /// @return the determinized fst
         */
        public static Fst Get(Fst fst) 
        {

            if (fst.Semiring == null) {
                // semiring not provided
                return null;
            }

            // initialize the queue and new fst
            Semiring semiring = fst.Semiring;
            Fst res = new Fst(semiring);
            res.Isyms = fst.Isyms;
            res.Osyms = fst.Osyms;

            // stores the queue (item in index 0 is next)
            Queue<List<Pair<State, float>>> queue = new Queue<List<Pair<State, float>>>();

            Dictionary<String, State> stateMapper = new Dictionary<String, State>();

            State s = new State(semiring.Zero);
            string stateString = "(" + fst.Start + "," + semiring.One + ")";
            queue.Enqueue(new List<Pair<State, float>>());
            queue.Peek().Add(new Pair<State, float>(fst.Start, semiring.One));
            res.AddState(s);
            stateMapper.Add(stateString, s);
            res.SetStart(s);

            while (queue.Count!=0) 
            {
                List<Pair<State, float>> p = queue.Dequeue();
                State pnew = GetStateLabel(p, stateMapper);
                //queueRemoveAt(0);
                List<int> labels = GetUniqueLabels(fst, p);
                foreach (int label in labels) 
                {
                    float wnew = semiring.Zero;
                    // calc w'
                    foreach (Pair<State, float> ps in p) 
                    {
                        State old = ps.GetLeft();
                        float u = ps.GetRight();
                        int numArcs = old.GetNumArcs();
                        for (int j = 0; j < numArcs; j++) {
                            Arc arc = old.GetArc(j);
                            if (label == arc.Ilabel) {
                                wnew = semiring.Plus(wnew,
                                        semiring.Times(u, arc.Weight));
                            }
                        }
                    }

                    // calc new states
                    // keep residual weights to variable forQueue
                    List<Pair<State, float>> forQueue = new List<Pair<State, float>>();
                    foreach (Pair<State, float> ps in p) 
                    {
                        State old = ps.GetLeft();
                        float u = ps.GetRight();
                        float wnewRevert = semiring.Divide(semiring.One, wnew);
                        int numArcs = old.GetNumArcs();
                        for (int j = 0; j < numArcs; j++) {
                            Arc arc = old.GetArc(j);
                            if (label == arc.Ilabel) {
                                State oldstate = arc.NextState;
                                Pair<State, float> pair = GetPair(forQueue,
                                        oldstate, semiring.Zero);
                                pair.SetRight(semiring.Plus(
                                        pair.GetRight(),
                                        semiring.Times(wnewRevert,
                                                semiring.Times(u, arc.Weight))));
                            }
                        }
                    }

                    // build new state's id and new elements for queue
                    string qnewid = "";
                    foreach (Pair<State, float> ps in forQueue) 
                    {
                        State old = ps.GetLeft();
                        float unew = ps.GetRight();
                        if (!qnewid.Equals("")) {
                            qnewid = qnewid + ",";
                        }
                        qnewid = qnewid + "(" + old + "," + unew + ")";
                    }

                    if (stateMapper.Get(qnewid) == null) 
                    {
                        State qnew = new State(semiring.Zero);
                        res.AddState(qnew);
                        stateMapper.Add(qnewid, qnew);
                        // update new state's weight
                        float fw = qnew.FinalWeight;
                        foreach (Pair<State, float> ps in forQueue) 
                        {
                            fw = semiring.Plus(fw, semiring.Times(ps.GetLeft().FinalWeight, ps.GetRight()));
                        }
                        qnew.FinalWeight = fw;

                        queue.Enqueue(forQueue);
                    }
                    pnew.AddArc(new Arc(label, label, wnew, stateMapper[qnewid]));
                }
            }

            return res;
        }
    }
}
