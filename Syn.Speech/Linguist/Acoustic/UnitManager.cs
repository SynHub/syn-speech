using System;
using System.Collections.Generic;
using Syn.Logging;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Manages the set of units for a recognizer
    /// </summary>
    public class UnitManager: IConfigurable
    {
        /// <summary>
        /// The name for the silence unit
        /// </summary>
        public static string SilenceName = "SIL";

        private const int SilenceID = 1;

        ///The silence unit
        public static Unit Silence = new Unit(SilenceName, true, SilenceID);
        private int _nextID = SilenceID + 1;

        private readonly Dictionary<String, Unit> _ciMap = new Dictionary<String, Unit>()
        {
            {SilenceName, Silence}
        };

        
        public UnitManager() 
        {

        }

        public void NewProperties(PropertySheet ps)
        {
            this.LogInfo("UnitManager.newProperties");
        }

        /// <summary>
        ///  Gets or creates a unit from the unit pool
        /// </summary>
        /// <param name="name">the name of the unit</param>
        /// <param name="filler"><code>true</code> if the unit is a filler unit</param>
        /// <param name="context"> the context for this unit</param>
        /// <returns>the unit</returns>
        public Unit GetUnit(String name, Boolean filler, Context context)
        {
            Unit unit = null;
            if (_ciMap != null && _ciMap.ContainsKey(name))
                unit = _ciMap[name];
            if (context == Context.EmptyContext)
            {
                if (unit == null)
                {
                    unit = new Unit(name, filler, _nextID++);
                    _ciMap.Add(name, unit);
                    this.LogInfo("CI Unit: " + unit);
                }
            }
            else
            {
                unit = new Unit(unit, filler, context);
            }
            return unit;
        }
        /// <summary>
        /// Gets or creates a unit from the unit pool
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filler"></param>
        /// <returns></returns>
        public Unit GetUnit(String name, Boolean filler)
        {
            return GetUnit(name, filler, Context.EmptyContext);
        }
        /// <summary>
        /// Gets or creates a unit from the unit pool
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Unit GetUnit(String name)
        {
            return GetUnit(name, false, Context.EmptyContext);
        }
    }
}
