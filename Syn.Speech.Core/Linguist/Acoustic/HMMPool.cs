using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// The HMMPool provides the ability to manage units via small integer IDs.  Context Independent units and context
    /// dependent units can be converted to an ID. IDs can be used to quickly retrieve a unit or an hmm associated with the
    /// unit.  This class operates under the constraint that context sizes are exactly one, which is generally only valid for
    /// large vocabulary tasks.
    /// </summary>
    public class HMMPool
    {
        private readonly Unit[] _unitTable;
        private readonly Dictionary<HMMPosition, IHMM[]> _hmmTable;
        private readonly UnitManager _unitManager;

        static readonly HMMPosition[] Pos = { HMMPosition.Begin, HMMPosition.End, HMMPosition.Single, HMMPosition.Internal};

        static readonly int[] Ids = { 9206, 9320, 9620, 9865, 14831, 15836 };

        protected HMMPool(){
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMMPool"/> class.
        /// </summary>
        /// <param name="model">The model to use for the pool</param>
        /// <param name="unitManager">The unit manager.</param>
        /// <exception cref="System.Exception">
        /// LexTreeLinguist: Unsupported left context size
        /// or
        /// LexTreeLinguist: Unsupported right context size
        /// </exception>
        public HMMPool(AcousticModel model, UnitManager unitManager)
        {
            var maxCiUnits = 0;
            this.Model = model;
            this._unitManager = unitManager;
            TimerPool.GetTimer(this,"Build HMM Pool").Start();

            if (model.GetLeftContextSize() != 1)
                throw new Exception("LexTreeLinguist: Unsupported left context size");

            if (model.GetRightContextSize() != 1)
                throw new Exception("LexTreeLinguist: Unsupported right context size");

            // count CI units:
            var i = model.GetContextIndependentUnitIterator();
            while (i.MoveNext()) 
            {
                var unit = i.Current;
                //this.LogInfo("CI unit " + unit);
                if (unit.BaseID > maxCiUnits) 
                {
                    maxCiUnits = unit.BaseID;
                }
            }

            NumCiUnits = maxCiUnits + 1;

            _unitTable = new Unit[NumCiUnits* NumCiUnits* NumCiUnits];
            var iHMM = model.GetHMMIterator();
            while (iHMM.MoveNext()) 
            {
                var hmm = iHMM.Current;
                var unit = hmm.Unit;
                var id = GetId(unit);
                _unitTable[id] = unit;

                //this.LogInfo("Unit " + unit + " id " + id);

            }

            // build up the hmm table to allow quick access to the hmms
            _hmmTable = new Dictionary<HMMPosition, IHMM[]>();
            foreach (HMMPosition position in Enum.GetValues(typeof(HMMPosition))) 
            {
                var hmms = new IHMM[_unitTable.Length];
                Java.Put(_hmmTable, position, hmms);
                //hmmTable.Put(position, hmms);
                for (var j = 1; j < _unitTable.Length; j++) 
                {
                    var unit = _unitTable[j];
                    if (unit == null) {
                        unit = SynthesizeUnit(j);
                    }
                    if (unit != null) {
                        hmms[j] = model.LookupNearestHMM(unit, position, false);
                        Debug.Assert(hmms[j] != null);
                    }
                }
            }
            TimerPool.GetTimer(this,"Build HMM Pool").Stop();
        }

        public AcousticModel Model { get; private set; }

        /**
        /// Given a unit ID, generate a full context dependent unit that will allow us to look for a suitable hmm
         *
        /// @param id the unit id
        /// @return a context dependent unit for the ID
         */
        private Unit SynthesizeUnit(int id) 
        {
            var centralID = GetCentralUnitId(id);
            var leftID = GetLeftUnitId(id);
            var rightID = GetRightUnitId(id);

            if (centralID == 0 || leftID == 0 || rightID == 0) {
                return null;
            }

            var centralUnit = _unitTable[centralID];
            var leftUnit = _unitTable[leftID];
            var rightUnit = _unitTable[rightID];

            Debug.Assert(centralUnit != null);
            Debug.Assert(leftUnit != null);
            Debug.Assert(rightUnit != null);

            var lc = new Unit[1];
            var rc = new Unit[1];
            lc[0] = leftUnit;
            rc[0] = rightUnit;
            var context = LeftRightContext.Get(lc, rc);

            var unit = _unitManager.GetUnit(
                    centralUnit.Name, centralUnit.IsFiller,
                    context);


            //this.LogInfo("Missing " + getUnitNameFromID(id) + " returning " + unit);

            return unit;
        }

        /**
        /// Returns the number of CI units
         *
        /// @return the number of CI Units
         */
        public int NumCiUnits { get; private set; }

        /**
        /// Gets the unit for the given id
         *
        /// @param unitID the id for the unit
        /// @return the unit associated with the ID
         */
        public Unit GetUnit(int unitId) 
        {
            return _unitTable[unitId];
        }

        /**
        /// Given a unit id and a position, return the HMM associated with the
        /// unit/position.
         *
        /// @param unitID   the id of the unit
        /// @param position the position within the word
        /// @return the hmm associated with the unit/position
         */
        public IHMM GetHMM(int unitId, HMMPosition position) 
        {
            return _hmmTable[position][unitId];
        }

        /**
        /// given a unit return its ID
         *
        /// @param unit the unit
        /// @return an ID
         */
        public int GetId(Unit unit) 
        {
            if (unit.IsContextDependent()) 
            {
                var context = (LeftRightContext) unit.Context;
                Debug.Assert(context.LeftContext.Length == 1);
                Debug.Assert(context.RightContext.Length == 1);
                return BuildId(GetSimpleUnitId(unit),
                               GetSimpleUnitId(context.LeftContext[0]),
                               GetSimpleUnitId(context.RightContext[0]));
            } 
            else 
            {
                return GetSimpleUnitId(unit);
            }
        }

        /**
        /// Returns a context independent ID
         *
        /// @param unit the unit of interest
        /// @return the ID of the central unit (ignoring any context)
         */
        private int GetSimpleUnitId(Unit unit) 
        {
            return unit.BaseID;
        }

        public Boolean IsValidId(int unitId) 
        {
            return unitId >= 0 &&
                   unitId < _unitTable.Length &&
                   _unitTable[unitId] != null;
        }

        /**
        /// Builds an id from the given unit and its left and right unit ids
         *
        /// @param unitID  the id of the central unit
        /// @param leftID  the id of the left context unit
        /// @param rightID the id of the right context unit
        /// @return the id for the context dependent unit
         */
        public int BuildId(int unitId, int leftId, int rightId) 
        {
            // special case ... if the unitID is associated with
            // filler than we have no context ... so use the CI
            // form

            if (_unitTable[unitId] == null)
                return -1;

            int id;
            if (_unitTable[unitId].IsFiller) {
                id = unitId;
            } else {
                id = unitId* (NumCiUnits* NumCiUnits)
                        + (leftId* NumCiUnits)
                        + rightId;
            }

            //Debug.Assert(id < unitTable.Length);

            return id;
        }

        /**
        /// Given a unit id extract the left context unit id
         *
        /// @param id the unit id
        /// @return the unit id of the left context (0 means no left context)
         */
        private int GetLeftUnitId(int id) 
        {
            return (id / NumCiUnits) % NumCiUnits;
        }

        /**
        /// Given a unit id extract the right context unit id
         *
        /// @param id the unit id
        /// @return the unit id of the right context (0 means no right context)
         */
        private int GetRightUnitId(int id) 
        {
            return id % NumCiUnits;
        }

        /**
        /// Given a unit id extract the central unit id
         *
        /// @param id the unit id
        /// @return the central unit id
         */
        private int GetCentralUnitId(int id) 
        {
            return id / (NumCiUnits* NumCiUnits);
        }

        /**
        /// Given an ID, build up a name for display
         *
        /// @return the name baed on the ID
         */
        private string GetUnitNameFromId(int id) 
        {
            var centralID = GetCentralUnitId(id);
            var leftID = GetLeftUnitId(id);
            var rightID = GetRightUnitId(id);

            var cs = _unitTable[centralID] == null ? "(" + centralID + ')' :
                    _unitTable[centralID].ToString();
            var ls = _unitTable[leftID] == null ? ("(" + leftID + ')') :
                    _unitTable[leftID].ToString();
            var rs = _unitTable[rightID] == null ? "(" + rightID + ')' :
                    _unitTable[rightID].ToString();

            return cs + '[' + ls + ',' + rs + ']';
        }

        /**
        /// Retrieves an HMM for a unit in context. If there is no direct match, the
        /// nearest match will be used. Note that we are currently only dealing with,
        /// at most, single unit left and right contexts.
        /// 
        /// @param base the base CI unit
        /// @param lc  the left context
        /// @param rc the right context
        /// @param pos the position of the base unit within the word
        /// @return the HMM. (This should never return null)
         */
        public IHMM GetHMM(Unit _base, Unit lc, Unit rc, HMMPosition pos) 
        {
            var id = -1;
            var bid = GetId(_base);


            var lid = GetId(lc);
            var rid = GetId(rc);

            if (!IsValidId(bid)) {
                //Trace.TraceError("Bad HMM Unit: " + _base.getName());
                return null;
            }
            if (!IsValidId(lid)) {
                //Trace.TraceError("Bad HMM Unit: " + lc.getName());
                return null;
            }
            if (!IsValidId(rid)) {
                //Trace.TraceError("Bad HMM Unit: " + rc.getName());
                return null;
            }
            id = BuildId(bid, lid, rid);

            if (id < 0) {
                //Trace.TraceError("Unable to build HMM Unit ID for " + _base.GetType().Name
                //        + " lc=" + lc.getName() + " rc=" + rc.getName());
                return null;
            }
            var hmm = GetHMM(id, pos);
            if (hmm == null) {
                //Trace.TraceError("Missing HMM Unit for " + _base.GetType().Name + " lc="
                //        + lc.getName() + " rc=" + rc.getName());
            }

            return hmm;
        }

        /// <summary>
        ///  Dumps out info about this pool.
        /// </summary>
        public void DumpInfo() {
            this.LogInfo("Max CI Units " + NumCiUnits);
            this.LogInfo("Unit table size " + _unitTable.Length);

            if (Logger.Level == LogLevel.All)
            {
                for (var i = 0; i < _unitTable.Length; i++)
                {
                    this.LogInfo(i.ToString(CultureInfo.InvariantCulture) + ' ' + _unitTable[i]);
                }
            }
        }

        void Benchmark() {
            var nullCount = 0;
            this.LogInfo("benchmarking ...");
            TimerPool.GetTimer(this,"hmmPoolBenchmark").Start();

            for (var i = 0; i < 1000000; i++) {
                var id = Ids[i % Ids.Length];
                var position = Pos[i % Pos.Length];
                var hmm = GetHMM(id, position);
                if (hmm == null) {
                    nullCount++;
                }
            }
            TimerPool.GetTimer(this,"hmmPoolBenchmark").Stop();
            this.LogInfo("null count " + nullCount);
        }
    }
}
