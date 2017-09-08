//REFACTORED
namespace Syn.Speech.Fsts.Operations
{
    public enum ProjectType
    {
        Input, Output
    }

    /// <summary>
    /// Project operation. 
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Project
    {
        /**
        /// Default Constructor
         */
        private Project()
        {
        }

        /**
        /// Projects an fst onto its domain or range by either copying each arc's
        /// input label to its output label or vice versa.
        /// 
        /// 
        /// @param fst
        /// @param pType
         */
        public static void Apply(Fst fst, ProjectType pType) 
        {
            if (pType == ProjectType.Input) {
                fst.Osyms = fst.Isyms;
            } else if (pType == ProjectType.Output) {
                fst.Isyms = fst.Osyms;
            }

            int numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++) 
            {
                State s = fst.GetState(i);
                // Immutable fsts hold an additional (null) arc
                int numArcs = (fst is ImmutableFst) ? s.GetNumArcs() - 1: s
                        .GetNumArcs();
                    for (int j = 0; j < numArcs; j++) {
                        Arc a = s.GetArc(j);
                        if (pType == ProjectType.Input) {
                            a.Olabel = a.Ilabel;
                        } else if (pType == ProjectType.Output) {
                            a.Ilabel = a.Olabel;
                        }
                    }
            }
        }
    }
}
