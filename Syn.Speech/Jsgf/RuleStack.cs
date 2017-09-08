using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{
    /// <summary>
    /// Manages a stack of grammar graphs that can be accessed by grammar name.
    /// </summary>
    public class RuleStack {

        private List<string> _stack;
        private HashMap<string, GrammarGraph> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleStack"/> class and creates a name stack
        /// </summary>
        public RuleStack() {
            Clear();
        }

        /// <summary>
        /// Pushes the grammar graph on the stack
        /// </summary>
        public void Push(String name, GrammarGraph g) {
            _stack.Insert(0, name);
            _map.Put(name, g);
        }

        /// <summary>
        /// remove the top graph on the stack.
        /// </summary>
        public void Pop()
        {
            _map.Remove(_stack.Remove(0));
        }

        /// <summary>
        /// Checks to see if the stack contains a graph with the given name.
        /// </summary>
        /// <param name="name">The graph name.</param>
        /// <returns>The grammar graph associated with the name if found, otherwise null</returns>
        public GrammarGraph Contains(String name)
        {
            if (_stack.Contains(name)) {
                return _map.Get(name);
            }
            return null;
        }

        /// <summary>
        /// Clears this name stack.
        /// </summary>
        public void Clear() {
            _stack = new List<String>();
            _map = new HashMap<String, GrammarGraph>();
        }
    }
}