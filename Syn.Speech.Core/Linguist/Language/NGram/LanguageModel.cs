using System.Collections.Generic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.NGram
{

    /**
     * Represents the generic interface to an N-Gram language model.
     * <p/>
     * Note that all probabilities are in LogMath log base, except as otherwise
     * noted.
     */

    public abstract class LanguageModel : IConfigurable
    {

        /** The property specifying the location of the language model. */
        [S4String(DefaultValue = ".")]
        public static string PropLocation = "location";

        /** The property specifying the unigram weight */
        [S4Double(DefaultValue = 1.0)]
        public static string PropUnigramWeight = "unigramWeight";
        /**
         * The property specifying the maximum depth reported by the language model
         * (from a getMaxDepth()) call. If this property is set to (-1) (the
         * default) the language model reports the implicit depth of the model.
         * This property allows a deeper language model to be used. For instance, a
         * trigram language model could be used as a bigram model by setting this
         * property to 2. Note if this property is set to a value greater than the
         * implicit depth, the implicit depth is used. Legal values for this
         * property are 1..N and -1.
         */
        [S4Integer(DefaultValue = -1)]
        public static string PropMaxDepth = "maxDepth";

        /** The property specifying the dictionary to use */
        [S4Component(Type = typeof(IDictionary))]
        public static string PropDictionary = "dictionary";

        /**
         * Create the language model
         *
         * @throws java.io.IOException
         */
        public abstract void Allocate();

        /**
         * Deallocate resources allocated to this language model
         *
         * @throws IOException
         */
        public abstract void Deallocate();

        /**
         * Gets the n-gram probability of the word sequence represented by the word
         * list
         *
         * @param wordSequence the wordSequence
         * @return the probability of the word sequence in LogMath log base
         */
        public abstract float GetProbability(WordSequence wordSequence);

        /**
         * Gets the smear term for the given wordSequence. Used in
         * {@link LexTreeLinguist}. See
         * {@link LexTreeLinguist#PROP_WANT_UNIGRAM_SMEAR} for details.
         *
         * @param wordSequence the word sequence
         * @return the smear term associated with this word sequence
         */
        public abstract float GetSmear(WordSequence wordSequence);

        /**
         * Returns the set of words in the language model. The set is unmodifiable.
         *
         * @return the unmodifiable set of words
         */
        public abstract HashSet<string> Vocabulary { get; }

        /**
         * Returns the maximum depth of the language model
         *
         * @return the maximum depth of the language model
         */
        public abstract int MaxDepth { get; set; }


        public abstract void NewProperties(PropertySheet ps);
    }
}
