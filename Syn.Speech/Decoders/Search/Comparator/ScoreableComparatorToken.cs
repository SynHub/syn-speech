using System.Collections.Generic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search.Comparator
{
    public class ScoreableComparatorToken : IComparer<Token>
    {
        public int Compare(Token t1, Token t2)
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