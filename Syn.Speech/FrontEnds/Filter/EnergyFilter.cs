using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Filter
{
    /// <summary>
    /// EnergyFilter silently drops zero energy frames from the stream. This is a deterministic alternative to <see cref="Dither"/>
    /// </summary>
    public class EnergyFilter : BaseDataProcessor
    {

        /** If energy is below this threshold frame is dropped */
        [S4Double(DefaultValue = 2.0)]
        public static string PropMaxEnergy = "maxEnergy";
        private double _maxEnergy;

        public EnergyFilter(double maxEnergy)
        {
            //initLogger();
            _maxEnergy = maxEnergy;
            Initialize();
        }

        public EnergyFilter()
        {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _maxEnergy = ps.GetDouble(PropMaxEnergy);
        }


        /// <summary>
        /// Returns the next DoubleData object, skipping frames with zero energy.
        /// </summary>
        /// <returns>
        /// The next available DoubleData object, or null if no Data is available.
        /// </returns>
        public override IData GetData()
        {
            double energy = 0;
            IData input;
            do
            {
                input = Predecessor.GetData();
                if (input == null || !(input is DoubleData))
                    return input;
                energy = 0.0f;
                foreach (double d in ((DoubleData)input).Values)
                {
                    energy += d * d;
                }
            } while (energy < _maxEnergy);

            return input;
        }
    }
}
