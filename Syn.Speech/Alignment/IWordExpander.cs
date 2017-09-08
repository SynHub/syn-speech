using System.Collections.Generic;

//PATROLLED
namespace Syn.Speech.Alignment
{
    public interface IWordExpander
    {
        List<string> expand(string text);
    }
}
