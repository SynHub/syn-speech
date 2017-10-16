using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// Manages a set of <code>IConfigurable</code>s, their parameterization and the relationships between them. Configurations
    /// can be specified either by xml or on-the-fly during runtime. 
    /// </summary>
    public class ConfigurationManager : ICloneable
    {
        private List<IConfigurationChangeListener> _changeListeners = new List<IConfigurationChangeListener>();

        private LinkedHashMap<String, PropertySheet> _symbolTable = new LinkedHashMap<String, PropertySheet>();
        private HashMap<String, RawPropertyData> _rawPropertyMap = new HashMap<String, RawPropertyData>();
        private HashMap<String, String> _globalProperties = new HashMap<String, String>();

        private readonly Boolean _showCreations;

        /// <summary>
        /// Creates a new empty configuration manager. This constructor is only of use in cases when a system configuration
        /// is created during runtime.
        /// </summary>
        public ConfigurationManager()
        {
        }

        /// <summary>
        /// Creates a new configuration manager. Initial properties are loaded from the given URL. No need to keep the notion
        /// of 'context' around anymore we will just pass around this property manager.
        /// </summary>
        /// <param name="url">Path to config file.</param>
        public ConfigurationManager(URL url)
        {
            ConfigUrl = url;

            try
            {
                _rawPropertyMap = new SaxLoader(url, _globalProperties).Load();
            }
            catch (IOException e)
            {
                throw new SystemException(e.ToString());
            }

            ConfigurationManagerUtils.ApplySystemProperties(_rawPropertyMap, _globalProperties);
            //ConfigurationManagerUtils.ConfigureLogger(this);

            // we can't configure the configuration manager with itself so we
            // do some of these configure items manually.
            if (_globalProperties.ContainsKey("showCreations"))
            {
                var showCreations = _globalProperties["showCreations"];
                if (showCreations != null) _showCreations = "true".Equals(showCreations);
            }
        }

        /// <summary>
        /// Returns the property sheet for the given object instance
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public PropertySheet GetPropertySheet(String instanceName)
        {
            if (!_symbolTable.ContainsKey(instanceName))
            {
                // if it is not in the symbol table, so construct
                // it based upon our raw property data

                RawPropertyData rpd = null;
                if (_rawPropertyMap.ContainsKey(instanceName)) rpd = _rawPropertyMap[instanceName];

                if (rpd != null)
                {
                    var className = rpd.ClassName;
                    try
                    {
                       
                        // now load the property-sheet by using the class annotation
                        var propertySheet = new PropertySheet(Type.GetType(className, true), instanceName, this, rpd);

                        _symbolTable.Put(instanceName, propertySheet);

                    }
                    catch (Exception)
                    {
                        
                        Trace.Fail(string.Format("Class '{0}' not found in Assembly '{1}'",className, Assembly.GetCallingAssembly()));
                        throw;
                    }
                }
            }

            return _symbolTable.Get(instanceName);
        }

        /**
     /// Gets all instances that are of the given type.
      *
     /// @param type the desired type of instance
     /// @return the set of all instances
      */
        public List<String> GetInstanceNames(Type type)
        {
            var instanceNames = new List<String>();

            foreach (var ps in _symbolTable.Values)
            {
                if (!ps.IsInstanciated())
                    continue;

                if (ConfigurationManagerUtils.IsDerivedClass(ps.GetConfigurableClass(), type))
                    instanceNames.Add(ps.InstanceName);
            }

            return instanceNames;
        }

        /// <summary>
        /// Returns all names of configurables registered to this instance. The resulting set includes instantiated and
        /// non-instantiated components.
        /// </summary>
        /// <returns></returns>
        public ICollection<String> GetComponentNames() //TODO: Set<String> in Java
        {
            return _rawPropertyMap.Keys;
        }


        public IConfigurable Lookup(String instanceName)
        {
            // Apply all new properties to the model.
            instanceName = GetStrippedComponentName(instanceName);
            var ps = GetPropertySheet(instanceName);

            if (ps == null)
                return null;

            if (_showCreations)
                this.LogInfo("Creating: " + instanceName);

            return ps.GetOwner();
        }

        public IConfigurable Lookup(Type confClass)
        {
            var matchPropSheets = GetPropSheets(confClass);
            if (matchPropSheets.Count == 0)
            {
                return null;
            }
            Debug.Assert(matchPropSheets.Count == 1);
            return Lookup(matchPropSheets[0].GetInstanceName());
        }

        public List<PropertySheet> GetPropSheets(Type confClass)
        {
            var psCol = new List<PropertySheet>();
            foreach (var ps in _symbolTable.Values)
            {
                if (ConfigurationManagerUtils.IsDerivedClass(ps.GetConfigurableClass(), confClass))
                {
                    psCol.Add(ps);
                }
            }

            return psCol;
        }


        public void AddConfigurable(Type confClass, String name)
        {
            AddConfigurable(confClass, name, new HashMap<String, Object>());
        }

        public void AddConfigurable(Type confClass, String name, HashMap<String, Object> props)
        {
            if (name == null) // use the class name as default if no name is given
                name = confClass.Name;

            if (_symbolTable.ContainsKey(name))
                throw new ArgumentException("tried to override existing component name : " + name);

            var ps = GetPropSheetInstanceFromClass(confClass, props, name, this);
            _symbolTable.Put(name, ps);
            _rawPropertyMap.Put(name, new RawPropertyData(name, confClass.Name));

            foreach (IConfigurationChangeListener changeListener in _changeListeners)
                changeListener.ComponentAdded(this, ps);
        }

        public void AddConfigurable(IConfigurable configurable, String name)
        {
            if (_symbolTable.ContainsKey(name))
                throw new ArgumentException("tried to override existing component name");

            var dummyRPD = new RawPropertyData(name, configurable.GetType().Name);

            var ps = new PropertySheet(configurable, name, dummyRPD, this);
            _symbolTable.Put(name, ps);
            _rawPropertyMap.Put(name, dummyRPD);

            foreach (IConfigurationChangeListener changeListener in _changeListeners)
                changeListener.ComponentAdded(this, ps);
        }


        /// <summary>
        /// Returns the URL of the XML configuration which defined this configuration or <code>null</code>  if it was created
        /// dynamically.
        /// </summary>
        /// <value></value>
        public URL ConfigUrl { get; private set; }

        /** Returns a copy of the map of global properties set for this configuration manager. */
        public Dictionary<String, String> GetGlobalProperties()
        {
            return new Dictionary<String, String>(_globalProperties);
        }

        /// <summary>
        /// Returns a global property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetGlobalProperty(String propertyName)
        {
            //propertyName = propertyName.startsWith("$") ? propertyName : "${" + propertyName + "}";
            var globProp = _globalProperties[propertyName];
            return globProp;
        }

        public string GetStrippedComponentName(String propertyName)
        {
            Debug.Assert(propertyName != null);

            while (propertyName.StartsWith("$"))
                propertyName = _globalProperties[ConfigurationManagerUtils.StripGlobalSymbol(propertyName)];

            return propertyName;
        }

        public static T GetInstance<T>() where T : IConfigurable
        {
            //var instance = (IConfigurable)Activator.CreateInstance(typeof(T));
            return GetInstance<T>(typeof(T));
        }

        /// <summary>
        /// Creates an instance of the given <code>Configurable</code> by using the default parameters as defined by the
        /// class annotations to parameterize the component.
        /// </summary>
        /// <param name="targetClass"></param>
        /// <returns></returns>
        public static T GetInstance<T>(Type targetClass) where T : IConfigurable
        {
            return GetInstance<T>(targetClass, new Dictionary<String, Object>());
        }

        //public static T GetInstance<T>(Type targetType, Dictionary<String, Object> props) where T : IConfigurable
        //{
        //    var instance = (IConfigurable)Activator.CreateInstance(targetType);
        //    return (T)GetInstance(instance, props);
        //}


        /// <summary>
        /// Creates an instance of the given <code>Configurable</code> by using the default parameters as defined by the
        /// class annotations to parameterize the component. Default parameters will be overridden if a their names are
        /// contained in the given <code>props</code>-map
        /// </summary>
        /// <param name="targetClass"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static T GetInstance<T>(Type targetClass, Dictionary<String, Object> props) where T : IConfigurable
        {
            return GetInstance<T>(targetClass, props, null);
        }

        //public static T GetInstance<T>(Type targetClass, Dictionary<String, Object> props) where T : IConfigurable
        //{
        //    return (T)GetInstance(targetClass, props, null);
        //}

        /// <summary>
        /// Creates an instance of the given <code>Configurable</code> by using the default parameters as defined by the
        /// class annotations to parameterize the component. Default parameters will be overridden if a their names are
        /// contained in the given <code>props</code>-map. The component is used to create a parameterized logger for the
        /// Configurable being created.
        /// </summary>
        /// <param name="targetClass"></param>
        /// <param name="props"></param>
        /// <param name="compName"></param>
        /// <returns></returns>
        public static T GetInstance<T>(Type targetClass, Dictionary<String, Object> props, string compName) where T : IConfigurable
        {
            var ps = GetPropSheetInstanceFromClass(targetClass, props, compName, new ConfigurationManager());
            var configurable = ps.GetOwner();
            return (T)configurable;
        }

        private static PropertySheet GetPropSheetInstanceFromClass(Type targetClass, Dictionary<String, Object> defaultProps, string componentName, ConfigurationManager cm)
        {
            var rpd = new RawPropertyData(componentName, targetClass.Name);

            foreach (var entry in defaultProps)
            {
                var property = entry.Value;

                if (property.GetType().IsInstanceOfType(targetClass))//Todo: check behaviour. Note changing this results in UnitTest fails for Scorer.
                    property = property.GetType().Name;

                rpd.GetProperties().Add(entry.Key, property);
            }

            return new PropertySheet(targetClass, componentName, cm, rpd);
        }

        public string GetGloPropReference(String propertyName)
        {
            return _globalProperties[propertyName];
        }

        /// <summary>
        /// Informs all registered <code>ConfigurationChangeListener</code>s about a configuration changes the component
        /// named <code>configurableName</code>.
        /// </summary>
        /// <param name="configurableName"></param>
        /// <param name="propertyName"></param>
        public void FireConfChanged(String configurableName, string propertyName)
        {
            Debug.Assert(GetComponentNames().Contains(configurableName));

            foreach (var changeListener in _changeListeners)
                changeListener.ConfigurationChanged(configurableName, propertyName, this);
        }

        /**
        /// Sets a global property.
         *
        /// @param propertyName The name of the global property.
        /// @param value        The new value of the global property. If the value is <code>null</code> the property becomes
        ///                     removed.
         */
        public void SetGlobalProperty(String propertyName, string value)
        {
            if (value == null)
                _globalProperties.Remove(propertyName);
            else
                _globalProperties.Add(propertyName, value);

            // update all component configurations because they might be affected by the change
            foreach (var instanceName in GetInstanceNames(typeof(IConfigurable)))
            {
                var ps = GetPropertySheet(instanceName);
                if (ps.IsInstanciated())
                    try
                    {
                        ps.GetOwner().NewProperties(ps);
                    }
                    catch (PropertyException e)
                    {
                        this.LogInfo(e.Message);
                    }
            }
        }

        public Object Clone()
        {
            var cloneCM = (ConfigurationManager)new Object();

            cloneCM._changeListeners = new List<IConfigurationChangeListener>();
            cloneCM._symbolTable = new LinkedHashMap<String, PropertySheet>();
            foreach (KeyValuePair<String, PropertySheet> entry in _symbolTable)
            {
                cloneCM._symbolTable.Put(entry.Key, ((PropertySheet)entry.Value.Clone()));
            }

            cloneCM._globalProperties = new HashMap<String, String>(_globalProperties);
            cloneCM._rawPropertyMap = new HashMap<String, RawPropertyData>(_rawPropertyMap);


            return cloneCM;
        }
    }
}
