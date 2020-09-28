using System;
using System.Diagnostics;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{
    public class JSGFRuleGrammarFactory
    {
        readonly JSGFRuleGrammarManager _manager;
        public JSGFRuleGrammarFactory(JSGFRuleGrammarManager manager)
        {
            _manager = manager;
        }

        public JSGFRuleGrammar NewGrammar(String name)
        {
            Debug.Assert(_manager != null);
            var grammar = new JSGFRuleGrammar(name, _manager);
            _manager.StoreGrammar(grammar);
            return grammar;
        }
    }
}
