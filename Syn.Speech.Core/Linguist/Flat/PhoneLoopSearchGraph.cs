using System.Collections.Generic;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    class PhoneLoopSearchGraph : ISearchGraph
    {

        private readonly CIPhoneLoop _parent;
        
        protected Dictionary<string, ISearchState> ExistingStates;
        protected SentenceHMMState FirstState;


        /** Constructs a phone loop search graph. */
        public PhoneLoopSearchGraph(CIPhoneLoop parent)
        {
            _parent = parent;
            ExistingStates = new Dictionary<string, ISearchState>();
            FirstState = new UnknownWordState();
            SentenceHMMState branchState = new BranchOutState(FirstState);
            AttachState(FirstState, branchState, _parent.LogOne, _parent.LogOne);

            SentenceHMMState lastState = new LoopBackState(FirstState);
            lastState.SetFinalState(true);
            AttachState(lastState, branchState, _parent.LogOne, _parent.LogOne);

            for (var i = _parent.Model.GetContextIndependentUnitIterator(); i.MoveNext(); )
            {
                var unitState = new UnitState(i.Current, HMMPosition.Undefined);
                var debug = unitState.ToString();

                // attach unit state to the branch out state
                AttachState(branchState, unitState, _parent.LogOne, _parent.LogPhoneInsertionProbability);

                var hmm = _parent.Model.LookupNearestHMM(unitState.Unit, unitState.GetPosition(), false);
                var initialState = hmm.GetInitialState();
                var hmmTree = new HMMStateState(unitState, initialState);
                AddStateToCache(hmmTree);

                // attach first HMM state to the unit state
                AttachState(unitState, hmmTree, _parent.LogOne, _parent.LogOne);

                // expand the HMM tree
                var finalState = ExpandHMMTree(unitState, hmmTree);

                // attach final state of HMM tree to the loopback state
                AttachState(finalState, lastState, _parent.LogOne, _parent.LogOne);
            }
        }


        /**
        /// Retrieves initial search state
         *
        /// @return the set of initial search state
         */

        public ISearchState InitialState
        {
            get { return FirstState; }
        }


        /**
        /// Returns the number of different state types maintained in the search graph
         *
        /// @return the number of different state types
         */

        public int NumStateOrder
        {
            get { return 5; }
        }

        public bool WordTokenFirst
        {
            get { return false; }
        }

        /**
        /// Checks to see if a state that matches the given state already exists
         *
        /// @param state the state to check
        /// @return true if a state with an identical signature already exists.
         */
        private SentenceHMMState GetExistingState(SentenceHMMState state)
        {
            return (SentenceHMMState)ExistingStates.Get(state.Signature);
        }


        /**
        /// Adds the given state to the cache of states
         *
        /// @param state the state to add
         */
        protected void AddStateToCache(SentenceHMMState state)
        {
            ExistingStates.Add(state.Signature, state);
        }


        /**
        /// Expands the given hmm state tree
         *
        /// @param parent the parent of the tree
        /// @param tree   the tree to expand
        /// @return the final state in the tree
         */
        protected HMMStateState ExpandHMMTree(UnitState parent, HMMStateState tree)
        {
            var retState = tree;
            foreach (var arc in tree.HmmState.GetSuccessors())
            {
                HMMStateState newState;
                if (arc.HmmState.IsEmitting)
                {
                    newState = new HMMStateState(parent, arc.HmmState);
                }
                else
                {
                    newState = new NonEmittingHMMState(parent, arc.HmmState);
                }
                var existingState = GetExistingState(newState);
                var logProb = arc.LogProbability;
                if (existingState != null)
                {
                    AttachState(tree, existingState, _parent.LogOne, logProb);
                }
                else
                {
                    AttachState(tree, newState, _parent.LogOne, logProb);
                    AddStateToCache(newState);
                    retState = ExpandHMMTree(parent, newState);
                }
            }
            return retState;
        }


        protected void AttachState(SentenceHMMState prevState, SentenceHMMState nextState, float logLanguageProbability, float logInsertionProbability)
        {
            var arc = new SentenceHMMStateArc
                (nextState, logLanguageProbability, logInsertionProbability);
            prevState.Connect(arc);
        }
    }
}