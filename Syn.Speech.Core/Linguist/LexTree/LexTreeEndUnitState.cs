using System;
using System.Diagnostics;
using Syn.Speech.Linguist.Acoustic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    public class LexTreeEndUnitState : LexTreeState,IUnitSearchState 
    {
        readonly float _logLanguageProbability;
        readonly float _logInsertionProbability;


        /**
           /// Constructs a LexTreeUnitState
            *

           /// @param wordSequence the history of words




            */
        public LexTreeEndUnitState(EndNode endNode, WordSequence wordSequence,
            float smearTerm, float smearProb, float languageProbability,
            float insertionProbability, LexTreeLinguist _parent)
            : base(endNode, wordSequence, smearTerm, smearProb,_parent)
        {
            _logLanguageProbability = languageProbability;
            _logInsertionProbability = insertionProbability;
            //this.LogInfo("LTEUS " + logLanguageProbability + " " + logInsertionProbability);
        }


        /**
           /// Returns the base unit associated with this state
            *
           /// @return the base unit
            */

        public Unit Unit
        {
            get { return GetEndNode().BaseUnit; }
        }


        /**
           /// Generate a hashcode for an object
            *
           /// @return the hashcode
            */

        public override int GetHashCode()
        {
            //TODO: PERFORMANCE CRITICAL - Called sequentially 1s or twice per session
            return base.GetHashCode() * 17 + 423;
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


        /**
           /// Determines if the given object is equal to this object
            *
           /// @param o the object to test
           /// @return <code>true</code> if the object is equal to this
            */

        public override bool Equals(Object o)
        {
            return o == this || o is LexTreeEndUnitState && base.Equals(o);
            }


        /**
           /// Returns the unit node for this state
            *
           /// @return the unit node
            */
        private EndNode GetEndNode() {
            return (EndNode) GetNode();
        }


        /**
           /// Returns the list of successors to this state
            *
           /// @return a list of SearchState objects
            */
        public override ISearchStateArc[] GetSuccessors() 
        {
            ISearchStateArc[] arcs = GetCachedArcs();
            if (arcs == null) {
                HMMNode[] nodes = Parent.GetHMMNodes(GetEndNode());
                arcs = new ISearchStateArc[nodes.Length];

                if (Parent.GenerateUnitStates) {
                    for (int i = 0; i < nodes.Length; i++) {
                        arcs[i] = new LexTreeUnitState(nodes[i],
                            WordHistory, SmearTerm,
                            SmearProb, Parent.LogOne, Parent.LogOne,
                            GetNode() ,Parent);
                    }
                } else {
                    for (int i = 0; i < nodes.Length; i++) {
                        IHMM hmm = nodes[i].HMM;
                        arcs[i] = new LexTreeHmmState(nodes[i],
                            WordHistory, SmearTerm,
                            SmearProb, hmm.GetInitialState(),
                            Parent.LogOne, Parent.LogOne, GetNode(),Parent);
                    }
                }
                PutCachedArcs(arcs);
            }
            return arcs;
        }



        public override string ToString()
        {
            return base.ToString() + " EndUnit";
            }


        public override int Order
        {
            get { return 3; }
        }
    }
}