using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a hmmState in an SentenceHMMS
    /// </summary>
    public class NonEmittingHMMState : HMMStateState
    {
         /**
        /// Creates a NonEmittingHMMState
         *
        /// @param parent   the parent of this state
        /// @param hmmState the hmmState associated with this state
         */
        public NonEmittingHMMState(SentenceHMMState parent, IHMMState hmmState)
            :base(parent, hmmState)
        {
            
        }
    }
}
