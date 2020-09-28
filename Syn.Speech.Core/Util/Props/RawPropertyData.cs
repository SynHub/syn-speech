using System;
using System.Collections.Generic;
using System.Text;
//REFACTORED
using Syn.Speech.Helper;

namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// Holds the raw property data just as it has come in from the properties file.
    /// </summary>
    public class RawPropertyData
    {
        private readonly HashMap<String, Object> _properties;

        /// <summary>
        /// Creates a raw property data item.
        /// </summary>
        /// <param name="name">the name of the item</param>
        /// <param name="className">the class name of the item</param>
        public RawPropertyData(String name, string className)
            : this(name, className, new HashMap<String, Object>())
        {
            
        }

        /// <summary>
        /// Creates a raw property data item, using a given property map.
        /// </summary>
        /// <param name="name">the name of the item</param>
        /// <param name="className">the class name of the item</param>
        /// <param name="properties">existing property map to use</param>
        public RawPropertyData(String name, string className, HashMap<String, Object> properties)
        {
            Name = name;
            ClassName = className;
            _properties = properties;
        }

        /// <summary>
        /// Adds a new property with a {@code String} value.
        /// </summary>
        /// <param name="propName">the name of the property</param>
        /// <param name="propValue">the value of the property</param>
        public void Add(String propName, string propValue)
        {
            _properties.Add(propName, propValue);
        }

        /// <summary>
        /// Adds a new property with a List of string value.
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="propValue"></param>
        public void Add(String propName, List<String> propValue)
        {
            _properties.Add(propName, propValue);
        }

        /// <summary>
        /// Removes an existing property.
        /// </summary>
        /// <param name="propName">the name of the property</param>
        public void Remove(String propName)
        {
            _properties.Remove(propName);
        }

        /// <summary>
        /// Returns the className.
        /// </summary>
        /// <value></value>
        public string ClassName { get; private set; }

        /// <summary>
        /// Returns the name.
        /// </summary>
        /// <value></value>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the properties.
        /// </summary>
        /// <returns></returns>
        public HashMap<String, Object> GetProperties()
        {
            return _properties;
        }

        /// <summary>
        /// Determines if the map already contains an entry for a property.
        /// </summary>
        /// <param name="propName">the property of interest</param>
        /// <returns></returns>
        public Boolean Contains(String propName)
        {
            return _properties.ContainsKey(propName);
        }

        /// <summary>
        /// Returns a copy of this property data instance with all ${}-fields resolved.
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        public RawPropertyData Flatten(ConfigurationManager cm) 
        {
            var copyRPD = new RawPropertyData(Name, ClassName);

            foreach (var entry in _properties) 
            {
                var value = entry.Value;
                if (entry.Value is String) 
                {
                    if (((String)entry.Value).StartsWith("${"))
                        value = cm.GetGloPropReference(ConfigurationManagerUtils.StripGlobalSymbol((String) entry.Value));
                }
            
                copyRPD._properties.Add(entry.Key, value);
            }

            return copyRPD;
        }

        /// <summary>
        /// Lookup a global symbol with a given name (and resolves)
        /// </summary>
        /// <param name="key">the name of the property</param>
        /// <param name="globalProperties"></param>
        /// <returns>the property value or null if it doesn't exist.</returns>
        public string GetGlobalProperty(String key, Dictionary<String, String> globalProperties)
        {
            if (!key.StartsWith("${")) // is symbol already flat
                return key;

            while (true)
            {
                var retkey = globalProperties[key];
                if (retkey == null || !(retkey.StartsWith("${") && retkey.EndsWith("}")))
                    return retkey;
            }
        }


        /// <summary>
        ///  Provide information stored inside this Object, used mainly for debugging/testing.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder().Append("name : ").Append(Name);
            foreach(var entry in _properties) 
            {
                if (entry.Value != null) 
                {
                    if (entry.Value is String) 
                    {
                        output.Append("value string : ");
                    }
                    output.Append(entry.Value);
                }
            }
            return output.ToString();
        }
    }
}
