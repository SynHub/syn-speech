using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A logical property.
    /// </summary>
    [S4Property]
    public class S4Boolean:Attribute
    { 
        public S4Boolean(Boolean defaultValue=false)
        {
            // this default value will be mapped to zero by the configuration manager
            DefaultValue = defaultValue;
        }

        public bool DefaultValue { get; set; }
    }
}
