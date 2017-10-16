using System;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Represents a named value. A StatisticsVariable may be used to track data in a fashion that will allow the data to be
    /// viewed or dumped at any time.  Statistics are kept in a pool and are grouped in contexts. Statistics can be dumped
    /// as a whole or by context.
    /// </summary>
    public class StatisticsVariable
    {
        private static readonly HashMap<String, StatisticsVariable> Pool = new HashMap<String, StatisticsVariable>();

        /// <summary>
        /// Gets the StatisticsVariable with the given name from the given context. If the statistic does not currently
        /// exist, it is created. If the context does not currently exist, it is created. 
        /// </summary>
        /// <param name="statName">the name of the StatisticsVariable</param>
        /// <returns>the StatisticsVariable with the given name and context</returns>
        static public StatisticsVariable GetStatisticsVariable(String statName)
        {
            var stat = Pool.Get(statName);
            if (stat == null)
            {
                stat = new StatisticsVariable(statName);
                Pool.Put(statName, stat);
            }
            return stat;
        }

        /// <summary>
        ///  Gets the StatisticsVariable with the given name for the given instance and context. This is a convenience function.
        /// </summary>
        /// <param name="instanceName">The instance name of creator.</param>
        /// <param name="statName">Name of the StatisticsVariable.</param>
        /// <returns></returns>
        static public StatisticsVariable GetStatisticsVariable(string instanceName, string statName) 
        {
            return GetStatisticsVariable(instanceName + '.' + statName);
        }

        /// <summary>
        /// Dump all of the StatisticsVariable in the given context.
        /// </summary>
        static public void DumpAll() 
        {
            Logger.LogInfo<StatisticsVariable>(" ========= statistics  " + "=======");
            foreach (var stats in Pool.Values) 
            {
                stats.Dump();
            }
        }

        /// <summary>
        ///  Resets all of the StatisticsVariables in the given context.
        /// </summary>
        static public void ResetAll() 
        {
            foreach (var stats in Pool.Values) 
            {
                stats.Reset();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsVariable"/> class.
        /// </summary>
        /// <param name="statName">Name  of this StatisticsVariable.</param>
        private StatisticsVariable(String statName) 
        {
            Name = statName;
            Value = 0.0;
        }

        /// <summary>
        ///  Retrieves the name of this StatisticsVariable.
        /// </summary>
        /// <value>
        /// The name of this StatisticsVariable.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Retrieves the value for this StatisticsVariable.
        /// </summary>
        /// <value>
        /// The current value for this StatisticsVariable.
        /// </value>
        public double Value { get; set; }

        /// <summary>
        /// Resets this StatisticsVariable. The value is set to zero.
        /// </summary>
        public void Reset() 
        {
            Value = 0.0;
        }

        /// <summary>
        /// Dumps this StatisticsVariable.
        /// </summary>
        public void Dump() {
            if (IsEnabled) 
            {
                this.LogInfo(Name + ' ' + Value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get { return Enabled; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StatisticsVariable"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { private get; set; }

        /// <summary>
        /// Some simple tests for the StatisticsVariable
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args) 
        {
            var loops = GetStatisticsVariable("main", "loops");
            var sum = GetStatisticsVariable("main", "sum");

            var foot = GetStatisticsVariable("body", "foot");
            var leg = GetStatisticsVariable("body", "leg");
            var finger = GetStatisticsVariable("body", "finger");

            foot.Value = 2;
            leg.Value = 2;
            finger.Value = 10;

            DumpAll();
            DumpAll();

            for (var i = 0; i < 1000; i++) {
                loops.Value++;
                sum.Value += i;
            }

            DumpAll();

            var loopsAlias = GetStatisticsVariable("main", "loops");
            var sumAlias = GetStatisticsVariable("main", "sum");

            for (var i = 0; i < 1000; i++) {
                loopsAlias.Value++;
                sumAlias.Value += i;
            }

            DumpAll();
            ResetAll();
            DumpAll();
        }
    }
}
