using System.Collections.Generic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// ArcSort operation.
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class ArcSort
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="ArcSort"/> class from being created.
        /// </summary>
        private ArcSort()
        {
        }

        /// <summary>
        /// Applies the ArcSort on the provided fst. Sorting can be applied either on
        /// input or output label based on the provided comparator.
        /// </summary>
        /// <param name="fst">the fst to sort it's arcs.</param>
        /// <param name="cmp">The provided Comparator.</param>
        public static void Apply(Fst fst, Comparer<Arc> cmp)
        {
            int numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++)
            {
                State s = fst.GetState(i);
                s.ArcSort(cmp);
            }
        }
    }
}
