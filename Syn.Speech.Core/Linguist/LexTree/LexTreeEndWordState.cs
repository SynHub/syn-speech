//PATROLLED + REFACTORED

namespace Syn.Speech.Linguist.LexTree
{
    public class LexTreeEndWordState : LexTreeWordState 
    {

        /**
           /// Constructs a LexTreeWordState
            *
           /// @param wordNode       the word node
           /// @param lastNode       the previous word node
           /// @param wordSequence   the sequence of words triphone context


           /// @param logProbability the probability of this word occurring
            */
        public LexTreeEndWordState(WordNode wordNode, HMMNode lastNode,
            WordSequence wordSequence, float smearTerm, float smearProb,
            float logProbability, LexTreeLinguist parent)
            : base(wordNode, lastNode, wordSequence, smearTerm, smearProb,
                logProbability,parent)
        {
            //Trace.WriteLine(string.Format("LexTreeEndWordState Created with values wordNode: {0}, lastNode: {1}, wordSequence: {2}, smearTerm: {3}, smearProb: {4}, logProbability: {5}", 
               // wordNode,lastNode,wordSequence,smearTerm,smearProb, logProbability));
        }


        public override int Order
        {
            get { return 2; }
        }
    }
}