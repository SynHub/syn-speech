using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /**
     * Allows to modify the gain of an audio-signal.  If the gainFactor is 1 the signal passes this
     * <code>DataProcessor</code> unchanged.
     *
     * @author Holger Brandl
     */
    public class GainControlProcessor : BaseDataProcessor
    {

        [S4Double(DefaultValue = 1.0)]
        public static string GAIN_FACTOR = "gainFactor";

        public GainControlProcessor(double gainFactor)
        {
            //initLogger();
            this.GainFactor = gainFactor;
        }

        public GainControlProcessor()
        {
        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            GainFactor = ps.GetDouble(GAIN_FACTOR);
        }



        public override IData GetData()
        {
            IData data = Predecessor.GetData();

            if (data is FloatData)
            {
                float[] values = ((FloatData)data).Values;
                if (GainFactor != 1.0)
                {
                    // apply the gain-factor
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] *= (float)GainFactor;

                    }
                }

            }
            else if (data is DoubleData)
            {
                double[] values = ((DoubleData)data).Values;
                if (GainFactor != 1.0)
                {
                    // apply the gain-factor
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] *= GainFactor;

                    }
                }
            }

            return data;
        }


        public double GainFactor { get; set; }

    }
}
