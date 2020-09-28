using System;
using System.Collections;
using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Manages HMMs. This HMMManager groups {@link Linguist.Acoustic.HMM HMMs} together by their {@link
    /// Linguist.Acoustic.HMMPosition position} with the word.
    /// </summary>
    public class HMMManager : IEnumerable<IHMM>
    {
        private readonly List<IHMM> _allHMMs = new List<IHMM>();
        private readonly Dictionary<HMMPosition, Dictionary<Unit, IHMM>> _hmmsPerPosition = new Dictionary<HMMPosition, Dictionary<Unit, IHMM>>();

        ///HMMPosition.class
        private EnumMap _hmmsPerPositionx = new EnumMap();

        public HMMManager()
        {
            foreach (HMMPosition pos in Enum.GetValues(typeof(HMMPosition)))
            {
                if (_hmmsPerPosition.ContainsKey(pos))
                    _hmmsPerPosition[pos] = new Dictionary<Unit, IHMM>();
                else
                    _hmmsPerPosition.Add(pos, new Dictionary<Unit, IHMM>());
            }
        }


        /// <summary>
        /// Put an HMM into this manager
        /// </summary>
        /// <param name="hmm">The hmm to manage.</param>
        public void Put(IHMM hmm)
        {
            var pos = hmm.Position;
            var unit = hmm.Unit;
            if (_hmmsPerPosition[pos].ContainsKey(unit))
                _hmmsPerPosition[pos][unit] = hmm;
            else
                _hmmsPerPosition[pos].Add(hmm.Unit, hmm);
            _allHMMs.Add(hmm);
        }


        /// <summary>
        /// Retrieves an HMM by position and unit.
        /// </summary>
        /// <param name="position">The position of the HMM.</param>
        /// <param name="unit">The unit that this HMM represents.</param>
        /// <returns>The HMM for the unit at the given position or null if no HMM at the position could be found.</returns>
        public IHMM Get(HMMPosition position, Unit unit)
        {
            var units = _hmmsPerPosition[position];
            if (units == null || units.Count == 0 || !units.ContainsKey(unit))
                return null;
            return units[unit];
        }

        /// <summary>
        /// Gets an iterator that iterates through all HMMs
        /// </summary>
        /// <returns>
        ///an iterator that iterates through all HMMs
        /// </returns>
        public IEnumerator<IHMM> GetEnumerator()
        {
            return _allHMMs.GetEnumerator();
        }

        /// <summary>
        /// Returns the number of HMMS in this manager
        /// </summary>
        /// <returns>The number of HMMs.</returns>
        private int GetNumHMMs()
        {
            var count = 0;

            foreach (var map in _hmmsPerPosition.Values)
            {
                if (map != null)
                    count += map.Count;
            }
            return count;
        }


        /// <summary>
        /// Log information about this manager
        /// </summary>
        public void LogInfo()
        {
            this.LogInfo("HMM Manager: " + GetNumHMMs() + " hmms");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
