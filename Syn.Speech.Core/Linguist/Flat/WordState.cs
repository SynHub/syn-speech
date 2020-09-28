using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a word in an SentenceHMMS
    /// </summary>
    public class WordState : SentenceHMMState
    {
        /** Creates a WordState
        /// @param which*/
        public WordState(AlternativeState parent, int which) 
            :base("W", parent, which)
        {
            
        }


        /**
        /// Gets the word associated with this state
         *
        /// @return the word
         */
        public Word GetWord() 
        {
            return ((AlternativeState) GetParent()).GetAlternative()[GetWhich()];
        }


        /**
        /// Returns a pretty name for this state
         *
        /// @return a pretty name for this state
         */

        public override string PrettyName
        {
            get { return Name + '(' + GetWord().Spelling + ')'; }
        }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public override string TypeLabel
        {
            get { return "Word"; }
        }


        /**
        /// Returns the state order for this state type
         *
        /// @return the state order
         */

        public override int Order
        {
            get { return 1; }
        }
    }
}
