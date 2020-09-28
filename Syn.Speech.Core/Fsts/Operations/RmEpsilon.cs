using System.Collections.Generic;
//REFACTORED
using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// Remove epsilon operation.
    /// @author John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class RmEpsilon
    {
        private RmEpsilon() {
        }

        /**
        /// Put a new state in the epsilon closure
         */
        private static void Put(State fromState, State toState, float weight, Dictionary<State, float>[] cl) 
        {
            var tmp = cl[fromState.GetId()];
            if (tmp == null) {
                tmp = new Dictionary<State, float>();
                cl[fromState.GetId()] = tmp;
            }
            tmp.Add(toState, weight);
        }

        /**
        /// Add a state in the epsilon closure
         */
        private static void Add(State fromState, State toState, float weight, Dictionary<State, float>[] cl, Semiring semiring) 
        {
            var old = GetPathWeight(fromState, toState, cl);
            if (old == float.NaN) {
                Put(fromState, toState, weight, cl);
            } else {
                Put(fromState, toState, semiring.Plus(weight, old), cl);
            }

        }

        /**
        /// Calculate the epsilon closure
         */
        private static void CalcClosure(Fst fst, State state, Dictionary<State, float>[] cl, Semiring semiring) 
        {
            var s = state;

            float pathWeight;
            var numArcs = s.GetNumArcs();
            for (var j = 0; j < numArcs; j++) {
                var a = s.GetArc(j);
                if ((a.Ilabel == 0) && (a.Olabel == 0)) {
                    if (cl[a.NextState.GetId()] == null) {
                        CalcClosure(fst, a.NextState, cl, semiring);
                    }
                    if (cl[a.NextState.GetId()] != null) 
                    {
                        foreach (var pathFinalState in cl[a.NextState.GetId()].Keys) 
                        {
                            pathWeight = semiring.Times(
                                    GetPathWeight(a.NextState, pathFinalState,
                                            cl), a.Weight);
                            Add(state, pathFinalState, pathWeight, cl, semiring);
                        }
                    }
                    Add(state, a.NextState, a.Weight, cl, semiring);
                }
            }
        }

        /**
        /// Get an epsilon path's cost in epsilon closure
         */
        public static float GetPathWeight(State _in, State _out, Dictionary<State, float>[] cl) 
        {
            if (cl[_in.GetId()] != null) 
            {
                return cl[_in.GetId()][_out];
            }

            return float.NaN;
        }

        /**
        /// Removes epsilon transitions from an fst.
        /// 
        /// It return a new epsilon-free fst and does not modify the original fst
        /// 
        /// @param fst the fst to remove epsilon transitions from
        /// @return the epsilon-free fst
         */
        public static Fst Get(Fst fst) {
            if (fst == null) {
                return null;
            }

            if (fst.Semiring == null) {
                return null;
            }

            var semiring = fst.Semiring;

            var res = new Fst(semiring);

            var cl = new Dictionary<State, float>[fst.GetNumStates()];
            var oldToNewStateMap = new State[fst.GetNumStates()];
            var newToOldStateMap = new State[fst.GetNumStates()];

            var numStates = fst.GetNumStates();
            for (var i = 0; i < numStates; i++) {
                var s = fst.GetState(i);
                // Add non-epsilon arcs
                var newState = new State(s.FinalWeight);
                res.AddState(newState);
                oldToNewStateMap[s.GetId()] = newState;
                newToOldStateMap[newState.GetId()] = s;
                if (newState.GetId() == fst.Start.GetId()) {
                    res.SetStart(newState);
                }
            }

            for (var i = 0; i < numStates; i++) {
                var s = fst.GetState(i);
                // Add non-epsilon arcs
                var newState = oldToNewStateMap[s.GetId()];
                var numArcs = s.GetNumArcs();
                for (var j = 0; j < numArcs; j++) {
                    var a = s.GetArc(j);
                    if ((a.Ilabel != 0) || (a.Olabel != 0)) {
                        newState.AddArc(new Arc(a.Ilabel, a.Olabel, a.Weight, oldToNewStateMap[a.NextState
                                .GetId()]));
                    }
                }

                // Compute e-Closure
                if (cl[s.GetId()] == null) {
                    CalcClosure(fst, s, cl, semiring);
                }
            }

            // augment fst with arcs generated from epsilon moves.
            numStates = res.GetNumStates();
            for (var i = 0; i < numStates; i++) {
                var s = res.GetState(i);
                var oldState = newToOldStateMap[s.GetId()];
                if (cl[oldState.GetId()] != null) {
                    foreach (var pathFinalState in cl[oldState.GetId()].Keys) {
                        var s1 = pathFinalState;
                        if (s1.FinalWeight != semiring.Zero) {
                            s.FinalWeight = semiring.Plus(s.FinalWeight,
                                semiring.Times(GetPathWeight(oldState, s1, cl),
                                    s1.FinalWeight));
                        }
                        var numArcs = s1.GetNumArcs();
                        for (var j = 0; j < numArcs; j++) {
                            var a = s1.GetArc(j);
                            if ((a.Ilabel != 0) || (a.Olabel != 0)) {
                                var newArc = new Arc(a.Ilabel, a.Olabel,
                                        semiring.Times(a.Weight,
                                                GetPathWeight(oldState, s1, cl)),
                                        oldToNewStateMap[a.NextState.GetId()]);
                                s.AddArc(newArc);
                            }
                        }
                    }
                }
            }

            res.Isyms = fst.Isyms;
            res.Osyms = fst.Osyms;

            Connect.Apply(res);

            return res;
        }
    }
}
