using System;
using System.Runtime.Serialization;
using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a set of alternatives in an SentenceHMMS
    /// </summary>
    public class AlternativeState : SentenceHMMState, ISerializable
    {
        /* Creates a WordState
       /// @param which*/
        public AlternativeState(GrammarState parent, int which) 
            :base("A", parent, which)
        {
        }


        /**
        /// Gets the word associated with this state
         *
        /// @return the word
         */
        public Word[] GetAlternative() 
        {
            return ((GrammarState) GetParent()).GrammarNode.GetWords(GetWhich());
        }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public override string TypeLabel
        {
            get { return "Alt"; }
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

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
