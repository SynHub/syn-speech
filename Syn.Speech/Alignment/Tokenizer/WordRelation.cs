//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    /// <summary>
    /// Helper class to add words and breaks into a Relation object.
    /// </summary>
    public class WordRelation
    {
        private readonly Relation _relation;
        private readonly UsEnglishTokenizer _tokenToWords;

        private WordRelation(Relation parentRelation, UsEnglishTokenizer tokenToWords)
        {
            _relation = parentRelation;
            _tokenToWords = tokenToWords;
        }

        /// <summary>
        /// Creates a WordRelation object with the given utterance and TokenToWords.
        /// </summary>
        /// <param name="utterance">the Utterance from which to create a Relation.</param>
        /// <param name="tokenToWords">The TokenToWords object to use.</param>
        /// <returns>a WordRelation object</returns>
        public static WordRelation CreateWordRelation(Utterance utterance, UsEnglishTokenizer tokenToWords)
        {
            var relation = utterance.CreateRelation(Relation.Word);
            return new WordRelation(relation, tokenToWords);
        }

        /// <summary>
        /// Adds a break as a feature to the last item in the list.
        /// </summary>
        public virtual void AddBreak()
        {
            var wordItem = _relation.Tail;
            if (wordItem != null)
            {
                var featureSet = wordItem.Features;
                featureSet.SetString("break", "1");
            }
        }

        /// <summary>
        ///Adds a word as an Item to this WordRelation object.
        /// </summary>
        /// <param name="word">The word to add.</param>
        public virtual void AddWord(string word)
        {
            var tokenItem = _tokenToWords.GetTokenItem();
            var wordItem = tokenItem.CreateDaughter();
            var featureSet = wordItem.Features;
            featureSet.SetString("name", word);
            _relation.AppendItem(wordItem);
        }

        /// <summary>
        ///  Sets the last Item in this WordRelation to the given word.
        /// </summary>
        /// <param name="word">The word to set.</param>
        public virtual void SetLastWord(string word)
        {
            var lastItem = _relation.Tail;
            var featureSet = lastItem.Features;
            featureSet.SetString("name", word);
        }

        /// <summary>
        /// Returns the last item in this WordRelation.
        /// </summary>
        /// <value>The last item</value>
        public virtual Item Tail
        {
            get { return _relation.Tail; }
        }
    }
}
