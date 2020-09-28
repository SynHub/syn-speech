using System;
using System.Collections.Generic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A list property.
    /// </summary>
    [S4Property]
    public class S4ComponentList:Attribute
    {
        public S4ComponentList()
        {
            DefaultList = new List<Type>();
            BeTolerant = true;

        }

        public Type Type { get; set; }

        public IList<Type> DefaultList { get; set; }

        public bool BeTolerant { get; set; }
    }
}
