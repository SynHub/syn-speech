using System;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Util
{

    /**
     * Plots positive energy values of a cepstrum to stdout. The energy value is assumed to be the first element of the
     * double array returned by <code>Data.getValues()</code>. For negative energy value, a "-" will be printed out. The
     * plots look like the following, one line per cepstrum. The energy value for that particular cepstrum is printed at the
     * end of the line.
     * <p/>
     * <code> <br>+......7 <br>+......7 <br>Cepstrum: SPEECH_START <br>+......7 <br>+.......8 <br>+......7 <br>+.......8
     * <br>+.......8 <br>+........9 <br>+............14 <br>+...........13 <br>+...........13 <br>+...........13
     * <br>+.............15 <br>+.............15 <br>+..............16 <br>+..............16 <br>+..............16
     * <br>+.............15 <br>+............14 <br>+............14 <br>+............14 <br>+............14
     * <br>+.............15 <br>+..............16 <br>+...............17 <br>+...............17 <br>+...............17
     * <br>+...............17 <br>+...............17 <br>+...............17 <br>+..............16 <br>+.............15
     * <br>+............14 <br>+............14 <br>+............14 <br>+...........13 <br>+........9 <br>+.......8
     * <br>+......7 <br>+......7 <br>+......7 <br>Cepstrum: SPEECH_END <br>+......7 </code>
     */
    public class EnergyPlotter : IConfigurable
    {
        /// <summary>
        /// The maximum level of energy for which a plot string will be preconstructed.
        /// </summary>
        [S4Integer(DefaultValue = 20)]
        public static string PropMaxEnergy = "maxEnergy";

        private int _maxEnergy;
        private String[] _plots;

        public EnergyPlotter(int maxEnergy)
        {
            _maxEnergy = maxEnergy;
            BuildPlots(maxEnergy);
        }

        public EnergyPlotter()
        {

        }

        public void NewProperties(PropertySheet ps)
        {
            _maxEnergy = ps.GetInt(PropMaxEnergy);
            BuildPlots(_maxEnergy);
        }

        /// <summary>
        /// Builds the strings for the plots.
        /// </summary>
        /// <param name="maxEnergy">The maximum energy value.</param>
        private void BuildPlots(int maxEnergy)
        {
            _plots = new String[maxEnergy + 1];
            for (var i = 0; i < maxEnergy + 1; i++)
            {
                _plots[i] = GetPlotString(i);
            }
        }

        /// <summary>
        /// Gets the plot string for the given energy.
        /// </summary>
        /// <param name="energy">The energy level.</param>
        /// <returns></returns>
        private static string GetPlotString(int energy)
        {
            var plot = new char[energy];
            Arrays.Fill(plot,'.');
            if (energy > 0)
            {
                if (energy < 10)
                {
                    plot[plot.Length - 1] = (char)('0' + energy);
                }
                else
                {
                    plot[plot.Length - 2] = '1';
                    plot[plot.Length - 1] = (char)('0' + (energy - 10));
                }
            }
            return ('+' + new String(plot));
        }

        /// <summary>
        /// Plots the energy values of the given Data to System.out. If the Data contains a signal, it prints the signal.
        /// </summary>
        /// <param name="cepstrum">The Data to plot.</param>
        public void Plot(IData cepstrum)
        {
            if (cepstrum != null)
            {
                if (cepstrum is DoubleData)
                {
                    var energy = (int)((DoubleData)cepstrum).Values[0];
                    this.LogInfo(GetPlot(energy));
                }
                else
                {
                    this.LogInfo(cepstrum);
                }
            }
        }

        /// <summary>
        /// Returns the corresponding plot string for the given energy value. 
        /// The energy value should be positive or zero. If its negative, It will output the string "-".
        /// </summary>
        /// <param name="energy">The energy value.</param>
        /// <returns></returns>
        private string GetPlot(int energy)
        {
            if (energy < 0)
            {
                return "-";
            }
            if (energy <= _maxEnergy)
            {
                return _plots[energy];
            }
            return GetPlotString(energy);
        }
    }
}
