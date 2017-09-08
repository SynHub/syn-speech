using System.Collections.Generic;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    /// <summary>
    /// The EntryPoint table is used to manage the set of entry points into the lex tree.
    /// </summary>
    public class EntryPointTable
    {
        private readonly Dictionary<Unit, EntryPoint> _entryPoints;

        /**
        /// Create the entry point table give the set of all possible entry point units
         *
        /// @param entryPointCollection the set of possible entry points
         */
        public EntryPointTable(IEnumerable<Unit> entryPointCollection,HMMTree parent) 
        {
            _entryPoints = new Dictionary<Unit, EntryPoint>();
            foreach (var unit in entryPointCollection) 
            {
               Java.Put(_entryPoints,unit, new EntryPoint(unit,parent));
            }
        }


        /**
        /// Given a CI unit, return the EntryPoint object that manages the entry point for the unit
         *
        /// @param baseUnit the unit of interest (A ci unit)
        /// @return the object that manages the entry point for the unit
         */
        public EntryPoint GetEntryPoint(Unit baseUnit) 
        {
            return _entryPoints[baseUnit];
        }


        /** Creates the entry point maps for all entry points. */
        public void CreateEntryPointMaps() 
        {
            foreach (EntryPoint ep in _entryPoints.Values)
            {

                this.LogDebug("Creating Entry Point: {0}", ep.BaseUnit.GetKey());
                ep.CreateEntryPointMap();
                this.LogDebug("Total Entry Points: {0}", ep.UnitToEntryPointMap.Count);
            }
        }


        /** Freezes the entry point table */
        public void Freeze() 
        {
            foreach (var ep in _entryPoints.Values) 
            {
                ep.Freeze();
            }
        }


        /** Dumps the entry point table */
        void Dump() 
        {
            foreach (var ep in _entryPoints.Values) 
            {
                ep.Dump();
            }
        }

    }
}
