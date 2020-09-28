using System.Collections.Generic;
//REFACTORED
namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// Comparator used in {@link edu.cmu.sphinx.fst.operations.ArcSort} for sorting
    /// based on input labels
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class ILabelCompare : Comparer<Arc> 
    {
        public override int Compare(Arc o1, Arc o2)
        {
            if (o1 == null)
            {
                return 1;
            }
            if (o2 == null)
            {
                return -1;
            }

            return (o1.Ilabel < o2.Ilabel) ? -1 : ((o1.Ilabel == o2.Ilabel) ? 0 : 1);
        }
    }
}
