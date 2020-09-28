using System;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Util
{
    /// <summary>
    /// A simple converter which converts <code>DoubleData</code> to <code>FloatData</code> and vv (depending on its configuration). 
    /// All remaining <code>Data</code>s will pass this processor unchanged.
    /// @author Holger Brandl
    /// </summary>
    public class DataConverter : BaseDataProcessor
    {

        public const string ConvertD2F = "d2f";
        public const string ConvertF2D = "f2d";

        [S4String(DefaultValue = "d2f", Range = new[] { ConvertD2F, ConvertF2D })]
        public static string PropConversionMode = "conversionMode";
        private string _convMode;

        public DataConverter(String convMode)
        {
            _convMode = convMode;
        }

        public DataConverter()
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            _convMode = ps.GetString(PropConversionMode);
        }

        public override IData GetData()
        {
            var d = Predecessor.GetData();

            if (d is DoubleData && _convMode.Equals(ConvertD2F))
            {
                var dd = (DoubleData)d;
                d = new FloatData(MatrixUtils.Double2Float(dd.Values), dd.SampleRate,
                        dd.FirstSampleNumber);
            }
            else if (d is FloatData && _convMode.Equals(ConvertF2D))
            {
                var fd = (FloatData)d;
                d = new DoubleData(MatrixUtils.Float2Double(fd.Values), fd.SampleRate,
                        fd.FirstSampleNumber);
            }

            return d;
        }
    }

}
