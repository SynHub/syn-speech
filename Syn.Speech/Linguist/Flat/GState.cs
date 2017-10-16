using System;
using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Language.Grammar;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    public partial class FlatLinguist
    {
        /// <summary>
        /// 
        /// This is a nested class that is used to manage the construction of the states in a grammar node. There is one
        /// GState created for each grammar node. The GState is used to collect the entry and exit points for the grammar
        /// node and for connecting up the grammar nodes to each other.
        ///
        /// </summary>
        /// 
        public class GState
        {
            private readonly FlatLinguist _parent;
            private readonly HashMap<ContextPair, List<ISearchState>> _entryPoints = new HashMap<ContextPair, List<ISearchState>>();
            private readonly HashMap<ContextPair, List<ISearchState>> _exitPoints = new HashMap<ContextPair, List<ISearchState>>();
            private readonly HashMap<String, SentenceHMMState> _existingStates = new HashMap<String, SentenceHMMState>();

            private readonly GrammarNode _node;

            private readonly HashSet<UnitContext> _rightContexts = new HashSet<UnitContext>();
            private readonly HashSet<UnitContext> _leftContexts = new HashSet<UnitContext>();
            private HashSet<UnitContext> _startingContexts;

            private int _exitConnections;
            //        private GrammarArc[] successors = null;


            /// <summary>
            /// Creates a GState for a grammar node
            /// </summary>
            /// <param name="node">the grammar node</param>
            /// <param name="parent"></param>
            public GState(GrammarNode node, FlatLinguist parent)
            {
                _parent = parent;
                this._node = node;
                _parent.NodeStateMap.Put(node, this);
            }


            /// <summary>
            /// Retrieves the set of starting contexts for this node. The starting contexts are the set of Unit[] with a size
            /// equal to the maximum right context size.
            /// </summary>
            /// <returns>the set of starting contexts across nodes.</returns>
            private HashSet<UnitContext> GetStartingContexts()
            {
                if (_startingContexts == null)
                {
                    _startingContexts = new HashSet<UnitContext>();
                    // if this is an empty node, the starting context is the set of starting contexts for all successor
                    // nodes, otherwise, it is built up from each pronunciation of this word
                    if (_node.IsEmpty)
                    {
                        GrammarArc[] arcs = GetSuccessors();
                        foreach (GrammarArc arc in arcs)
                        {
                            GState gstate = _parent.GetGState(arc.GrammarNode);
                            _startingContexts.AddAll(gstate.GetStartingContexts());
                        }
                    }
                    else
                    {
                        //                    int maxSize = getRightContextSize();
                        Word word = _node.GetWord();
                        Pronunciation[] prons = word.GetPronunciations(null);
                        foreach (Pronunciation pron in prons)
                        {
                            UnitContext startingContext = GetStartingContext(pron);
                            _startingContexts.Add(startingContext);
                        }
                    }
                }
                return _startingContexts;
            }


            /**
               /// Retrieves the starting UnitContext for the given pronunciation
                *
               /// @param pronunciation the pronunciation
               /// @return a UnitContext representing the starting context of the pronunciation
                */
            private UnitContext GetStartingContext(Pronunciation pronunciation)
            {
                int maxSize = GetRightContextSize();
                Unit[] units = pronunciation.Units;
                Unit[] context = units.Length > maxSize ? Arrays.copyOf(units, maxSize) : units;
                return UnitContext.Get(context);
            }


            /// <summary>
            /// Retrieves the set of trailing contexts for this node. the trailing contexts are the set of Unit[] with a size
            /// equal to the maximum left context size that align with the end of the node
            /// </summary>
            /// <returns></returns>
            List<UnitContext> GetEndingContexts()
            {
                List<UnitContext> endingContexts = new List<UnitContext>();
                if (!_node.IsEmpty)
                {
                    int maxSize = GetLeftContextSize();
                    Word word = _node.GetWord();
                    Pronunciation[] prons = word.GetPronunciations(null);
                    foreach (Pronunciation pron in prons)
                    {
                        Unit[] units = pron.Units;
                        int size = units.Length;
                        Unit[] context = size > maxSize ? Arrays.copyOfRange(units, size - maxSize, size) : units;
                        endingContexts.Add(UnitContext.Get(context));
                    }
                }
                return endingContexts;
            }


            /// <summary>
            /// Visit all of the successor states, and gather their starting contexts into this gstates right context
            /// </summary>
            private void PullRightContexts()
            {
                GrammarArc[] arcs = GetSuccessors();
                foreach (GrammarArc arc in arcs)
                {
                    GState gstate = _parent.GetGState(arc.GrammarNode);
                    _rightContexts.AddAll(gstate.GetStartingContexts());
                }
            }

            /// <summary>
            /// Returns the set of succesor arcs for this grammar node. If a successor grammar node has no words we'll
            /// substitute the successors for that node (avoiding loops of course)
            /// </summary>
            /// <returns>an array of successors for this GState</returns>
            private GrammarArc[] GetSuccessors()
            {
                return _node.GetSuccessors();
            }


            /// <summary>
            /// Visit all of the successor states, and push our ending context into the successors left context
            /// </summary>
            void PushLeftContexts()
            {
                List<UnitContext> endingContext = GetEndingContexts();
                var visitedSet = new HashSet<GrammarNode>();
                PushLeftContexts(visitedSet, endingContext);
            }


            /// <summary>
            /// Pushes the given left context into the successor states. If a successor state is empty, continue to push into
            /// this empty states successors
            /// </summary>
            /// <param name="visitedSet"></param>
            /// <param name="leftContext">leftContext the context to push</param>
            void PushLeftContexts(HashSet<GrammarNode> visitedSet, List<UnitContext> leftContext)
            {
                if (visitedSet.Contains(GetNode()))
                {
                    return;
                }
                else
                {
                    visitedSet.Add(GetNode());
                }

                foreach (GrammarArc arc in GetSuccessors())
                {
                    GState gstate = _parent.GetGState(arc.GrammarNode);
                    gstate.AddLeftContext(leftContext);
                    // if our successor state is empty, also push our
                    // ending context into the empty nodes successors
                    if (gstate.GetNode().IsEmpty)
                    {
                        gstate.PushLeftContexts(visitedSet, leftContext);
                    }
                }
            }


            /**
               /// Add the given left contexts to the set of left contexts for this state
                *
               /// @param context the set of contexts to add
                */
            private void AddLeftContext(List<UnitContext> context)
            {
                _leftContexts.AddAll(context);
            }


            /**
               /// Adds the given context to the set of left contexts for this state
                *
               /// @param context the context to add
                */
            public void AddLeftContext(UnitContext context)
            {
                _leftContexts.Add(context);
            }

            /// <summary>
            /// Returns the entry points for a given context pair
            /// </summary>
            /// <param name="contextPair"></param>
            /// <returns></returns>
            private List<ISearchState> GetEntryPoints(ContextPair contextPair)
            {
                return _entryPoints.Get(contextPair);
            }

            /// <summary>
            /// Gets the context-free entry point to this state
            /// </summary>
            /// <returns>the entry point to the state</returns>
            /// TODO: ideally we'll look for entry points with no left
            // context, but those don't exist yet so we just take
            // the first entry point with an SILENCE left context
            // note that this assumes that the first node in a grammar has a
            // word and that word is a SIL. Not always a valid assumption.
            public SentenceHMMState GetEntryPoint()
            {
                ContextPair cp = ContextPair.Get(UnitContext.Silence, UnitContext.Silence);
                List<ISearchState> list = GetEntryPoints(cp);
                return list == null || list.Count == 0 ? null : (SentenceHMMState)list[0];
            }

            /// <summary>
            /// Collects the right contexts for this node and pushes this nodes ending context into the next next set of
            /// nodes.
            /// </summary>
            public void CollectContexts()
            {
                PullRightContexts();
                PushLeftContexts();
            }


            /// <summary>
            /// Expands each GState into the sentence HMM States
            /// </summary>
            public void Expand()
            {
                //this.LogDebug("Item Context: {0} : {1} : {2}",startingContexts.Count, rightContexts.Count, leftContexts.Count);
                // for each left context/starting context pair create a list
                // of starting states.
                foreach (UnitContext leftContext in _leftContexts)
                {
                    foreach (UnitContext startingContext in GetStartingContexts())
                    {
                        ContextPair contextPair = ContextPair.Get(leftContext, startingContext);
                        if (!_entryPoints.ContainsKey(contextPair))
                            _entryPoints.Add(contextPair, new List<ISearchState>());
                    }
                }
                this.LogDebug("Item entryPoints Count: {0}", _entryPoints.Count);
                // if this is a final node don't expand it, just create a
                // state and add it to all entry points
                if (_node.IsFinalNode)
                {
                    GrammarState gs = new GrammarState(_node);
                    foreach (List<ISearchState> epList in _entryPoints.Values)
                    {
                        epList.Add(gs);
                    }
                }
                else if (!_node.IsEmpty)
                {
                    // its a full fledged node with a word
                    // so expand it. Nodes without words don't need
                    // to be expanded.
                    foreach (UnitContext leftContext in _leftContexts)
                    {
                        ExpandWord(leftContext);
                    }
                }
                else
                {
                    //if the node is empty, populate the set of entry and exit
                    //points with a branch state. The branch state
                    // branches to the successor entry points for this
                    // state
                    // the exit point should consist of the set of
                    // incoming left contexts and outgoing right contexts
                    // the 'entryPoint' table already consists of such
                    // pairs so we can use that
                    foreach (var entry in _entryPoints)
                    {
                        ContextPair cp = entry.Key;
                        List<ISearchState> epList = entry.Value;
                        SentenceHMMState bs = new BranchState(cp.LeftContext.ToString(), cp.RightContext.ToString(), _node.ID);
                        epList.Add(bs);
                        AddExitPoint(cp, bs);
                    }
                }
                AddEmptyEntryPoints();
            }


            /**
               /// Adds the set of empty entry points. The list of entry points are tagged with a context pair. The context pair
               /// represent the left context for the state and the starting context for the state, this allows states to be
               /// hooked up properly. However, we may be transitioning from states that have no right hand context (CI units
               /// such as SIL fall into this category). In this case we'd normally have no place to transition to since we add
               /// entry points for each starting context. To make sure that there are entry points for empty contexts if
               /// necessary, we go through the list of entry points and find all left contexts that have a right hand context
               /// size of zero. These entry points will need an entry point with an empty starting context. These entries are
               /// synthesized and added to the the list of entry points.
                */
            private void AddEmptyEntryPoints()
            {
                var emptyEntryPoints = new HashMap<ContextPair, List<ISearchState>>();
                foreach (var entry in _entryPoints)
                {
                    ContextPair cp = entry.Key;
                    if (NeedsEmptyVersion(cp))
                    {
                        ContextPair emptyContextPair = ContextPair.Get(cp.LeftContext, UnitContext.Empty);
                        List<ISearchState> epList = emptyEntryPoints.Get(emptyContextPair);
                        if (epList == null)
                        {
                            epList = new List<ISearchState>();
                            emptyEntryPoints.Put(emptyContextPair, epList);
                        }
                        epList.AddRange(entry.Value);
                    }
                }
                _entryPoints.PutAll(emptyEntryPoints);
            }


            /**
               /// Determines if the context pair needs an empty version. A context pair needs an empty version if the left
               /// context has a max size of zero.
                *
               /// @param cp the contex pair to check
               /// @return <code>true</code> if the pair needs an empt version
                */
            private Boolean NeedsEmptyVersion(ContextPair cp)
            {
                UnitContext left = cp.LeftContext;
                Unit[] units = left.Units;
                return units.Length > 0 && (GetRightContextSize(units[0]) < GetRightContextSize());

            }


            /**
               /// Returns the grammar node of the gstate
                *
               /// @return the grammar node
                */
            private GrammarNode GetNode()
            {
                return _node;
            }


            /**
               /// Expand the the word given the left context
                *
               /// @param leftContext the left context
                */
            private void ExpandWord(UnitContext leftContext)
            {
                Word word = _node.GetWord();
                _parent.T("Expanding word " + word + " for lc " + leftContext);
                Pronunciation[] pronunciations = word.GetPronunciations(null);
                this.LogDebug("Item Pronounciation Count: {0}", pronunciations.Length);
                for (int i = 0; i < pronunciations.Length; i++)
                {
                    ExpandPronunciation(leftContext, pronunciations[i], i);
                }
            }


            /**
               /// Expand the pronunciation given the left context
                *
               /// @param leftContext   the left context
               /// @param pronunciation the pronunciation to expand
               /// @param which         unique ID for this pronunciation
                */
            // Each GState maintains a list of entry points. This list of
            // entry points is used when connecting up the end states of
            // one GState to the beginning states in another GState. The
            // entry points are tagged by a ContextPair which represents
            // the left context upon entering the state (the left context
            // of the initial units of the state), and the right context
            // of the previous states (corresponding to the starting
            // contexts for this state).
            //
            // When expanding a pronunciation, the following steps are
            // taken:
            //      1) Get the starting context for the pronunciation.
            //      This is the set of units that correspond to the start
            //      of the pronunciation.
            //
            //      2) Create a new PronunciationState for the
            //      pronunciation.
            //
            //      3) Add the PronunciationState to the entry point table
            //      (a hash table keyed by the ContextPair(LeftContext,
            //      StartingContext).
            //
            //      4) Generate the set of context dependent units, using
            //      the left and right context of the GState as necessary.
            //      Note that there will be fan out at the end of the
            //      pronunciation to allow for units with all of the
            //      various right contexts. The point where the fan-out
            //      occurs is the (length of the pronunciation - the max
            //      right context size).
            //
            //      5) Attach each cd unit to the tree
            //
            //      6) Expand each cd unit into the set of HMM states
            //
            //      7) Attach the optional and looping back silence cd
            //      unit
            //
            //      8) Collect the leaf states of the tree and add them to
            //      the exitStates list.
            private void ExpandPronunciation(UnitContext leftContext, Pronunciation pronunciation, int which)
            {
                UnitContext startingContext = GetStartingContext(pronunciation);
                // Add the pronunciation state to the entry point list
                // (based upon its left and right context)
                string pname = "P(" + pronunciation.Word + '[' + leftContext
                        + ',' + startingContext + "])-G" + GetNode().ID;
                PronunciationState ps = new PronunciationState(pname, pronunciation, which);
                _parent.T("     Expanding " + ps.Pronunciation + " for lc " + leftContext);
                ContextPair cp = ContextPair.Get(leftContext, startingContext);

                var epList = _entryPoints.Get(cp);
                if (epList == null)
                {
                    throw new Error("No EP list for context pair " + cp);
                }
                else
                {
                    epList.Add(ps);
                }

                Unit[] units = pronunciation.Units;
                int fanOutPoint = units.Length - GetRightContextSize();
                if (fanOutPoint < 0)
                {
                    fanOutPoint = 0;
                }
                SentenceHMMState tail = ps;
                for (int i = 0; tail != null && i < fanOutPoint; i++)
                {
                    tail = AttachUnit(ps, tail, units, i, leftContext, UnitContext.Empty);
                }
                SentenceHMMState branchTail = tail;
                foreach (UnitContext finalRightContext in _rightContexts)
                {
                    tail = branchTail;
                    for (int i = fanOutPoint; tail != null && i < units.Length; i++)
                    {
                        tail = AttachUnit(ps, tail, units, i, leftContext, finalRightContext);
                    }
                }
            }


            /**
               /// Attaches the given unit to the given tail, expanding the unit if necessary. If an identical unit is already
               /// attached, then this path is folded into the existing path.
                *
               /// @param parent       the parent state
               /// @param tail         the place to attach the unit to
               /// @param units        the set of units
               /// @param which        the index into the set of units
               /// @param leftContext  the left context for the unit
               /// @param rightContext the right context for the unit
               /// @return the tail of the added unit (or null if the path was folded onto an already expanded path.
                */
            private SentenceHMMState AttachUnit(PronunciationState parent,
                                                SentenceHMMState tail, Unit[] units, int which,
                                                UnitContext leftContext, UnitContext rightContext)
            {
                Unit[] lc = GetLC(leftContext, units, which);
                Unit[] rc = GetRC(units, which, rightContext);
                UnitContext actualRightContext = UnitContext.Get(rc);
                LeftRightContext context = LeftRightContext.Get(lc, rc);
                Unit cdUnit = _parent.UnitManager.GetUnit(units[which].Name, units[which].IsFiller, context);
                UnitState unitState = new ExtendedUnitState(parent, which, cdUnit);
                float logInsertionProbability;
                if (unitState.Unit.IsSilence)
                {
                    logInsertionProbability = _parent.LogSilenceInsertionProbability;
                }
                else if (unitState.Unit.IsFiller)
                {
                    logInsertionProbability = _parent._logFillerInsertionProbability;
                }
                else if (unitState.GetWhich() == 0)
                {
                    logInsertionProbability = _parent._logWordInsertionProbability;
                }
                else
                {
                    logInsertionProbability = _parent._logUnitInsertionProbability;
                }
                // check to see if this state already exists, if so
                // branch to it and we are done, otherwise, branch to
                // the new state and expand it.
                SentenceHMMState existingState = GetExistingState(unitState);
                if (existingState != null)
                {
                    AttachState(tail, existingState, LogOne, logInsertionProbability);
                    // T(" Folding " + existingState);
                    return null;
                }
                else
                {
                    AttachState(tail, unitState, LogOne, logInsertionProbability);
                    AddStateToCache(unitState);
                    // T(" Attaching " + unitState);
                    tail = ExpandUnit(unitState);
                    // if we are attaching the last state of a word, then
                    // we add it to the exitPoints table. the exit points
                    // table is indexed by a ContextPair, consisting of
                    // the exiting left context and the right context.
                    if (unitState.IsLast())
                    {
                        UnitContext nextLeftContext = GenerateNextLeftContext(leftContext, units[which]);
                        ContextPair cp = ContextPair.Get(nextLeftContext, actualRightContext);
                        // T(" Adding to exitPoints " + cp);
                        AddExitPoint(cp, tail);
                    }
                    return tail;
                }
            }


            /// <summary>
            /// Adds an exit point to this gstate
            /// </summary>
            /// <param name="cp">the context tag for the state</param>
            /// <param name="state">the state associated with the tag</param>
            private void AddExitPoint(ContextPair cp, SentenceHMMState state)
            {
                List<ISearchState> list = _exitPoints.Get(cp);
                if (list == null)
                {
                    list = new List<ISearchState>();
                    _exitPoints.Put(cp, list);
                }
                list.Add(state);
            }

            /**
               /// Get the left context for a unit based upon the left context size, the entry left context and the current
               /// unit.
                *
               /// @param left  the entry left context
               /// @param units the set of units
               /// @param index the index of the current unit

                */
            private Unit[] GetLC(UnitContext left, Unit[] units, int index)
            {
                Unit[] leftUnits = left.Units;
                int curSize = leftUnits.Length + index;
                int actSize = Math.Min(curSize, GetLeftContextSize(units[index]));
                int leftIndex = index - actSize;

                Unit[] lc = new Unit[actSize];
                for (int i = 0; i < lc.Length; i++)
                {
                    int lcIndex = leftIndex + i;
                    if (lcIndex < 0)
                    {
                        lc[i] = leftUnits[leftUnits.Length + lcIndex];
                    }
                    else
                    {
                        lc[i] = units[lcIndex];
                    }
                }
                return lc;
            }

            /**
               /// Get the right context for a unit based upon the right context size, the exit right context and the current
               /// unit.
                *
               /// @param units the set of units
               /// @param index the index of the current unit
               /// @param right the exiting right context

                */
            private Unit[] GetRC(Unit[] units, int index, UnitContext right)
            {
                Unit[] rightUnits = right.Units;
                int leftIndex = index + 1;
                int curSize = units.Length - leftIndex + rightUnits.Length;
                int actSize = Math.Min(curSize, GetRightContextSize(units[index]));

                Unit[] rc = new Unit[actSize];
                for (int i = 0; i < rc.Length; i++)
                {
                    int rcIndex = leftIndex + i;
                    if (rcIndex < units.Length)
                    {
                        rc[i] = units[rcIndex];
                    }
                    else
                    {
                        rc[i] = rightUnits[rcIndex - units.Length];
                    }
                }
                return rc;
            }

            /**
               /// Gets the maximum context size for the given unit
                *
               /// @param unit the unit of interest
               /// @return the maximum left context size for the unit
                */
            private int GetLeftContextSize(Unit unit)
            {
                return unit.IsFiller ? 0 : GetLeftContextSize();
            }


            /**
               /// Gets the maximum context size for the given unit
                *
               /// @param unit the unit of interest
               /// @return the maximum right context size for the unit
                */
            private int GetRightContextSize(Unit unit)
            {
                return unit.IsFiller ? 0 : GetRightContextSize();
            }


            /**
               /// Returns the size of the left context.
                *
               /// @return the size of the left context
                */
            protected int GetLeftContextSize()
            {
                return _parent._acousticModel.GetLeftContextSize();
            }

            /// <summary>
            /// Returns the size of the right context.
            /// </summary>
            /// <returns>the size of the right context</returns>
            protected int GetRightContextSize()
            {
                return _parent._acousticModel.GetRightContextSize();
            }

            /// <summary>
            /// Generates the next left context based upon a previous context and a unit
            /// </summary>
            /// <param name="prevLeftContext">the previous left context</param>
            /// <param name="unit">the current unit</param>
            /// <returns></returns>
            UnitContext GenerateNextLeftContext(UnitContext prevLeftContext, Unit unit)
            {
                Unit[] prevUnits = prevLeftContext.Units;
                int actSize = Math.Min(prevUnits.Length, GetLeftContextSize());
                if (actSize == 0) return UnitContext.Empty;
                Unit[] leftUnits = Arrays.copyOfRange(prevUnits, 1, actSize + 1);
                leftUnits[actSize - 1] = unit;
                return UnitContext.Get(leftUnits);
            }


            /**
               /// Expands the unit into a set of HMMStates. If the unit is a silence unit add an optional loopback to the
               /// tail.
                *
               /// @param unit the unit to expand
               /// @return the head of the hmm tree
                */
            protected SentenceHMMState ExpandUnit(UnitState unit)
            {
                SentenceHMMState tail = GetHMMStates(unit);
                // if the unit is a silence unit add a loop back from the
                // tail silence unit
                if (unit.Unit.IsSilence)
                {
                    // add the loopback, but don't expand it // anymore
                    AttachState(tail, unit, LogOne, _parent.LogSilenceInsertionProbability);
                }
                return tail;
            }


            /**
               /// Given a unit state, return the set of sentence hmm states associated with the unit
                *
               /// @param unitState the unit state of intereset
               /// @return the hmm tree for the unit
                */
            private HMMStateState GetHMMStates(UnitState unitState)
            {
                HMMStateState hmmTree;
                HMMStateState finalState;
                Unit unit = unitState.Unit;
                HMMPosition position = unitState.GetPosition();
                IHMM hmm = _parent._acousticModel.LookupNearestHMM(unit, position, false);
                IHMMState initialState = hmm.GetInitialState();
                hmmTree = new HMMStateState(unitState, initialState);
                AttachState(unitState, hmmTree, LogOne, LogOne);
                AddStateToCache(hmmTree);
                finalState = ExpandHMMTree(unitState, hmmTree);
                return finalState;
            }


            /**
               /// Expands the given hmm state tree
                *
               /// @param parent the parent of the tree
               /// @param tree   the tree to expand
               /// @return the final state in the tree
                */
            private HMMStateState ExpandHMMTree(UnitState parent, HMMStateState tree)
            {
                HMMStateState retState = tree;
                foreach (HmmStateArc arc in tree.HmmState.GetSuccessors())
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
                    SentenceHMMState existingState = GetExistingState(newState);
                    float logProb = arc.LogProbability;
                    if (existingState != null)
                    {
                        AttachState(tree, existingState, LogOne, logProb);
                    }
                    else
                    {
                        AttachState(tree, newState, LogOne, logProb);
                        AddStateToCache(newState);
                        retState = ExpandHMMTree(parent, newState);
                    }
                }
                return retState;
            }


            /// <summary>
            /// Connect up all of the GStates. Each state now has a table of exit points. These exit points represent tail
            /// states for the node. Each of these tail states is tagged with a ContextPair, that indicates what the left
            /// context is (the exiting context) and the right context (the entering context) for the transition. To connect
            /// up a state, the connect does the following: 
            /// 1) Iterate through all of the grammar successors for this state
            /// 2) Get the 'entry points' for the successor that match the exit points. 
            /// 3) Hook them up.
            /// 
            /// Note that for a task with 1000 words this will involve checking on the order of 35,000,000 connections and
            /// making about 2,000,000 connections
            /// </summary>
            public void Connect()
            {
                // T("Connecting " + node.getWord());
                foreach (GrammarArc arc in GetSuccessors())
                {
                    GState gstate = _parent.GetGState(arc.GrammarNode);
                    if (!gstate.GetNode().IsEmpty
                            && gstate.GetNode().GetWord().Spelling.Equals(IDictionary.SentenceStartSpelling))
                    {
                        continue;
                    }
                    float probability = arc.Probability;
                    // adjust the language probability by the number of
                    // pronunciations. If there are 3 ways to say the
                    // word, then each pronunciation gets 1/3 of the total
                    // probability.
                    if (_parent._spreadWordProbabilitiesAcrossPronunciations && !gstate.GetNode().IsEmpty)
                    {
                        int numPronunciations = gstate.GetNode().GetWord().GetPronunciations(null).Length;
                        probability -= _parent.LogMath.LinearToLog(numPronunciations);
                    }
                    float fprob = probability;
                    foreach (var entry in _exitPoints)
                    {
                        List<ISearchState> destEntryPoints = gstate.GetEntryPoints(entry.Key);
                        if (destEntryPoints != null)
                        {
                            List<ISearchState> srcExitPoints = entry.Value;
                            Connect(srcExitPoints, destEntryPoints, fprob);
                        }
                    }
                }
            }


            /// <summary>
            /// connect all the states in the source list to the states in the destination list
            /// </summary>
            /// <param name="sourceList">the set of source states</param>
            /// <param name="destList">the set of destination states.</param>
            /// <param name="logLangProb"></param>
            private void Connect(List<ISearchState> sourceList, List<ISearchState> destList, float logLangProb)
            {
                foreach (ISearchState source in sourceList)
                {
                    SentenceHMMState sourceState = (SentenceHMMState)source;
                    foreach (ISearchState dest in destList)
                    {
                        SentenceHMMState destState = (SentenceHMMState)dest;
                        sourceState.Connect(_parent.GetArc(destState, logLangProb, LogOne));
                        _exitConnections++;
                    }
                }
            }

            /**
               /// Attaches one SentenceHMMState as a child to another, the transition has the given probability
                *
               /// @param prevState              the parent state
               /// @param nextState              the child state
               /// @param logLanguageProbablity  the language probability of transition in the LogMath log domain
               /// @param logInsertionProbablity insertion probability of transition in the LogMath log domain
                */
            protected void AttachState(SentenceHMMState prevState,
                                        SentenceHMMState nextState,
                                        float logLanguageProbablity,
                                        float logInsertionProbablity)
            {
                prevState.Connect(_parent.GetArc(nextState,
                        logLanguageProbablity, logInsertionProbablity));
                if (_parent._showCompilationProgress && _parent._totalStateCounter++ % 1000 == 0)
                {
                    this.LogInfo(".");
                }
            }


            /**
               /// Returns all of the states maintained by this gstate
                *
               /// @return the set of all states
                */
            public List<ISearchState> GetStates()
            {
                // since pstates are not placed in the cache we have to
                // gather those states. All other states are found in the
                // existingStates cache.
                List<ISearchState> allStates = new List<ISearchState>(_existingStates.Values);
                foreach (List<ISearchState> list in _entryPoints.Values)
                {
                    allStates.AddRange(list);
                }
                return allStates;
            }


            /**
               /// Checks to see if a state that matches the given state already exists
                *
               /// @param state the state to check
               /// @return true if a state with an identical signature already exists.
                */
            private SentenceHMMState GetExistingState(SentenceHMMState state)
            {
                return _existingStates.Get(state.Signature);
            }


            /**
               /// Adds the given state to the cache of states
                *
               /// @param state the state to add
                */
            private void AddStateToCache(SentenceHMMState state)
            {
                _existingStates.Put(state.Signature, state);
            }


            /**
               /// Prints info about this GState
                */
            public void DumpInfo()
            {
                this.LogInfo(" ==== " + this + " ========");
                this.LogInfo("Node: " + _node);
                if (_node.IsEmpty)
                {
                    this.LogInfo("  (Empty)");
                }
                else
                {
                    this.LogInfo(" " + _node.GetWord());
                }
                this.LogInfo(" ep: " + _entryPoints.Count);
                this.LogInfo(" exit: " + _exitPoints.Count);
                this.LogInfo(" cons: " + _exitConnections);
                this.LogInfo(" tot: " + GetStates().Count);
                this.LogInfo(" sc: " + GetStartingContexts().Count);
                this.LogInfo(" rc: " + _leftContexts.Count);
                this.LogInfo(" lc: " + _rightContexts.Count);
                DumpDetails();
            }


            /**
               /// Dumps the details for a gstate
                */
            void DumpDetails()
            {
                DumpCollection(" entryPoints", (_entryPoints.Keys));
                DumpCollection(" entryPoints states", _entryPoints.Values);
                DumpCollection(" exitPoints", _exitPoints.Keys);
                DumpCollection(" exitPoints states", _exitPoints.Values);
                DumpNextNodes();
                DumpExitPoints(_exitPoints.Values);
                DumpCollection(" startingContexts", GetStartingContexts());
                DumpCollection(" branchingInFrom", _leftContexts);
                DumpCollection(" branchingOutTo", _rightContexts);
                DumpCollection(" existingStates", _existingStates.Keys);
            }


            /**
               /// Dumps out the names of the next set of grammar nodes
                */
            private void DumpNextNodes()
            {
                this.LogInfo("     Next Grammar Nodes: ");
                foreach (GrammarArc arc in _node.GetSuccessors())
                {
                    this.LogInfo("          " + arc.GrammarNode);
                }
            }


            /**
               /// Dumps the exit points and their destination states
                *
               /// @param eps the collection of exit points
                */
            private void DumpExitPoints(IEnumerable<List<ISearchState>> eps)
            {
                foreach (List<ISearchState> epList in eps)
                {
                    foreach (ISearchState state in epList)
                    {
                        this.LogInfo("      Arcs from: " + state);
                        foreach (ISearchStateArc arc in state.GetSuccessors())
                        {
                            this.LogInfo("          " + arc.State);
                        }
                    }
                }
            }


            /**
               /// Dumps the given collection
                *
               /// @param name       the name of the collection
               /// @param collection the collection to dump
                */
            private void DumpCollection(String name, IEnumerable<object> collection)
            {
                this.LogInfo("     " + name);
                foreach (Object obj in collection)
                {
                    this.LogInfo("         " + obj);
                }
            }
            /**
               /// Returns the string representation of the object
                *
               /// @return the string representation of the object
                */
            public override string ToString()
            {
                if (_node.IsEmpty)
                {
                    return "GState " + _node + "(empty)";
                }
                return "GState " + _node + " word " + _node.GetWord();
            }
        }
    }
}
