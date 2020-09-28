using System.Collections.Generic;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Allphone
{

    public class AllphoneLinguist : Linguist
    {

        /// <summary>
        /// The property that defines the acoustic model to use when building the search graph.
        /// </summary>
        [S4Component(Type = typeof(AcousticModel))]
        public static string PropAcousticModel = "acousticModel";

        /// <summary>
        /// The property that controls phone insertion probability.
        /// Default value for context independent phoneme decoding is 0.05,
        /// while for context dependent - 0.01.
        /// </summary>
        [S4Double(DefaultValue = 0.05)]
        public static string PropPip = "phoneInsertionProbability";

        /// <summary>
        /// The property that controls whether to use context dependent phones.
        /// Changing it for true, don't forget to tune phone insertion probability.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropCD = "useContextDependentPhones";

        private List<IHMM> _fillerHMMs;
        private List<IHMM> _leftContextSilHMMs;
        private HashMap<SenoneSequence, List<Unit>> _senonesToUnits;
        private HashMap<Unit, HashMap<Unit, List<IHMM>>> _cdHMMs;

        public AllphoneLinguist()
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            AcousticModel = (AcousticModel)ps.GetComponent(PropAcousticModel);
            PhoneInsertionProb = LogMath.GetLogMath().LinearToLog(ps.GetFloat(PropPip));

            UseContextDependentPhones = ps.GetBoolean(PropCD);
            if (UseContextDependentPhones)
                CreateContextDependentSuccessors();
            else
                CreateContextIndependentSuccessors();
        }

        public override ISearchGraph SearchGraph
        {
            get { return new AllphoneSearchGraph(this); }
        }

        public override void StartRecognition()
        {
        }

        public override void StopRecognition()
        {
        }

        public override void Allocate()
        {
        }

        public override void Deallocate()
        {
        }

        public AcousticModel AcousticModel { get; private set; }

        public float PhoneInsertionProb { get; private set; }

        public bool UseContextDependentPhones { get; private set; }

        public List<IHMM> CISuccessors { get; private set; }

        public List<IHMM> GetCDSuccessors(Unit lc, Unit baseUnit)
        {
            if (lc.IsFiller)
                return _leftContextSilHMMs;
            if (baseUnit == UnitManager.Silence)
                return _fillerHMMs;
            return _cdHMMs.Get(lc).Get(baseUnit);
        }

        public List<Unit> GetUnits(SenoneSequence senoneSeq)
        {
            return _senonesToUnits.Get(senoneSeq);
        }

        private void CreateContextIndependentSuccessors()
        {
            var hmmIter = AcousticModel.GetHMMIterator();
            CISuccessors = new List<IHMM>();
            _senonesToUnits = new HashMap<SenoneSequence, List<Unit>>();
            while (hmmIter.MoveNext())
            {
                IHMM hmm = hmmIter.Current;
                if (!hmm.Unit.IsContextDependent())
                {
                    List<Unit> sameSenonesUnits;
                    SenoneSequence senoneSeq = ((SenoneHMM)hmm).SenoneSequence;
                    if ((sameSenonesUnits = _senonesToUnits.Get(senoneSeq)) == null)
                    {
                        sameSenonesUnits = new List<Unit>();
                        _senonesToUnits.Put(senoneSeq, sameSenonesUnits);
                    }
                    sameSenonesUnits.Add(hmm.Unit as Unit);
                    CISuccessors.Add(hmm);
                }
            }
        }

        private void CreateContextDependentSuccessors()
        {
            _cdHMMs = new HashMap<Unit, HashMap<Unit, List<IHMM>>>();
            _senonesToUnits = new HashMap<SenoneSequence, List<Unit>>();
            _fillerHMMs = new List<IHMM>();
            _leftContextSilHMMs = new List<IHMM>();
            var hmmIter = AcousticModel.GetHMMIterator();
            while (hmmIter.MoveNext())
            {
                IHMM hmm = hmmIter.Current;
                List<Unit> sameSenonesUnits;
                SenoneSequence senoneSeq = ((SenoneHMM)hmm).SenoneSequence;
                if ((sameSenonesUnits = _senonesToUnits.Get(senoneSeq)) == null)
                {
                    sameSenonesUnits = new List<Unit>();
                    _senonesToUnits.Put(senoneSeq, sameSenonesUnits);
                }
                sameSenonesUnits.Add(hmm.Unit as Unit);
                if (hmm.Unit.IsFiller)
                {
                    _fillerHMMs.Add(hmm);
                    continue;
                }
                if (hmm.Unit.IsContextDependent())
                {
                    LeftRightContext context = (LeftRightContext)hmm.Unit.Context;
                    Unit lc = context.LeftContext[0] as Unit;
                    if (lc == UnitManager.Silence)
                    {
                        _leftContextSilHMMs.Add(hmm);
                        continue;
                    }
                    Unit baseUnit = hmm.Unit.BaseUnit as Unit;
                    HashMap<Unit, List<IHMM>> lcSuccessors;
                    if ((lcSuccessors = _cdHMMs.Get(lc)) == null)
                    {
                        lcSuccessors = new HashMap<Unit, List<IHMM>>();
                        _cdHMMs.Put(lc, lcSuccessors);
                    }
                    List<IHMM> lcBaseSuccessors;
                    if ((lcBaseSuccessors = lcSuccessors.Get(baseUnit)) == null)
                    {
                        lcBaseSuccessors = new List<IHMM>();
                        lcSuccessors.Put(baseUnit, lcBaseSuccessors);
                    }
                    lcBaseSuccessors.Add(hmm);
                }
            }
            _leftContextSilHMMs.AddRange(_fillerHMMs);
        }

    }
}
