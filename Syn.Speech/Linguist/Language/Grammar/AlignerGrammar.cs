using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Logging;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    public class AlignerGrammar : Grammar
    {
        protected GrammarNode FinalNode;
        private readonly List<string> _tokens = new List<string>();

        public AlignerGrammar(bool showGrammar, bool optimizeGrammar, bool addSilenceWords, bool addFillerWords,
            IDictionary dictionary)
            : base(showGrammar, optimizeGrammar, addSilenceWords, addFillerWords, dictionary)
        {

        }

        public AlignerGrammar()
        {

        }

        /// <summary>
        /// Reads Text and converts it into a list of tokens
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetText(string text)
        {
            SetWords(text.Split(' '));
        }

        public void SetWords(IEnumerable<string> words)
        {
            _tokens.Clear();
            foreach (string word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    _tokens.Add(word);
                }
            }
            CreateGrammar();
            PostProcessGrammar();
        }


        protected override GrammarNode CreateGrammar()
        {
            this.LogInfo("Creating Grammar");
            InitialNode = CreateGrammarNode(IDictionary.SilenceSpelling);
            FinalNode = CreateGrammarNode(true);

            GrammarNode prevNode = InitialNode;
            foreach (string word in _tokens)
            {
                var wordNode = CreateGrammarNode(word);
                var alternateNode = CreateGrammarNode(false);
                var exitNode = CreateGrammarNode(false);
                prevNode.Add(wordNode, LogMath.LogOne);
                prevNode.Add(alternateNode, LogMath.LogOne);
                wordNode.Add(exitNode, LogMath.LogOne);
                alternateNode.Add(exitNode, LogMath.LogOne);
                prevNode = exitNode;
            }
            InitialNode.Add(prevNode, LogMath.LogOne);

            this.LogInfo("Done making Grammar");
            return InitialNode;
        }

    }
}
