using System;
using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Fsts.Utils;
using Syn.Speech.Helper;

//REFACTORED
namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// Compose operation.
    /// See: M. Mohri, "Weighted automata algorithms", Handbook of Weighted Automata.
    /// Springer, pp. 213-250, 2009.
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Compose
    {
        /**
        /// Default Constructor
         */
        private Compose()
        {
        }

        /**
       /// Computes the composition of two Fsts. Assuming no epsilon transitions.
       /// 
       /// Input Fsts are not modified.
       /// 
       /// @param fst1 the first Fst
       /// @param fst2 the second Fst
       /// @param semiring the semiring to use in the operation
       /// @param sorted
       /// @return the composed Fst
        */
        public static Fst compose(Fst fst1, Fst fst2, Semiring semiring, Boolean sorted)
        {
            if (!Arrays.AreEqual(fst1.Osyms, fst2.Isyms))
            {
                // symboltables do not match
                return null;
            }

            Fst res = new Fst(semiring);

            Dictionary<Pair<State, State>, State> stateMap = new Dictionary<Pair<State, State>, State>();
            Queue<Pair<State, State>> queue = new Queue<Pair<State, State>>();

            State s1 = fst1.Start;
            State s2 = fst2.Start;

            if ((s1 == null) || (s2 == null))
            {
                Logger.LogInfo<Compose>("Cannot find initial state.");
                return null;
            }

            Pair<State, State> p = new Pair<State, State>(s1, s2);
            State s = new State(semiring.Times(s1.FinalWeight,
                    s2.FinalWeight));

            res.AddState(s);
            res.SetStart(s);
            if (stateMap.ContainsKey(p))
                stateMap[p] = s;
            else
                stateMap.Add(p, s);
            queue.Enqueue(p);

            while (queue.Count != 0)
            {
                p = queue.Dequeue();
                s1 = p.GetLeft();
                s2 = p.GetRight();
                s = stateMap[p];
                int numArcs1 = s1.GetNumArcs();
                int numArcs2 = s2.GetNumArcs();
                for (int i = 0; i < numArcs1; i++)
                {
                    Arc a1 = s1.GetArc(i);
                    for (int j = 0; j < numArcs2; j++)
                    {
                        Arc a2 = s2.GetArc(j);
                        if (sorted && a1.Olabel < a2.Ilabel)
                            break;
                        if (a1.Olabel == a2.Ilabel)
                        {
                            State nextState1 = a1.NextState;
                            State nextState2 = a2.NextState;
                            Pair<State, State> nextPair = new Pair<State, State>(
                                    nextState1, nextState2);
                            State nextState = stateMap.Get(nextPair);
                            if (nextState == null)
                            {
                                nextState = new State(semiring.Times(
                                        nextState1.FinalWeight,
                                        nextState2.FinalWeight));
                                res.AddState(nextState);
                                if (stateMap.ContainsKey(nextPair))
                                    stateMap[nextPair] = nextState;
                                else
                                    stateMap.Add(nextPair, nextState);

                                queue.Enqueue(nextPair);
                            }
                            Arc a = new Arc(a1.Ilabel, a2.Olabel,
                                    semiring.Times(a1.Weight, a2.Weight),
                                    nextState);
                            s.AddArc(a);
                        }
                    }
                }
            }

            res.Isyms = fst1.Isyms;
            res.Osyms = fst2.Osyms;

            return res;
        }

        /**
        /// Computes the composition of two Fsts. The two Fsts are augmented in order
        /// to avoid multiple epsilon paths in the resulting Fst
        /// 
        /// @param fst1 the first Fst
        /// @param fst2 the second Fst
        /// @param semiring the semiring to use in the operation
        /// @return the composed Fst
         */
        public static Fst Get(Fst fst1, Fst fst2, Semiring semiring)
        {
            if ((fst1 == null) || (fst2 == null))
            {
                return null;
            }

            if (!Arrays.AreEqual(fst1.Osyms, fst2.Isyms))
            {
                // symboltables do not match
                return null;
            }

            Fst filter = GetFilter(fst1.Osyms, semiring);
            Augment(1, fst1, semiring);
            Augment(0, fst2, semiring);

            Fst tmp = compose(fst1, filter, semiring, false);

            Fst res = compose(tmp, fst2, semiring, false);

            // Connect.apply(res);

            return res;
        }

        /**
        /// Get a filter to use for avoiding multiple epsilon paths in the resulting
        /// Fst
        /// 
        /// See: M. Mohri, "Weighted automata algorithms", Handbook of Weighted
        /// Automata. Springer, pp. 213-250, 2009.
        /// 
        /// @param syms the gilter's input/output symbols
        /// @param semiring the semiring to use in the operation
        /// @return the filter
         */
        public static Fst GetFilter(String[] syms, Semiring semiring)
        {
            Fst filter = new Fst(semiring);

            if (syms == null)
                return filter; //empty one

            int e1Index = syms == null ? 0 : syms.Length;
            int e2Index = syms == null ? 1 : syms.Length + 1;

            filter.Isyms = syms;
            filter.Osyms = syms;

            // State 0
            State s0 = new State(syms.Length + 3);
            s0.FinalWeight = semiring.One;
            State s1 = new State(syms.Length);
            s1.FinalWeight = semiring.One;
            State s2 = new State(syms.Length);
            s2.FinalWeight = semiring.One;
            filter.AddState(s0);
            s0.AddArc(new Arc(e2Index, e1Index, semiring.One, s0));
            s0.AddArc(new Arc(e1Index, e1Index, semiring.One, s1));
            s0.AddArc(new Arc(e2Index, e2Index, semiring.One, s2));
            for (int i = 1; i < syms.Length; i++)
            {
                s0.AddArc(new Arc(i, i, semiring.One, s0));
            }
            filter.SetStart(s0);

            // State 1
            filter.AddState(s1);
            s1.AddArc(new Arc(e1Index, e1Index, semiring.One, s1));
            for (int i = 1; i < syms.Length; i++)
            {
                s1.AddArc(new Arc(i, i, semiring.One, s0));
            }

            // State 2
            filter.AddState(s2);
            s2.AddArc(new Arc(e2Index, e2Index, semiring.One, s2));
            for (int i = 1; i < syms.Length; i++)
            {
                s2.AddArc(new Arc(i, i, semiring.One, s0));
            }

            return filter;
        }

        /**
        /// Augments the labels of an Fst in order to use it for composition avoiding
        /// multiple epsilon paths in the resulting Fst
        /// 
        /// Augment can be applied to both {@link edu.cmu.sphinx.fst.Fst} and
        /// {@link edu.cmu.sphinx.fst.ImmutableFst}, as immutable fsts hold an
        /// additional null arc for that operation
        /// 
        /// @param label constant denoting if the augment should take place on input
        ///            or output labels For value equal to 0 augment will take place
        ///            for input labels For value equal to 1 augment will take place
        ///            for output labels
        /// @param fst the fst to augment
        /// @param semiring the semiring to use in the operation
         */
        public static void Augment(int label, Fst fst, Semiring semiring)
        {
            // label: 0->augment on ilabel
            // 1->augment on olabel

            String[] isyms = fst.Isyms;
            String[] osyms = fst.Osyms;

            int e1InputIndex = isyms == null ? 0 : isyms.Length;
            int e2InputIndex = isyms == null ? 1 : isyms.Length + 1;

            int e1OutputIndex = osyms == null ? 0 : osyms.Length;
            int e2OutputIndex = osyms == null ? 1 : osyms.Length + 1;

            int numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++)
            {
                State s = fst.GetState(i);
                // Immutable fsts hold an additional (null) arc for augmention
                int numArcs = (fst is ImmutableFst) ? s.GetNumArcs() - 1
                        : s.GetNumArcs();
                for (int j = 0; j < numArcs; j++)
                {
                    Arc a = s.GetArc(j);
                    if ((label == 1) && (a.Olabel == 0))
                    {
                        a.Olabel = e2OutputIndex;
                    }
                    else if ((label == 0) && (a.Ilabel == 0))
                    {
                        a.Ilabel = e1InputIndex;
                    }
                }
                if (label == 0)
                {
                    if (fst is ImmutableFst)
                    {
                        s.SetArc(numArcs, new Arc(e2InputIndex, 0, semiring.One,
                                s));
                    }
                    else
                    {
                        s.AddArc(new Arc(e2InputIndex, 0, semiring.One, s));
                    }
                }
                else if (label == 1)
                {
                    if (fst is ImmutableFst)
                    {
                        s.SetArc(numArcs, new Arc(0, e1OutputIndex, semiring.One,
                                s));
                    }
                    else
                    {
                        s.AddArc(new Arc(0, e1OutputIndex, semiring.One, s));
                    }
                }
            }
        }

    }
}
