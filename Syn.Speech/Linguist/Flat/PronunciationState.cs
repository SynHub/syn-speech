using System;
using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a pronunciation in an SentenceHMMS
    /// </summary>
    public class PronunciationState:  SentenceHMMState,IWordSearchState
    {
        /**
        /// Creates a PronunciationState
         *
        /// @param parent the parent word of the current pronunciation
        /// @param which  the pronunciation of interest
         */
        public PronunciationState(WordState parent, int which) 
        :base("P", parent, which)
        {
            Pronunciation = parent.GetWord().GetPronunciations(null)[which];
        }


        /**
        /// Creates a PronunciationState
         *
        /// @param name  the name of the pronunciation associated with this state
        /// @param p     the pronunciation
        /// @param which the index for the pronunciation
         */
        public PronunciationState(String name, Pronunciation p, int which) 
            :base(name, null, which)
        {
            Pronunciation = p;
        }


        /**
        /// Gets the pronunciation associated with this state
         *
        /// @return the pronunciation
         */

        public Pronunciation Pronunciation { get; private set; }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public override string TypeLabel
        {
            get { return "Pron"; }
        }


        /**
        /// Returns the state order for this state type
         *
        /// @return the state order
         */

        public override int Order
        {
            get { return 4; }
        }


        /**
        /// Returns true if this PronunciationState indicates the start of a word. Returns false if this PronunciationState
        /// indicates the end of a word.
         *
        /// @return true if this PronunciationState indicates the start of a word, false if this PronunciationState indicates
        ///         the end of a word
         */
        public override Boolean IsWordStart() 
        {
            return true;
        }
    }
}
