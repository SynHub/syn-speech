using System;
//REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// Indicates that a problem occurred while setting one or more properties for this component
    /// </summary>
    public class PropertyException: SystemException
    {
        private readonly string _instanceName;
        private readonly string _propertyName;

        /// <summary>
        /// Creates a new property exception.
        /// </summary>
        /// <param name="instanceName">The component this exception is related to.  (or <code>null</code> if unknown)</param>
        /// <param name="propertyName">The name of the component-property which the problem is related. (or <code>null</code> if unknown)</param>
        /// <param name="msg">a description of the problem.</param>
        public PropertyException(String instanceName, string propertyName, string msg)
            :this(null, instanceName, propertyName, msg)
        {
            
        }

        /// <summary>
        /// Creates a new property exception.
        /// </summary>
        /// <param name="cause">The cause of exception. (or <code>null</code> if unknown)</param>
        /// <param name="instanceName">The component this exception is related to.  (or <code>null</code> if unknown)</param>
        /// <param name="propertyName">The name of the component-property which the problem is related. (or <code>null</code> if unknown)</param>
        /// <param name="msg">a description of the problem.</param>
        public PropertyException(Exception cause, string instanceName, string propertyName, string msg)
            : base(msg, cause)
        {
            _instanceName = instanceName;
            _propertyName = propertyName;
        }

        public PropertyException(Exception e)
            :base(e.Message,e)
        {
        
        }

        /// <summary>
        /// Retrieves the name of the offending property
        /// </summary>
        /// <returns>the name of the offending property</returns>
        public string GetProperty()
        {
            return _propertyName;
        }

        /// <summary>
        /// Returns a string representation of this object
        /// </summary>
        /// <returns>the string representation of the object.</returns>
        public override string ToString()
        {
            return "Property exception component:'" + _instanceName + "' property:'" + _propertyName + "' - " + Message + '\n'
                    + base.ToString();
        }
    }
}
