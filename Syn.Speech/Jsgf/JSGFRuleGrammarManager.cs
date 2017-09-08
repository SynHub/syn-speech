using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{
    public class JSGFRuleGrammarManager
    {
        private readonly HashMap<String, JSGFRuleGrammar> _grammars;

        public JSGFRuleGrammarManager()
        {
            _grammars = new HashMap<String, JSGFRuleGrammar>();
        }

        public ICollection<JSGFRuleGrammar> Grammars()
        {
            return _grammars.Values;
        }

        public void Remove(JSGFRuleGrammar grammar)
        {
            String name = grammar.GetName();
            _grammars.Remove(name);
        }

        public void Remove(String name)
        {
            _grammars.Remove(name);
        }

        /// <summary>
        /// Add a grammar to the grammar list.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        protected internal void StoreGrammar(JSGFRuleGrammar grammar)
        {
            _grammars.Put(grammar.GetName(), grammar);
        }

        /// <summary>
        /// Retrieve a grammar from the grammar list.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public JSGFRuleGrammar RetrieveGrammar(String name)
        {
            return _grammars.Get(name);
        }

        public void LinkGrammars()
        {
            foreach (JSGFRuleGrammar grammar in _grammars.Values)
            {
                grammar.ResolveAllRules();
            }
        }
    }
}
