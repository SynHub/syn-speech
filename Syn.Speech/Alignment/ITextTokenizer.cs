using System.Collections.Generic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment
{
    public interface ITextTokenizer
    {
        List<string> Expand(string text);
    }
}
