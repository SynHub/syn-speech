//PATROLLED
namespace Syn.Speech.Alignment
{
    /// <summary>
    /// Helper class to add words and breaks into a Relation object.
    /// </summary>
    public class WordRelation
    {
        private readonly Relation relation;
        private readonly UsEnglishWordExpander tokenToWords;

        private WordRelation(Relation parentRelation, UsEnglishWordExpander tokenToWords)
        {
            relation = parentRelation;
            this.tokenToWords = tokenToWords;
        }

        /// <summary>
        /// Creates a WordRelation object with the given utterance and TokenToWords.
        /// </summary>
        /// <param name="utterance">the Utterance from which to create a Relation.</param>
        /// <param name="tokenToWords">The TokenToWords object to use.</param>
        /// <returns>a WordRelation object</returns>
        public static WordRelation createWordRelation(Utterance utterance, UsEnglishWordExpander tokenToWords)
        {
            Relation relation = utterance.createRelation(Relation.WORD);
            return new WordRelation(relation, tokenToWords);
        }

        /// <summary>
        /// Adds a break as a feature to the last item in the list.
        /// </summary>
        public virtual void addBreak()
        {
            Item wordItem = relation.getTail();
            if (wordItem != null)
            {
                FeatureSet featureSet = wordItem.getFeatures();
                featureSet.setString("break", "1");
            }
        }

        /// <summary>
        ///Adds a word as an Item to this WordRelation object.
        /// </summary>
        /// <param name="word">The word to add.</param>
        public virtual void addWord(string word)
        {
            Item tokenItem = tokenToWords.getTokenItem();
            Item wordItem = tokenItem.createDaughter();
            FeatureSet featureSet = wordItem.getFeatures();
            featureSet.setString("name", word);
            relation.appendItem(wordItem);
        }

        /// <summary>
        ///  Sets the last Item in this WordRelation to the given word.
        /// </summary>
        /// <param name="word">The word to set.</param>
        public virtual void setLastWord(string word)
        {
            Item lastItem = relation.getTail();
            FeatureSet featureSet = lastItem.getFeatures();
            featureSet.setString("name", word);
        }

        /// <summary>
        /// Returns the last item in this WordRelation.
        /// </summary>
        /// <returns>The last item</returns>
        public virtual Item getTail()
        {
            return relation.getTail();
        }
    }
}
