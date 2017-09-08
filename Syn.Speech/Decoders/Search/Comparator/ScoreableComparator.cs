using System.Collections.Generic;
using Syn.Speech.Decoders.Scorer;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search.Comparator
{
    public class ScoreableComparator : IComparer<IScoreable>
    {
        public int Compare(IScoreable t1, IScoreable t2)
        {
            if (t1.Score > t2.Score)
            {
                return -1;
            }
            else if (t1.Score == t2.Score)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
