using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A double property.
    /// </summary>
    [S4Property]
    public class S4Double:Attribute
    {
        /// <summary>
        /// Default value to return
        /// </summary>
        public const double NotDefined = -918273645.12345; // not bullet-proof, but should work in most cases.

        public S4Double()
        {
            DefaultValue = NotDefined;
            Range = new[] { -JDouble.MAX_VALUE, JDouble.MAX_VALUE };
            Mandatory = true;
        }
        

        public double DefaultValue { get; set; }

        public double[] Range { get; set; }

        public bool Mandatory { get; set; }
    }
}
