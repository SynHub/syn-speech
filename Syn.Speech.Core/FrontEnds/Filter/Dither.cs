using System;
using System.Diagnostics;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Filter
{
    /// <summary>
    /// Implements a dither for the incoming packet. A small amount of random noise is added
    /// to the signal to avoid floating point errors and prevent the energy from
    /// being zero. <p/> Other {@link Data} objects are passed along unchanged through
    /// this Dither processor. <p/> See also {@link EnergyFilter}, an alternative to Dither.
    /// </summary>
    public class Dither : BaseDataProcessor
    {

        /// <summary>
        /// The maximal value which could be added/subtracted to/from the signal.
        /// </summary>
        [S4Double(DefaultValue = 2.0)]
        public static string PropMaxDither = "maxDither";
        private double _ditherMax;

        /// <summary>
        /// The maximal value of dithered values.
        /// </summary>
        [S4Double(DefaultValue = JDouble.MAX_VALUE)]
        public static string PropMaxVal = "upperValueBound";
        private double _maxValue;

        /// <summary>
        /// The minimal value of dithered values.
        /// </summary>
        [S4Double(DefaultValue = -JDouble.MAX_VALUE)]
        public static string PropMinVal = "lowerValueBound";
        private double _minValue;


        /// <summary>
        /// The property about using random seed or not.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropUseRandseed = "useRandSeed";
        private bool _useRandSeed;
        Random _r;

        public Dither(double ditherMax, bool useRandSeed, double maxValue, double minValue)
        {
            //initLogger();

            _ditherMax = ditherMax;
            _useRandSeed = useRandSeed;

            _maxValue = maxValue;
            _minValue = minValue;
            Initialize();
        }

        public Dither()
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            _ditherMax = ps.GetDouble(PropMaxDither);
            _useRandSeed = ps.GetBoolean(PropUseRandseed);

            _maxValue = ps.GetDouble(PropMaxVal);
            _minValue = ps.GetDouble(PropMinVal);
        }

        public override sealed void Initialize()
        {
            base.Initialize();
            if (_useRandSeed)
                _r = new Random();
            else
                _r = new Random(12345);
        }

        /// <summary>
        /// Returns the next DoubleData object, which is a dithered version of the input.
        /// </summary>
        /// <returns>
        /// The next available DoubleData object, or null if no Data is available.
        /// </returns>
        public override IData GetData()
        {
            var input = Predecessor.GetData(); // get the spectrum
            if (input != null && _ditherMax != 0)
            {
                if (input is DoubleData || input is FloatData)
                {
                    input = Process(input);
                }
            }
            return input;
        }

        /// <summary>
        /// rocess data, adding dither.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private DoubleData Process(IData input)
        {
            Debug.Assert(input is DoubleData);

            var doubleData = (DoubleData)input;
            var inFeatures = doubleData.Values;
            var outFeatures = new double[inFeatures.Length];
            for (var i = 0; i < inFeatures.Length; ++i)
            {
                outFeatures[i] = _r.NextDouble() * 2 * _ditherMax - _ditherMax + inFeatures[i];
                outFeatures[i] = Math.Max(Math.Min(outFeatures[i], _maxValue), _minValue);
            }

            var output = new DoubleData(outFeatures, doubleData.SampleRate,
                doubleData.FirstSampleNumber);

            return output;
        }
    }

}
