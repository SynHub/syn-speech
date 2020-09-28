//PATROLLED + REFACTORED
using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Decoders.Scorer;

namespace Syn.Speech.Linguist.LexTree
{
    public class LexTreeHmmState : LexTreeState,IHMMSearchState, IScoreProvider 
    {
        private readonly float _logLanguageProbability;
        private readonly float _logInsertionProbability;
        private readonly Node _parentNode;
        private int _hashCode = -1;

        /**
           /// Constructs a LexTreeHMMState
            *
           /// @param hmmNode              the HMM state associated with this unit
           /// @param wordSequence         the word history
           /// @param languageProbability  the probability of the transition
           /// @param insertionProbability the probability of the transition
            */
        public LexTreeHmmState(HMMNode hmmNode, WordSequence wordSequence,
            float smearTerm, float smearProb, IHMMState hmmState,
            float languageProbability, float insertionProbability,
            Node parentNode, LexTreeLinguist parent)
            : base(hmmNode, wordSequence, smearTerm, smearProb,parent)
        {
            HmmState = hmmState;
            _parentNode = parentNode;
            _logLanguageProbability = languageProbability;
            _logInsertionProbability = insertionProbability;
        }


        /**
           /// Gets the ID for this state
            *
           /// @return the ID
            */

        public override string Signature
        {
            get { return base.Signature + "-HMM-" + HmmState.State; }
        }


        /**
           /// returns the HMM state associated with this state
            *
           /// @return the HMM state
            */

        public IHMMState HmmState { get; private set; }


        /**
           /// Generate a hashcode for an object
            *
           /// @return the hashcode
            */

        public override int GetHashCode() 
        {
            //TODO: PERFORMANCE CRTICAL - FIND FASTER SOLUTION
            if (_hashCode == -1)
            {
                _hashCode = base.GetHashCode() * 29 + (HmmState.State + 1);
                if (_parentNode != null)
                {
                    _hashCode *= 377;
                    _hashCode += _parentNode.GetHashCode();
                }
            }
            return _hashCode;
        }


        /**
           /// Determines if the given object is equal to this object
            *
           /// @param o the object to test
           /// @return <code>true</code> if the object is equal to this
            */
        public override bool Equals(object o)
        {
            //Todo: PERFORMANCE CRTICAL - THIS IS CALLED TWICE EVERY SESSION - No Cache Benefits
            if (o == this)
            {
                return true;
            }
            else if (o is LexTreeHmmState)
            {
                var other = (LexTreeHmmState)o;
                return HmmState == other.HmmState && _parentNode == other._parentNode && base.Equals(o);
            }
            else
            {
                return false;
            }
        }

        /**
           /// Gets the language probability of entering this state
            *
           /// @return the log probability
            */

        public override float LanguageProbability
        {
            get { return _logLanguageProbability; }
        }


        /**
           /// Gets the language probability of entering this state
            *
           /// @return the log probability
            */

        public override float InsertionProbability
        {
            get { return _logInsertionProbability; }
        }


        /**
           /// Retrieves the set of successors for this state
            *
           /// @return the list of successor states
            */
        public override ISearchStateArc[] GetSuccessors() 
        {
            var nextStates = GetCachedArcs();
            if (nextStates == null) 
            {

                //if this is an exit state, we are transitioning to a
                //new unit or to a word end.

                if (HmmState.IsExitState()) {
                    if (_parentNode == null) {
                        nextStates = base.GetSuccessors();
                    } else {
                        nextStates = base.GetSuccessors(_parentNode);
                    }
                } else {
                    //The current hmm state is not an exit state, so we
                    //just go through the next set of successors

                    var arcs = HmmState.GetSuccessors();
                    nextStates = new ISearchStateArc[arcs.Length];
                    for (var i = 0; i < arcs.Length; i++) 
                    {
                        var arc = arcs[i];
                        if (arc.HmmState.IsEmitting) {
                            //if its a self loop and the prob. matches
                            //reuse the state
                            if (arc.HmmState == HmmState
                                && _logInsertionProbability == arc.LogProbability) {
                                        nextStates[i] = this;
                                    } else {
                                        nextStates[i] = new LexTreeHmmState(
                                            (HMMNode) GetNode(), WordHistory,
                                            SmearTerm, SmearProb,
                                            arc.HmmState, Parent.LogOne,
                                            arc.LogProbability, _parentNode,Parent);
                                    }
                        } else {
                            nextStates[i] = new LexTreeNonEmittingHMMState(
                                (HMMNode) GetNode(), WordHistory,
                                SmearTerm, SmearProb,
                                arc.HmmState,
                                arc.LogProbability, _parentNode,Parent);
                        }
                    }
                }
                PutCachedArcs(nextStates);
            }
            return nextStates;
        }


        /** Determines if this is an emitting state */

        public override bool IsEmitting
        {
            get { return HmmState.IsEmitting; }
        }

        public override string ToString() 
        {
            return base.ToString() + " hmm:" + HmmState;
        }

        public override int Order
        {
            get { return 5; }
        }

        public float GetScore(IData data) 
        {
            return HmmState.GetScore(data);
        }

        public float[] GetComponentScore(IData feature) 
        {
            return HmmState.CalculateComponentScore(feature);
        }

    }
}