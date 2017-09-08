using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A string property.
    /// @author Holger Brandl
    /// </summary>
    [S4Property]
    public class S4String: Attribute 
    {
       
        /// <summary>
        /// Default value to return
        /// </summary>
        public const string NotDefined = "nullnullnull";
        private readonly String[] _defaultRange = {};


        public S4String()
        {
            DefaultValue = NotDefined;
            Range = _defaultRange;
            Mandatory = true;
        }

        public string DefaultValue { get; set; }

        public String[] Range { get; set; }

        public bool Mandatory { get; set; }
    }
}
