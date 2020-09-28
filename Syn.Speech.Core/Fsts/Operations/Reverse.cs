//REFACTORED

using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    ///  Reverse operation.
    /// @author John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class Reverse
    {
        /**
       /// Default Constructor
        */
        private Reverse()
        {
        }

        /**
        /// Reverses an fst
        /// 
        /// @param fst the fst to reverse
        /// @return the reversed fst
         */
        public static Fst Get(Fst fst) 
        {
            if (fst.Semiring == null) 
            {
                return null;
            }

            ExtendFinal.Apply(fst);

            Semiring semiring = fst.Semiring;

            Fst res = new Fst(fst.GetNumStates());
            res.Semiring = semiring;

            res.Isyms = fst.Osyms;
            res.Osyms = fst.Isyms;

            State[] stateMap = new State[fst.GetNumStates()];
            int numStates = fst.GetNumStates();
            for (int i=0; i<numStates; i++) 
            {
                State _is = fst.GetState(i);
                State s = new State(semiring.Zero);
                res.AddState(s);
                stateMap[_is.GetId()] = s;
                if (_is.FinalWeight != semiring.Zero) {
                    res.SetStart(s);
                }
            }

            stateMap[fst.Start.GetId()].FinalWeight = semiring.One;

            for (int i=0; i<numStates; i++) {
                State olds = fst.GetState(i);
                State news = stateMap[olds.GetId()];
                int numArcs = olds.GetNumArcs();
                for (int j = 0; j < numArcs; j++) {
                    Arc olda = olds.GetArc(j);
                    State next = stateMap[olda.NextState.GetId()];
                    Arc newa = new Arc(olda.Ilabel, olda.Olabel,
                            semiring.Reverse(olda.Weight), news);
                    next.AddArc(newa);
                }
            }

            ExtendFinal.Undo(fst);
            return res;
        }
    }
}
