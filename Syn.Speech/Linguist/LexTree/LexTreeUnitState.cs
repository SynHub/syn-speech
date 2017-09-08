using System;
using System.Diagnostics;
using Syn.Speech.Linguist.Acoustic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    public class LexTreeUnitState :LexTreeState,IUnitSearchState 
    {
        /// <summary>
        /// Nested class interaction
        /// </summary>
        private readonly LexTreeLinguist _parent;

        private readonly float _logInsertionProbability;
        private readonly float _logLanguageProbability;
        private readonly Node _parentNode;
        private int _hashCode = -1;


        /**
           /// Constructs a LexTreeUnitState
            *
           /// @param wordSequence the history of words
            */
        public LexTreeUnitState(HMMNode hmmNode, WordSequence wordSequence,
            float smearTerm, float smearProb, float languageProbability,
            float insertionProbability, LexTreeLinguist parent)
            : this(hmmNode, wordSequence, smearTerm, smearProb,
                languageProbability, insertionProbability, null,parent)
        {
            
        }


        /**
           /// Constructs a LexTreeUnitState
            *
           /// @param wordSequence the history of words
            */
        public LexTreeUnitState(HMMNode hmmNode, WordSequence wordSequence,
            float smearTerm, float smearProb, float languageProbability,
            float insertionProbability, Node parentNode, LexTreeLinguist parent)
            :base(hmmNode, wordSequence, smearTerm, smearProb,parent)
        {
            
            _logInsertionProbability = insertionProbability;
            _logLanguageProbability = languageProbability;
            _parentNode = parentNode;
            _parent = parent;

            
           
        }


        /**
           /// Returns the base unit associated with this state
            *
           /// @return the base unit
            */

        public Unit Unit
        {
            get { return GetHMMNode().BaseUnit; }
        }


        /**
           /// Generate a hashcode for an object
            *
           /// @return the hashcode
            */


        public override int GetHashCode()
        {
            //Note: NOT performance critical - call wasn't detected in test session
            if (_hashCode == -1)
            {
                _hashCode = base.GetHashCode() * 17 + 421;
                if (_parentNode != null)
                {
                    _hashCode *= 432;
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
        
         public override bool Equals(Object o)
        {
            if (o == this) {
                return true;
            }
            if (o is LexTreeUnitState) {
                LexTreeUnitState other = (LexTreeUnitState) o;
                return _parentNode == other._parentNode && base.Equals(o);
            }
            return false;
        }


        /**
           /// Returns the unit node for this state
            *
           /// @return the unit node
            */
        private HMMNode GetHMMNode() {
            return (HMMNode) GetNode();
        }


        /**
           /// Returns the list of successors to this state
            *
           /// @return a list of SearchState objects
            */
        public override ISearchStateArc[] GetSuccessors() 
        {
            ISearchStateArc[] arcs = new ISearchStateArc[1];
            IHMM hmm = GetHMMNode().HMM;
            arcs[0] = new LexTreeHmmState(GetHMMNode(), WordHistory,
                SmearTerm, SmearProb, hmm.GetInitialState(),
                _parent.LogOne, _parent.LogOne, _parentNode,_parent);
            return arcs;
        }


        override
            public string ToString() {
            return base.ToString() + " unit";
            }


        /**
           /// Gets the acoustic probability of entering this state
            *
           /// @return the log probability
            */

        public override float InsertionProbability
        {
            get { return _logInsertionProbability; }
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


        public override int Order
        {
            get { return 4; }
        }
    }
}