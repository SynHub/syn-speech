using System.Collections.Generic;
//REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGrammarInterface
    {
        GrammarNode InitialNode { get; }

        HashSet<GrammarNode> GrammarNodes { get; }
    }
}
