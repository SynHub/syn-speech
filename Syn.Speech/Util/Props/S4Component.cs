using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A component property.
    /// </summary>
    ///
    [S4Property]
    public class S4Component:Attribute
    {
        public S4Component()
        {
            DefaultClass = typeof (IConfigurable);
            Mandatory = true;
        }

        public Type Type { get; set; }

        public Type DefaultClass { get; set; }

        public bool Mandatory { get; set; }
    }
}
