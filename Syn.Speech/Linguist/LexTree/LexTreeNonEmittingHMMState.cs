//PATROLLED + REFACTORED
using Syn.Speech.Linguist.Acoustic;
namespace Syn.Speech.Linguist.LexTree
{
    public class LexTreeNonEmittingHMMState : LexTreeHmmState 
    {

        /**
           /// Constructs a NonEmittingLexTreeHMMState
            *


           /// @param hmmState     the hmm state associated with this unit

           /// @param wordSequence the word history
           /// @param probability  the probability of the transition occurring

            */
        public LexTreeNonEmittingHMMState(HMMNode hmmNode, WordSequence wordSequence,
            float smearTerm, float smearProb, IHMMState hmmState,
            float probability, Node parentNode, LexTreeLinguist _parent) 
            :base(hmmNode, wordSequence, smearTerm, smearProb, hmmState,
                _parent.LogOne, probability, parentNode, _parent)
        {
                
        }


        public override int Order
        {
            get { return 0; }
        }
    }
}