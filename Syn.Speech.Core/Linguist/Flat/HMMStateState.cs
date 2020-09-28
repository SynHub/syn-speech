using System;
using System.Runtime.Serialization;
using Syn.Speech.Decoders.Scorer;
using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a hmmState in an SentenceHMMS
    /// IScoreProvider -  is implemented without using interface derivation
    /// </summary>
    public class HMMStateState : SentenceHMMState, ISerializable, IHMMSearchState , IScoreProvider
    
    {
        private readonly IHMMState _hmmState;
        private readonly Boolean _isEmitting;


        /**
        /// Creates a HMMStateState
         *
        /// @param parent   the parent of this state
        /// @param hmmState the hmmState associated with this state
         */
        public HMMStateState(SentenceHMMState parent, IHMMState hmmState) 
            : base("S", parent, hmmState.State)
        {
            _hmmState = hmmState;
            _isEmitting = hmmState.IsEmitting;
        }


        /**
        /// Gets the hmmState associated with this state
         *
        /// @return the hmmState
         */

        public IHMMState HmmState
        {
            get { return _hmmState; }
        }


        /**
        /// Determines if this state is an emitting state
         *
        /// @return true if the state is an emitting state
         */

        public override bool IsEmitting
        {
            get { return _isEmitting; }
        }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public override string TypeLabel
        {
            get { return "HMM"; }
        }


        /**
        /// Calculate the acoustic score for this state
         *
        /// @return the acoustic score for this state
         */
        public float GetScore(IData feature) 
        {
            return _hmmState.GetScore(feature);
        }


        /**
        /// Returns the state order for this state type
         *
        /// @return the state order
         */

        public override int Order
        {
            get { return _isEmitting ? 6 : 0; }
        }


        public float[] GetComponentScore(IData feature) {
		    return _hmmState.CalculateComponentScore(feature);
	    }
    }
}
