using System.Collections.Generic;
//REFACTORED
namespace Syn.Speech.Linguist.G2p
{
    /// <summary>
    /// Comparator for {@link edu.cmu.sphinx.linguist.g2p.Path} object based on its cost
    /// @author John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class PathComparator : IComparer<Path>
    {
        int IComparer<Path>.Compare(Path o1, Path o2)
        {
            if (o1.Cost < o2.Cost)
                return -1;
            else if (o1.Cost > o2.Cost)
                return 1;

            return 0;
        }
    }
}
