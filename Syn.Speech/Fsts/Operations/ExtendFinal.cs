using System.Collections.Generic;
using Syn.Logging;

//REFACTORED
using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// Extend an Fst to a single final state and undo operations.
    /// @author John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class ExtendFinal
    {
        /**
        /// Default Contructor
        */
        private ExtendFinal()
        {
        }

        /**
        /// Extends an Fst to a single final state.
        /// 
        /// It adds a new final state with a 0.0 (Semiring's 1) final wight and
        /// connects the current final states to it using epsilon transitions with
        /// weight equal to the original final state's weight.
        /// 
        /// @param fst the Fst to extend
         */
        public static void Apply(Fst fst) 
        {
            Semiring semiring = fst.Semiring;
            List<State> fStates = new List<State>();

            int numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++) {
                State s = fst.GetState(i);
                if (s.FinalWeight != semiring.Zero) {
                    fStates.Add(s);
                }
            }

            // Add a new single final
            State newFinal = new State(semiring.One);
            fst.AddState(newFinal);
            foreach (State s in fStates) 
            {
                // add epsilon transition from the old final to the new one
                s.AddArc(new Arc(0, 0, s.FinalWeight, newFinal));
                // set old state's weight to zero
                s.FinalWeight = semiring.Zero;
            }
        }

        /**
        /// Undo of the extend operation
         */
        public static void Undo(Fst fst)
        {
            State f = null;
            int numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++)
            {
                State s = fst.GetState(i);
                if (s.FinalWeight != fst.Semiring.Zero)
                {
                    f = s;
                    break;
                }
            }

            if (f == null)
            {
                Logger.LogInfo<ExtendFinal>("Final state not found.");
                return;
            }
            for (int i = 0; i < numStates; i++)
            {
                State s = fst.GetState(i);
                for (int j = 0; j < s.GetNumArcs(); j++)
                {
                    Arc a = s.GetArc(j);
                    if (a.Ilabel == 0 && a.Olabel == 0
                            && a.NextState.GetId() == f.GetId())
                    {
                        s.FinalWeight = a.Weight;
                    }
                }
            }
            fst.DeleteState(f);
        }

    }
}
