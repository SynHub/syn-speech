using System;
//REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// ndicates that a problem occurred while setting one or more properties for this component. This includes errors as
    /// improper type for component(-lists) properties, out-of-range-problems for double-, int- and ranged string-properties,
    /// instantiation errors and undefined mandatory properties.
    /// 
    /// This exception is instantiable only by the configuration management classes itself. In order to indicate problems
    /// within Configurable.newProperties which are not coped by types or ranges (eg file-not-found, complex configuration
    /// logic problems, etc.) <code>PropertyException</code> (which superclasses this class) can be used.
    ///
    /// The intention of the class is to make a clear distinction between core configuration errors and high level user
    /// specific problems.
    /// </summary>
    /// @SuppressWarnings("serial")

    public class InternalConfigurationException : PropertyException
    {
        public InternalConfigurationException(String instanceName, string propertyName, string msg)
            :base(instanceName, propertyName, msg)
        {
            
        }


        public InternalConfigurationException(Exception cause, string instanceName, string propertyName, string msg)
            : base(cause, instanceName, propertyName, msg)
        {
            
        }
    }
}
