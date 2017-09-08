using System;
//REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// Describes all methods necessary to process change events of a <code>ConfigurationManager</code>.
    /// </summary>
    interface IConfigurationChangeListener
    {

        /// <summary>
        /// Called if the configuration of a registered compoenent named <code>configurableName</code> was changed.
        /// </summary>
        /// <param name="configurableName">The name of the changed configurable.</param>
        /// <param name="propertyName">The name of the property which was changed</param>
        /// <param name="cm">The <code>ConfigurationManager</code>-instance this component is registered to</param>
        void ConfigurationChanged(String configurableName, string propertyName, ConfigurationManager cm);


        /// <summary>
        /// Called if a new compoenent defined by <code>ps</code> was registered to the ConfigurationManager
        /// <code>cm</code>.
        /// </summary>
        /// <param name="cm"></param>
        /// <param name="ps"></param>
        void ComponentAdded(ConfigurationManager cm, PropertySheet ps);


        /// <summary>
        /// Called if a compoenent defined by <code>ps</code> was unregistered (removed) from the ConfigurationManager
        /// <code>cm</code>.
        /// </summary>
        /// <param name="cm"></param>
        /// <param name="ps"></param>
        void ComponentRemoved(ConfigurationManager cm, PropertySheet ps);


        /// <summary>
        /// Called if a compoenent was renamed.
        /// </summary>
        /// <param name="cm"></param>
        /// <param name="ps"></param>
        /// <param name="oldName"></param>
        void ComponentRenamed(ConfigurationManager cm, PropertySheet ps, string oldName);
    }
}
