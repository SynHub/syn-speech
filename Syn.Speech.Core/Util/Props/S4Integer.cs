using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// An integer property.
    /// </summary>
    [S4Property]
    public class S4Integer:Attribute
    {
        /// <summary>
        /// Default value to return
        /// </summary>
        public const int NotDefined = -918273645;

        public S4Integer()
        {
            DefaultValue = NotDefined;
            Range = new[] { -Integer.MAX_VALUE, Integer.MAX_VALUE };
            Mandatory = true;
        }

        public int DefaultValue { get; set; }

        public int[] Range { get; set; }

        public bool Mandatory { get; set; }
    }
}
