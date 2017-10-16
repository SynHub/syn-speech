using System;
using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Language.Grammar;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// 
    /// A simple form of the linguist.
    ///
    /// The flat linguist takes a Grammar graph (as returned by the underlying, configurable grammar), and generates a search
    /// graph for this grammar.
    ///
    /// It makes the following simplifying assumptions:
    ///
    /// Zero or one word per grammar node <li> No fan-in allowed ever <li> No composites (yet) <li> Only Unit,
    /// HMMState, and pronunciation states (and the initial/final grammar state are in the graph (no word, alternative or
    /// grammar states attached). <li> Only valid transitions (matching contexts) are allowed <li> No tree organization of
    /// units <li> Branching grammar states are  allowed </ul>
    ///
    /// Note that all probabilities are maintained in the log math domain
    /// </summary>
    public partial class FlatLinguist : Linguist, IConfigurable
    {
        /**
       /// The property used to define the grammar to use when building the search graph
        */
        [S4Component(Type = typeof(Grammar))]
        public static string PropGrammar = "grammar";

        /**
        /// The property used to define the unit manager to use when building the search graph
         */
        [S4Component(Type = typeof(UnitManager))]
        public static string PropUnitManager = "unitManager";

        /**
        /// The property used to define the acoustic model to use when building the search graph
         */
        [S4Component(Type = typeof(AcousticModel))]
        public static string PropAcousticModel = "acousticModel";

        /**
        /// The property used to determine whether or not the gstates are dumped.
         */
        [S4Boolean(DefaultValue = false)]
        public static string PropDumpGstates = "dumpGstates";

        /**
        /// The property that specifies whether to add a branch for detecting out-of-grammar utterances.
         */
        [S4Boolean(DefaultValue = false)]
        public static string PropAddOutOfGrammarBranch = "addOutOfGrammarBranch";

        /**
        /// The property for the probability of entering the out-of-grammar branch.
         */
        [S4Double(DefaultValue = 1.0)]
        public static string PropOutOfGrammarProbability = "outOfGrammarProbability";

        /**
        /// The property for the acoustic model used for the CI phone loop.
         */
        [S4Component(Type = typeof(AcousticModel))]
        public static string PropPhoneLoopAcousticModel = "phoneLoopAcousticModel";

        /**
        /// The property for the probability of inserting a CI phone in the out-of-grammar ci phone loop
         */
        [S4Double(DefaultValue = 1.0)]
        public static string PropPhoneInsertionProbability = "phoneInsertionProbability";

        /**
        /// Property to control whether compilation progress is displayed on standard output. 
        /// If this property is true, a 'dot' is  displayed for every 1000 search states added
        ///  to the search space
         */
        [S4Boolean(DefaultValue = false)]
        public static string PropShowCompilationProgress = "showCompilationProgress";

        /**
        /// Property that controls whether word probabilities are spread across all pronunciations.
         */
        [S4Boolean(DefaultValue = false)]
        public static string PropSpreadWordProbabilitiesAcrossPronunciations = "spreadWordProbabilitiesAcrossPronunciations";

        protected static float LogOne = LogMath.LogOne;

        // note: some fields are protected to allow to override FlatLinguist.compileGrammar()

        // ----------------------------------
        // Subcomponents that are configured
        // by the property sheet
        // -----------------------------------
        protected Grammar Grammar;
        private AcousticModel _acousticModel;
        public UnitManager UnitManager;
        protected LogMath LogMath;

        // ------------------------------------
        // Fields that define the OOV-behavior
        // ------------------------------------
        protected AcousticModel PhoneLoopAcousticModel;
        protected Boolean AddOutOfGrammarBranch;
        protected float LogOutOfGrammarBranchProbability;
        protected float LogPhoneInsertionProbability;

        // ------------------------------------
        // Data that is configured by the
        // property sheet
        // ------------------------------------
        private float _logWordInsertionProbability;
        private float _logFillerInsertionProbability;
        private float _logUnitInsertionProbability;
        private Boolean _showCompilationProgress = true;
        private Boolean _spreadWordProbabilitiesAcrossPronunciations;
        private Boolean _dumpGStates;
        private float _languageWeight;

        // -----------------------------------
        // Data for monitoring performance
        // ------------------------------------
        protected StatisticsVariable TotalStates;
        protected StatisticsVariable TotalArcs;
        protected StatisticsVariable ActualArcs;
        private int _totalStateCounter;
        private static Boolean tracing = false;

        // ------------------------------------
        // Data used for building and maintaining
        // the search graph
        // -------------------------------------
        private HashSet<SentenceHMMState> _stateSet;
        public HashMap<GrammarNode, GState> NodeStateMap;
        protected Cache<SentenceHMMStateArc> ArcPool;
        protected GrammarNode InitialGrammarState;

        private ISearchGraph _searchGraph;


        /**
        /// Returns the search graph
         *
        /// @return the search graph
         */

        public override ISearchGraph SearchGraph
        {
            get { return _searchGraph; }
        }

        public FlatLinguist(AcousticModel acousticModel, Grammar grammar, UnitManager unitManager,
                double wordInsertionProbability, double silenceInsertionProbability, double fillerInsertionProbability,
                double unitInsertionProbability, float languageWeight, Boolean dumpGStates, Boolean showCompilationProgress,
                Boolean spreadWordProbabilitiesAcrossPronunciations, Boolean addOutOfGrammarBranch,
                double outOfGrammarBranchProbability, double phoneInsertionProbability, AcousticModel phoneLoopAcousticModel)
        {

            _acousticModel = acousticModel;
            LogMath = LogMath.GetLogMath();
            Grammar = grammar;
            UnitManager = unitManager;

            _logWordInsertionProbability = LogMath.LinearToLog(wordInsertionProbability);
            LogSilenceInsertionProbability = LogMath.LinearToLog(silenceInsertionProbability);
            _logFillerInsertionProbability = LogMath.LinearToLog(fillerInsertionProbability);
            _logUnitInsertionProbability = LogMath.LinearToLog(unitInsertionProbability);
            _languageWeight = languageWeight;

            _dumpGStates = dumpGStates;
            _showCompilationProgress = showCompilationProgress;
            _spreadWordProbabilitiesAcrossPronunciations = spreadWordProbabilitiesAcrossPronunciations;

            AddOutOfGrammarBranch = addOutOfGrammarBranch;

            if (addOutOfGrammarBranch)
            {
                LogOutOfGrammarBranchProbability = LogMath.LinearToLog(outOfGrammarBranchProbability);
                LogPhoneInsertionProbability = LogMath.LinearToLog(phoneInsertionProbability);
                PhoneLoopAcousticModel = phoneLoopAcousticModel;
            }

            Name = null;
        }

        public FlatLinguist()
        {

        }

        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public override void NewProperties(PropertySheet ps)
        {
            LogMath = LogMath.GetLogMath();

            _acousticModel = (AcousticModel)ps.GetComponent(PropAcousticModel);
            Grammar = (Grammar)ps.GetComponent(PropGrammar);
            UnitManager = (UnitManager)ps.GetComponent(PropUnitManager);

            // get the rest of the configuration data
            _logWordInsertionProbability = LogMath.LinearToLog(ps.GetDouble(PropWordInsertionProbability));
            LogSilenceInsertionProbability = LogMath.LinearToLog(ps.GetDouble(PropSilenceInsertionProbability));
            _logFillerInsertionProbability = LogMath.LinearToLog(ps.GetDouble(PropFillerInsertionProbability));
            _logUnitInsertionProbability = LogMath.LinearToLog(ps.GetDouble(PropUnitInsertionProbability));
            _languageWeight = ps.GetFloat(PropLanguageWeight);
            _dumpGStates = ps.GetBoolean(PropDumpGstates);
            _showCompilationProgress = ps.GetBoolean(PropShowCompilationProgress);
            _spreadWordProbabilitiesAcrossPronunciations = ps.GetBoolean(PropSpreadWordProbabilitiesAcrossPronunciations);

            AddOutOfGrammarBranch = ps.GetBoolean(PropAddOutOfGrammarBranch);

            if (AddOutOfGrammarBranch)
            {
                LogOutOfGrammarBranchProbability = LogMath.LinearToLog(ps.GetDouble(PropOutOfGrammarProbability));
                LogPhoneInsertionProbability = LogMath.LinearToLog(ps.GetDouble(PropPhoneInsertionProbability));
                PhoneLoopAcousticModel = (AcousticModel)ps.GetComponent(PropPhoneLoopAcousticModel);
            }

            Name = ps.InstanceName;
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#getName()
        */

        public string Name { get; private set; }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.linguist.Linguist#allocate()
        */
        public override void Allocate()
        {
            AllocateAcousticModel();
            Grammar.Allocate();
            TotalStates = StatisticsVariable.GetStatisticsVariable(Name, "totalStates");
            TotalArcs = StatisticsVariable.GetStatisticsVariable(Name, "totalArcs");
            ActualArcs = StatisticsVariable.GetStatisticsVariable(Name, "actualArcs");
            _stateSet = CompileGrammar();
            TotalStates.Value = _stateSet.Count;
        }


        /**
        /// Allocates the acoustic model.
        /// @throws java.io.IOException
         */
        protected void AllocateAcousticModel()
        {
            _acousticModel.Allocate();
            if (AddOutOfGrammarBranch)
            {
                PhoneLoopAcousticModel.Allocate();
            }
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.linguist.Linguist#deallocate()
        */
        public override void Deallocate()
        {
            if (_acousticModel != null)
            {
                _acousticModel.Deallocate();
            }
            Grammar.Deallocate();
        }


        /**
        /// Called before a recognition
         */
        public override void StartRecognition()
        {
            if (GrammarHasChanged())
            {
                _stateSet = CompileGrammar();
                TotalStates.Value = _stateSet.Count;
            }
        }


        /**
        /// Called after a recognition
         */
        public override void StopRecognition()
        {
        }

        /**
        /// Returns the log silence insertion probability.
         *
        /// @return the log silence insertion probability.
         */

        public float LogSilenceInsertionProbability { get; private set; }


        /// <summary>
        /// Compiles the grammar into a sentence HMM. A GrammarJob is created for the
        /// initial grammar node and added to the GrammarJob queue. While there are
        /// jobs left on the grammar job queue, a job is removed from the queue and
        /// the associated grammar node is expanded and attached to the tails.
        /// GrammarJobs for the successors are added to the grammar job queue.
        /// </summary>
        /// <returns></returns>
        protected HashSet<SentenceHMMState> CompileGrammar()
        {
            InitialGrammarState = Grammar.InitialNode;

            NodeStateMap = new HashMap<GrammarNode, GState>();
            // create in declaration section (22.12.2014)

            ArcPool = new Cache<SentenceHMMStateArc>();

            var gstateList = new List<GState>();
            TimerPool.GetTimer(this, "Compile").Start();

            // get the nodes from the grammar and create states
            // for them. Add the non-empty gstates to the gstate list.
            TimerPool.GetTimer(this, "Create States").Start();
            foreach (var grammarNode in Grammar.GrammarNodes)
            {
                var gstate = CreateGState(grammarNode);
                gstateList.Add(gstate);
            }
            TimerPool.GetTimer(this, "Create States").Stop();
            AddStartingPath();

            // ensures an initial path to the start state
            // Prep all the gstates, by gathering all of the contexts up
            // this allows each gstate to know about its surrounding contexts
            TimerPool.GetTimer(this, "Collect Contexts").Start();
            foreach (var gstate in gstateList)
                gstate.CollectContexts();
            TimerPool.GetTimer(this, "Collect Contexts").Stop();

            // now all gstates know all about their contexts, we can expand them fully
            TimerPool.GetTimer(this, "Expand States").Start();
            foreach (var gstate in gstateList)
                gstate.Expand();
            TimerPool.GetTimer(this, "Expand States").Stop();

            // now that all states are expanded fully, we can connect all the states up
            TimerPool.GetTimer(this, "Connect Nodes").Start();
            foreach (var gstate in gstateList)
                gstate.Connect();
            TimerPool.GetTimer(this, "Connect Nodes").Stop();

            var initialState = FindStartingState();

            // add an out-of-grammar branch if configured to do so
            if (AddOutOfGrammarBranch)
            {
                var phoneLoop = new CIPhoneLoop(PhoneLoopAcousticModel, LogPhoneInsertionProbability);
                var firstBranchState = (SentenceHMMState)phoneLoop.GetSearchGraph().InitialState;
                initialState.Connect(GetArc(firstBranchState, LogOne, LogOutOfGrammarBranchProbability));
            }

            _searchGraph = new FlatSearchGraph(initialState);
            TimerPool.GetTimer(this, "Compile").Stop();
            // Now that we are all done, dump out some interesting
            // information about the process
            if (_dumpGStates)
            {
                foreach (var grammarNode in Grammar.GrammarNodes)
                {
                    var gstate = GetGState(grammarNode);
                    gstate.DumpInfo();
                }
            }
            NodeStateMap = null;
            ArcPool = null;
            return SentenceHMMState.CollectStates(initialState);
        }


        /// <summary>
        /// Returns a new GState for the given GrammarNode.
        /// </summary>
        /// <param name="grammarNode"></param>
        /// <returns>a new GState for the given GrammarNode</returns>
        protected GState CreateGState(GrammarNode grammarNode)
        {
            return new GState(grammarNode, this);
        }


        /// <summary>
        /// Ensures that there is a starting path by adding an empty left context to the starting gstate
        /// </summary>
        protected void AddStartingPath()
        {
            // TODO: Currently the FlatLinguist requires that the initial grammar node returned by the Grammar contains a "sil" word
            AddStartingPath(Grammar.InitialNode);
        }

        /// <summary>
        /// Start the search at the indicated node
        /// </summary>
        /// <param name="initialNode"></param>
        protected void AddStartingPath(GrammarNode initialNode)
        {
            // guarantees a starting path into the initial node by
            // adding an empty left context to the starting gstate
            var gstate = GetGState(initialNode);
            gstate.AddLeftContext(UnitContext.Silence);
        }


        /**
        /// Determines if the underlying grammar has changed since we last compiled the search graph
         *
        /// @return true if the grammar has changed
         */
        protected Boolean GrammarHasChanged()
        {
            return InitialGrammarState == null ||
                    InitialGrammarState != Grammar.InitialNode;
        }


        /// <summary>
        /// Finds the starting state
        /// </summary>
        /// <returns></returns>
        protected SentenceHMMState FindStartingState()
        {
            var node = Grammar.InitialNode;
            var gstate = GetGState(node);
            return gstate.GetEntryPoint();
        }


        /**
        /// Gets a SentenceHMMStateArc. The arc is drawn from a pool of arcs.
         *
        /// @param nextState               the next state
        /// @param logLanguageProbability  the log language probability
        /// @param logInsertionProbability the log insertion probability
         */
        protected SentenceHMMStateArc GetArc(SentenceHMMState nextState,float logLanguageProbability, float logInsertionProbability)
        {
            var arc = new SentenceHMMStateArc(nextState,
                    logLanguageProbability * _languageWeight,
                    logInsertionProbability);
            var pooledArc = ArcPool.cache(arc);
            ActualArcs.Value = ArcPool.Misses;
            TotalArcs.Value = ArcPool.Hits + ArcPool.Misses;
            return pooledArc == null ? arc : pooledArc;
        }


        /**
        /// Given a grammar node, retrieve the grammar state
         *
        /// @param node the grammar node
        /// @return the grammar state associated with the node
         */
        protected GState GetGState(GrammarNode node)
        {
            return NodeStateMap[node];
        }

        /**
        /// Quick and dirty tracing. Traces the string if 'tracing' is true
         *
        /// @param s the string to trace.
         */
        private void T(String s)
        {
            if (tracing)
            {
                this.LogInfo(s);
            }
        }

        void IConfigurable.NewProperties(PropertySheet ps)
        {
            NewProperties(ps);
        }
    }
}
