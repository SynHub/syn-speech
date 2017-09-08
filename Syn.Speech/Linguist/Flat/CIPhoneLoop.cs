using System;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Constructs a loop of all the context-independent phones. This loop is used in the static flat linguist for detecting
    /// out-of-grammar utterances. A 'phoneInsertionProbability' will be added to the score each time a new phone is entered.
    /// To obtain the all-phone search graph loop, simply called the method {@link #getSearchGraph() getSearchGraph}.
    ///
    /// For futher details of this approach cf. 'Modeling Out-of-vocabulary Words for Robust Speech Recognition', Brazzi,
    //// 2000, Proc. ICSLP
    /// </summary>
    public class CIPhoneLoop
    {
        public readonly AcousticModel Model;
        public readonly float LogPhoneInsertionProbability;
        public readonly float LogOne = LogMath.LogOne;


        /**
        /// Creates the CIPhoneLoop with the given acoustic model and phone insertion probability
         *
        /// @param model                        the acoustic model
        /// @param logPhoneInsertionProbability the insertion probability
         */
        public CIPhoneLoop(AcousticModel model,
                           float logPhoneInsertionProbability) 
        {
            Model = model;
            LogPhoneInsertionProbability = logPhoneInsertionProbability;
        }


        /**
        /// Creates a new loop of all the context-independent phones.
         *
        /// @return the phone loop search graph
         */
        public ISearchGraph GetSearchGraph() 
        {
            return new PhoneLoopSearchGraph(this);
        }
        
    }

    class UnknownWordState: SentenceHMMState,IWordSearchState 
    {
        public Pronunciation Pronunciation
        {
            get { return Word.Unknown.GetPronunciations()[0]; }
        }


        public override int Order
        {
            get { return 0; }
        }


        public override string Name
        {
            get { return "UnknownWordState"; }
        }


        /**
        /// Returns true if this UnknownWordState indicates the start of a word. Returns false if this UnknownWordState
        /// indicates the end of a word.
         *
        /// @return true if this UnknownWordState indicates the start of a word, false if this UnknownWordState indicates the
        ///         end of a word
         */
        public override Boolean IsWordStart()
        {
            return true;
        }
    }

    class LoopBackState:SentenceHMMState 
    {
        public LoopBackState(SentenceHMMState parent) 
            :base("CIPhonesLoopBackState", parent, 0)
        {
            
        }

        public override int Order
        {
            get { return 1; }
        }
    }

    class BranchOutState: SentenceHMMState 
    {
        public BranchOutState(SentenceHMMState parent) 
            :base("BranchOutState", parent, 0)
        {
            
        }

        public override int Order
        {
            get { return 1; }
        }
    }
}
