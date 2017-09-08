using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Syn.Speech.Helper;
using Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A property sheet which defines a collection of properties for a single component
    /// in the system.
    /// </summary>
    public class PropertySheet : ICloneable
    {
        public static string CompLogLevel = "logLevel";

        private HashMap<string, S4PropWrapper> _registeredProperties = new HashMap<string, S4PropWrapper>();
        private HashMap<string, Object> _propValues = new HashMap<string, Object>();

        /// <summary>
        /// Maps the names of the component properties to their (possibly unresolved) values.
        /// </summary>
        private HashMap<string, Object> _rawProps = new HashMap<string, Object>();

        private ConfigurationManager _cm;
        private IConfigurable _owner;
        private Type _ownerClass;

        private string _instanceName;

        public string InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }

        public PropertySheet(IConfigurable configurable, string name, RawPropertyData rpd, ConfigurationManager configurationManager)
            :this(configurable.GetType(), name, configurationManager, rpd)
        {
            _owner = configurable;
        }

        public PropertySheet(Type confClass, string name, ConfigurationManager cm, RawPropertyData rpd)
        {
            _ownerClass = confClass;
            _cm = cm;
            _instanceName = name;

            ParseClass(confClass);
            SetConfigurableClass(confClass);

            // now apply all xml properties
            var flatProps = rpd.Flatten(cm).GetProperties();
            _rawProps = new HashMap<string, Object>(rpd.GetProperties());

            foreach (var propName in _rawProps.Keys)
                _propValues.Put(propName, flatProps.Get(propName));

        }

        /// <summary>
        /// Registers a new property which type and default value are defined by the given sphincs property.
        /// </summary>
        /// <param name="propName">The name of the property to be registered.</param>
        /// <param name="property">The property annotation masked by a proxy.</param>
        private void RegisterProperty(string propName, S4PropWrapper property)
        {
            if (property == null || propName == null)
                throw new InternalConfigurationException(_instanceName, propName, "property or its value is null");

            if (!_registeredProperties.ContainsKey(propName))
                _registeredProperties.Put(propName, property);


            if (!_propValues.ContainsKey(propName))
            {
                _propValues.Put(propName, null);
                _rawProps.Put(propName, null);
            }
        }

        /// <summary>
        /// Returns the property names <code>name</code> which is still wrapped into the annotation instance.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public S4PropWrapper GetProperty<T>(string name) 
        {
            if (!_propValues.ContainsKey(name))
                throw new InternalConfigurationException(_instanceName, name,
                        "Unknown property '" + name + "' ! Make sure that you've annotated it.");

            var s4PropWrapper = _registeredProperties.Get(name);

            if (s4PropWrapper == null)
            {
                throw new InternalConfigurationException(_instanceName, name, "Property is not an annotated property of " + GetConfigurableClass());
            }

            if(!(s4PropWrapper.Annotation is T))
                throw new InternalConfigurationException(_instanceName, name, "Property annotation " + s4PropWrapper.Annotation + " doesn't match the required type ");

            return s4PropWrapper;
        }

        /// <summary>
        /// Gets the value associated with this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetString(string name)
        {
            var s4PropWrapper = GetProperty <S4String>(name);
            var s4String = ((S4String) s4PropWrapper.Annotation);

            if (_propValues[name] == null) 
            {
                var isDefDefined = s4String.DefaultValue != S4String.NotDefined;

                if (s4String.Mandatory) 
                {
                    if (!isDefDefined)
                        throw new InternalConfigurationException(_instanceName, name, Strings.MANDATORY_PROPERTY_NOT_SET);
                }
                _propValues.Put(name, isDefDefined ? s4String.DefaultValue : null);
            }

            var propValue = FlattenProp(name);

            // Check range
            var range = s4String.Range.ToList();
            if (range.Count!=0 && !range.Contains(propValue))
                throw new InternalConfigurationException(_instanceName, name, " is not in range (" + range + ')');

            return propValue;
        }

        private string FlattenProp(string name) 
        {
            var value = _propValues.Get(name);
            return value is string ? (string)value : null;
        }

        /// <summary>
        /// Gets the value associated with this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetInt(string name) 
        {
            var s4PropWrapper = GetProperty<S4Integer>(name);
            var s4Integer = (S4Integer) s4PropWrapper.Annotation;

            if (_propValues[name] == null) 
            {
                var isDefDefined = (s4Integer.DefaultValue != S4Integer.NotDefined);

                if (s4Integer.Mandatory) 
                {
                    if (!isDefDefined)
                        throw new InternalConfigurationException(_instanceName, name, Strings.MANDATORY_PROPERTY_NOT_SET);
                }
                else if (!isDefDefined)
                    throw new InternalConfigurationException(_instanceName, name, "no default value for non-mandatory property");

                _propValues.Put(name, s4Integer.DefaultValue);
            }

            var propObject = _propValues.Get(name);
            var propValue = propObject is int ? (int)propObject : (int) Integer.Decode(FlattenProp(name));

            var range = s4Integer.Range;
            if (range.Length != 2)
                throw new InternalConfigurationException(_instanceName, name, range + " is not of expected range type, which is {minValue, maxValue)");

            if (propValue < range[0] || propValue > range[1])
                throw new InternalConfigurationException(_instanceName, name, " is not in range (" + range + ')');

            return propValue;
        }

        /// <summary>
        /// Gets the value associated with this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public float GetFloat(string name)
        {
            return (float)GetDouble(name);
        }

        /// <summary>
        /// Gets the value associated with this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double GetDouble(string name)
        {
            var s4PropWrapper = GetProperty<S4Double>(name);
            var s4Double = (S4Double) s4PropWrapper.Annotation;

            if (_propValues[name] == null) 
            {
                var isDefDefined = (s4Double.DefaultValue != S4Double.NotDefined);

                if (s4Double.Mandatory) 
                {
                    if (!isDefDefined)
                        throw new InternalConfigurationException(_instanceName, name, "mandatory property is not set!");
                } else 
                    if (!isDefDefined)
                        throw new InternalConfigurationException(_instanceName, name, "no default value for non-mandatory property");

                _propValues.Put(name, s4Double.DefaultValue);
            }


            var propObject = _propValues.Get(name);
            Double propValue;
	
            if (propObject is Double)
    	        propValue = (Double)propObject;
            else if (propObject is Decimal)
                propValue = Convert.ToDouble(propObject, CultureInfo.InvariantCulture.NumberFormat);
            else
                propValue = double.Parse(FlattenProp(name), CultureInfo.InvariantCulture.NumberFormat);
            
            var range = s4Double.Range;
            if (range.Length != 2)
                throw new InternalConfigurationException(_instanceName, name, range + " is not of expected range type, which is {minValue, maxValue)");

            if (propValue < range[0] || propValue > range[1])
                throw new InternalConfigurationException(_instanceName, name, " is not in range (" + range + ")");

            return propValue;
        }

        /// <summary>
        /// Gets the value associated with this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Boolean GetBoolean(string name)
        {
            var s4PropWrapper = GetProperty<S4Boolean>(name);
            var s4Boolean = (S4Boolean) s4PropWrapper.Annotation;

            if (_propValues.Get(name) == null)
            {
                _propValues.Put(name, s4Boolean.DefaultValue);
            }
 
            var propObject = _propValues.Get(name);
            Boolean propValue;
        
            if (propObject is Boolean)
                propValue = (Boolean) propObject;
            else
                propValue = Boolean.Parse(FlattenProp(name));
    
            return propValue;
        }

        /// <summary>
        /// Gets a component associated with the given parameter name. First search
        /// the component in property table, then try to get component by name from
        /// the manager, then creates component with default properties.
        /// </summary>
        /// <param name="name">the parameter name</param>
        /// <returns>the component associated with the name</returns>
        public IConfigurable GetComponent(string name)
        {
            var s4PropWrapper = GetProperty<S4Component>(name);
            IConfigurable configurable = null;

            var s4Component = (S4Component) s4PropWrapper.Annotation;
            var expectedType = s4Component.Type;

            var propVal = _propValues.Get(name);

            if (propVal != null && propVal is IConfigurable) 
            {
                return (IConfigurable) propVal;
            }

            if (propVal != null && propVal is string) 
            {
                var ps = _cm.GetPropertySheet(FlattenProp(name));
                if (ps != null)
                    configurable = ps.GetOwner();
                else
                    throw new InternalConfigurationException(_instanceName, name, "component '" + FlattenProp(name)
                            + "' is missing");
            }

            if (configurable != null && !expectedType.IsInstanceOfType(configurable))
            {
                throw new InternalConfigurationException(_instanceName, name, "mismatch between annotation and component type");
            }

            if (configurable != null) 
            {
                _propValues.Put(name,configurable);
                return configurable;
            }

            configurable = GetComponentFromAnnotation(name, s4Component);

            _propValues.Put(name, configurable);
            return configurable;
        }

        private IConfigurable GetComponentFromAnnotation(string name, S4Component s4Component) 
        {
            var defClass = s4Component.DefaultClass;

            if (defClass == null && s4Component.Mandatory)
            {
                throw new InternalConfigurationException(_instanceName, name, "mandatory property is not set!");
            }

            if (defClass.IsAbstract && s4Component.Mandatory)
                throw new InternalConfigurationException(_instanceName, name, defClass.Name + " is abstract!");

            // because we're forced to use the default type, make sure that it
            // is set
            if (defClass == typeof(IConfigurable))
            {
                if (s4Component.Mandatory)
                {
                    throw new InternalConfigurationException(_instanceName, name, InstanceName
                            + ": no default class defined for " + name);
                }
                return null;
            }

            var configurable = ConfigurationManager.GetInstance<IConfigurable>(defClass);
            if (configurable == null)
            {
                throw new InternalConfigurationException(_instanceName, name, "instantiation of referenenced configurable failed");
            }
        
            return configurable;
        }
                        
 

        /// <summary>
        /// Returns the Type of of a registered component property without instantiating it.
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public Type GetComponentClass(string propName) 
        {
            Type defClass;
            if (_propValues.Get(propName) != null)
            {
                try
                {
                    var objClass = Type.GetType((string) _propValues.Get(propName));
                    defClass = Activator.CreateInstance(objClass).GetType();
                }
                catch (Exception e)
                {
                    var ps = _cm.GetPropertySheet(FlattenProp(propName));
                    defClass = ps._ownerClass;
                }
            }
            else
            {
                var comAnno = (S4Component)_registeredProperties[propName].Annotation;
                defClass = comAnno.DefaultClass;
                if (comAnno.Mandatory)
                    defClass = null;
            }

            return defClass;
        }

        /// <summary>
        /// Gets a list of float numbers associated with the given parameter name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> GetstringList(string name)
        {
            GetProperty<S4StringList>(name);
            return ConfigurationManagerUtils.ToStringList (_propValues.Get(name));
        }

        /// <summary>
        /// Gets a list of components associated with the given parameter name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">the parameter name</param>
        /// <returns>the component associated with the name</returns>
        public  List<T> GetComponentList<T>(string name)
        {
            GetProperty<S4ComponentList>(name);

            var components = _propValues.Get(name) as IList;

            Debug.Assert(_registeredProperties.Get(name).Annotation is S4ComponentList);
            var annotation = (S4ComponentList) _registeredProperties.Get(name).Annotation;
            
            // no components names are available and no component list was yet
            // loaded therefore load the default list of components from the 
            // annotation
            if (components == null) 
            {
                var defaultComponents = new List<IConfigurable>();
                foreach (var defClass in annotation.DefaultList) 
                {
                    defaultComponents.Add(ConfigurationManager.GetInstance<IConfigurable>(defClass));
                }

                _propValues.Put(name, defaultComponents);
            } 
            else if (components.Count!=0  && !(components[0] is IConfigurable)) 
            {
                var resolvedComponents = new List<IConfigurable>();

                foreach (Object componentName in components) 
                {
                    var configurable = _cm.Lookup((string) componentName);

                    if (configurable != null) 
                    {
                        resolvedComponents.Add(configurable);
                    } 
                    else if (!annotation.BeTolerant) 
                    {
                        throw new InternalConfigurationException(name,
                                (string) componentName, "lookup of list-element '"+ componentName + "' failed!");
                    }
                }

                _propValues.Put(name, resolvedComponents);
            }


            var result = new List<T>();
            var values = (List<IConfigurable>)_propValues.Get(name);
            try
            {
                foreach (var obj in values)
                {
                    result.Add((T)obj);
                }
            }
            catch 
            {
                throw new InternalConfigurationException(_instanceName,
                            name, "Not all elements have required type " + typeof(T).Name + " Found one of type ");
            }
            return result;
        }

        /// <summary>
        /// arses the string with multiple URL's separated by ;. Return the list of
        /// resources to load
        /// </summary>
        /// <param name="name">list with URL's</param>
        /// <returns>ist of resources</returns>
        public List<URL> GetResourceList(string name) 
        {
            var resourceList = new List<URL>();
            var pathListString = GetString(name);

            if (pathListString != null) 
            {
                foreach (var url in (pathListString.Split(new[]{';'}))) 
                {
                    try 
                    {
                        var resourceUrl = new URL(url);
                        resourceList.Add(resourceUrl);
                    } 
                    catch (UriFormatException mue) 
                    {
                        //IllegalArgumentException
                        throw new ArgumentException(url + " is not a valid URL. " + mue);
                    }
                }
            }
            return resourceList;
        }


        /// <summary>
        /// arses the string with multiple URL's separated by ;. Return the list of
        /// resources to load
        /// </summary>
        /// <param name="name">list with URL's</param>
        /// <returns>ist of resources</returns>
        public List<string> GetResourceStringList(string name)
        {
            var resourceList = new List<string>();
            var pathListString = GetString(name);

            if (pathListString != null)
            {
                foreach (var url in (pathListString.Split(new Char[] { ';' })))
                {
                    try
                    {
                        resourceList.Add(url);
                    }
                    catch (UriFormatException mue)
                    {
                        //IllegalArgumentException
                        throw new ArgumentException(url + " is not a valid URL. " + mue);
                    }
                }
            }
            return resourceList;
        }

        /// <summary>
        /// Returns the owner of this property sheet. In most cases this will be the configurable instance which was
        /// instrumented by this property sheet.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IConfigurable GetOwner() 
        {
            try {

                if (!IsInstanciated()) 
                {
                    // ensure that all mandatory properties are set before instantiating the component
                    var undefProps = GetUndefinedMandatoryProps();
                    if (undefProps.Count!=0) 
                    {
                        throw new InternalConfigurationException(_instanceName,
                                undefProps.ToString(), "not all mandatory properties are defined");
                    }

                    _owner = (IConfigurable) Activator.CreateInstance(_ownerClass);
                    _owner.NewProperties(this);
                }
            }
            catch (MissingMethodException e)
            {
                throw new InternalConfigurationException(e, _instanceName, null, "Can't access class " + _ownerClass);
            }
            catch (Exception e) 
            {
                throw new InternalConfigurationException(e, _instanceName, null, "Can't instantiate class " + _ownerClass);
            }

            return _owner;
        }


        /// <summary>
        /// Returns the set of all component properties which were tagged as mandatory but which are not set (or no default
        /// value is given).
        /// </summary>
        /// <returns></returns>
        public Collection<string> GetUndefinedMandatoryProps() 
        {
            var undefProps = new Collection<string>();
            foreach (var propName in  GetRegisteredProperties()) 
            {
                var anno = _registeredProperties[propName].Annotation;

                var isMandatory = false;
                if (anno is S4Component) 
                {
                    isMandatory = ((S4Component) anno).Mandatory && ((S4Component) anno).DefaultClass == null;
                } 
                else if (anno is S4String) 
                {
                    isMandatory = ((S4String) anno).Mandatory && ((S4String) anno).DefaultValue.Equals(S4String.NotDefined);
                } 
                else if (anno is S4Integer) 
                {
                    isMandatory = ((S4Integer) anno).Mandatory && ((S4Integer) anno).DefaultValue == S4Integer.NotDefined;
                } 
                else if (anno is S4Double) 
                {
                    isMandatory = ((S4Double) anno).Mandatory && ((S4Double) anno).DefaultValue == S4Double.NotDefined;
                }

                if (isMandatory && !((_rawProps.ContainsKey(propName) && _rawProps[propName]!=null) || (_propValues.ContainsKey(propName) && _propValues[propName] != null)))
                    undefProps.Add(propName);
            }
            return undefProps;
        }

        /// <summary>
        /// Returns the names of registered properties of this PropertySheet object. 
        /// </summary>
        /// <returns></returns>
        public Collection<string> GetRegisteredProperties()
        {
           var ret =  new Collection<string>(); 
            foreach(var key in _registeredProperties.Keys)
                ret.Add(key);
            return ret;
        }

        /// <summary>
        /// Returns the class of the owner configurable of this property sheet.
        /// </summary>
        /// <returns></returns>
        public Type GetConfigurableClass() 
        {
            return _ownerClass;
        }

        /// <summary>
        /// Sets the configurable class of this object.
        /// RuntimeException if the the <code>IConfigurable</code> is already instantiated.
        /// </summary>
        /// <param name="confClass"></param>
        internal void SetConfigurableClass(Type confClass) 
        {
            _ownerClass = confClass;

            // Don't allow changes of the class if the configurable has already been instantiated
            if (IsInstanciated())
                throw new SystemException("class is already instantiated");

            // clean up the properties if necessary
	        // registeredProperties.clear();

            var classProperties = new HashSet<string>();
            var classProps = ParseClass(confClass);
            foreach(var entry in classProps) 
            {
                try 
                {
                    //String propertyName = (String)entry.Key.Name;
                    var propertyName = (String)entry.Key.GetValue(null);

                    // make sure that there is not already another property with this name
                    Debug.Assert(!classProperties.Contains(propertyName),
                            "duplicate property-name for different properties: " + propertyName + " for the class " + confClass.GetType());

                    RegisterProperty(propertyName, new S4PropWrapper(entry.Value));
                    classProperties.Add(propertyName);
                } 
                catch (Exception e) 
                {
                   e.PrintStackTrace();
                }
            }
        }

        private static Dictionary<FieldInfo, Attribute> ParseClass(Type configurable) 
        {
            var classFields = new List<FieldInfo>();
            classFields.AddRange(configurable.GetFields());

            var baseType = configurable.BaseType;
            while (baseType!=null)
            {
                var classNestedFields = baseType.GetFields();
                classFields.AddRange(classNestedFields);
                baseType = baseType.BaseType;
            }
           

            var s4props = new Dictionary<FieldInfo, Attribute>();
            foreach(var field in classFields) 
            {
                var annotations = field.GetCustomAttributes(true);

                foreach (Attribute annotation in annotations) 
                {
                    var superAnnotations = annotation.GetType().GetCustomAttributes(true);

                    foreach(Attribute superAnnotation in superAnnotations) 
                    {
                        if (superAnnotation is S4Property) 
                        {
                            ///int fieldModifiers = field;
                            Debug.Assert( field.IsStatic, "property fields are assumed to be static");
                            Debug.Assert( field.IsPublic,  "property fields are assumed to be public");
                            //Debug.Assert( Modifier.isFinal(fieldModifiers) : "property fields are assumed to be final";
                            Debug.Assert( field.FieldType == typeof(string), "properties fields are assumed to be instances of String");

                            s4props.Add(field, annotation);
                        }
                    }
                }
            }

            return s4props;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="cmName"></param>
        /// <param name="value"></param>
        private void ApplyConfigurationChange(string propName, Object cmName, Object value)
        {
            _rawProps.Put(propName, cmName);
            _propValues.Put(propName, value ?? cmName);

            if (InstanceName != null)
                _cm.FireConfChanged(InstanceName, propName);

            if(_owner !=null)
                _owner.NewProperties(this);
        }


        /// <summary>
        /// Sets the given property to the given name
        /// </summary>
        /// <param name="name">the simple property name</param>
        /// <param name="value"></param>
        public void SetString(String name, string value)
        {
            // ensure that there is such a property
            if (!_registeredProperties.ContainsKey(name))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name +
                        "' is not a registered string-property");

            var annotation = _registeredProperties.Get(name).Annotation;
            if (!(annotation is S4String))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name + "' is of type string");

            ApplyConfigurationChange(name, value, value);
        }

        /// <summary>
        /// Sets the given property to the given name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetInt(string name, int value)
        {
            // ensure that there is such a property
            if (!_registeredProperties.ContainsKey(name))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name +
                        "' is not a registered int-property");

            var annotation = _registeredProperties[name].Annotation;
            if (!(annotation is S4Integer))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name + "' is of type int");

            ApplyConfigurationChange(name, value, value);
        }

        /// <summary>
        /// Sets the given property to the given name
        /// </summary>
        /// <param name="name">the simple property name</param>
        /// <param name="value">the value for the property</param>
        public void SetDouble(string name, double value)
        {
            // ensure that there is such a property
            if (!_registeredProperties.ContainsKey(name))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name +
                        "' is not a registered double-property");

            var annotation = _registeredProperties[name].Annotation;
            if (!(annotation is S4Double))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name + "' is of type double");

            ApplyConfigurationChange(name, value, value);
        }


        /// <summary>
        /// Sets the given property to the given name
        /// </summary>
        /// <param name="name">the simple property name</param>
        /// <param name="value">the value for the property</param>
        public void SetBoolean(string name, Boolean value)
        {
            if (!_registeredProperties.ContainsKey(name))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name +
                        "' is not a registered Boolean-property");

            var annotation = _registeredProperties[name].Annotation;
            if (!(annotation is S4Boolean))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name + "' is of type Boolean");

            ApplyConfigurationChange(name, value, value);
        }

        /// <summary>
        /// Sets the given property to the given name
        /// </summary>
        /// <param name="name">the simple property name</param>
        /// <param name="cmName">the name of the configurable within the configuration manager (required for serialization only)</param>
        /// <param name="value">the value for the property</param>
        public void SetComponent(string name, string cmName, IConfigurable value)
        {
            if (!_registeredProperties.ContainsKey(name))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name +
                        "' is not a registered compontent");

            var annotation = _registeredProperties[name].Annotation;
            if (!(annotation is S4Component))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name + "' is of type component");


            ApplyConfigurationChange(name, cmName, value);
        }

        /// <summary>
        /// Sets the given property to the given name
        /// </summary>
        /// <param name="name">the simple property name</param>
        /// <param name="valueNames">the list of names of the configurables within the configuration manager (required for
        ///                   serialization only)</param>
        /// <param name="value">the value for the property</param>
        public void SetComponentList(string name, List<string> valueNames, List<IConfigurable> value)
        {
            if (!_registeredProperties.ContainsKey(name))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name +
                        "' is not a registered component-list");

            var annotation = _registeredProperties[name].Annotation;
            if (!(annotation is S4ComponentList))
                throw new InternalConfigurationException(_instanceName, name, '\'' + name + "' is of type component-list");

            _rawProps.Put(name, valueNames);
            _propValues.Put(name, value);

            ApplyConfigurationChange(name, valueNames, value);
        }

        /// <summary>
        /// Sets the raw property to the given name
        /// </summary>
        /// <param name="key">the simple property name</param>
        /// <param name="val">the value for the property</param>
        void SetRaw(string key, Object val)
        {
            _rawProps.Put(key, val);
            _propValues.Put(key, null);
        }

        /// <summary>
        /// Gets the raw value associated with this name
        /// </summary>
        /// <param name="name"> name the name</param>
        /// <returns>the value as an object (it could be a string or a string[] depending upon the property type)</returns>
        public Object GetRaw(string name)
        {
            return _rawProps.Get(name);
        }

        /// <summary>
        /// Gets the raw value associated with this name, no global symbol replacement is performed.
        /// </summary>
        /// <param name="name">the name</param>
        /// <returns>the value as an object (it could be a string or a string[] depending upon the property type)</returns>
        public Object GetRawNoReplacement(string name)
        {
            return _rawProps.Get(name);
        }

        /// <summary>
        /// Returns the type of the given property.
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public PropertyType GetType(string propName) 
        {
            var wrapper = _registeredProperties[propName];
            if (wrapper == null) {
                throw new InternalConfigurationException(_instanceName, propName, " is not a valid property of" + GetConfigurableClass());
            }

            var annotation = wrapper.Annotation;
            if (annotation is S4Component)
                return PropertyType.Component;
            if (annotation is S4ComponentList)
                return PropertyType.ComponentList;
            if (annotation is S4Integer)
                return PropertyType.Int;
            if (annotation is S4Double)
                return PropertyType.Double;
            if (annotation is S4Boolean)
                return PropertyType.Boolean;
            if (annotation is S4String)
                return PropertyType.String;
            throw new SystemException("Unknown property type");
        }

        /// <summary>
        /// Gets the owning property manager
        /// </summary>
        /// <returns></returns>
        public ConfigurationManager GetPropertyManager()
        {
            return _cm;
        }

        public TraceListener GetLogger(int index)
        {
            return  Trace.Listeners[index];
        }

        public void SetCM(ConfigurationManager cm)
        {
            _cm = cm;
        }

        /// <summary>
        /// Returns true if two property sheet define the same object in terms of configuration. The owner (and the parent
        /// configuration manager) are not expected to be the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj) 
        {
            if (obj == null || !(obj is PropertySheet))
                return false;

            var ps = (PropertySheet) obj;
            if (!_rawProps.Keys.Equals(ps._rawProps.Keys))
                return false;

            // maybe we could test a little bit more here. suggestions?
            return true;
        }

        public override int GetHashCode()
        {
            Debug.Assert(false, "hashCode not designed");
  	  	    return 1; // any arbitrary constant will do 
        }

        public override string ToString()
        {
            return _instanceName + "; isInstantiated=" + IsInstanciated() + "; props=" + _rawProps.Keys;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Object Clone()
        {
            var ps = (PropertySheet)new Object();

            ps._registeredProperties = new HashMap<String, S4PropWrapper>(_registeredProperties);
            ps._propValues = new HashMap<string, Object>(_propValues);

            ps._rawProps = new HashMap<string, Object>(_rawProps);

            // make deep copy of raw-lists
            foreach (var regProp in ps.GetRegisteredProperties()) 
            {
                if (GetType(regProp) == PropertyType.ComponentList) 
                {
                    ps._rawProps.Put(regProp, ConfigurationManagerUtils.ToStringList(_rawProps[regProp]));
                    ps._propValues.Put(regProp, null);
                }
            }

            ps._cm = _cm;
            ps._owner = null;
            ps.InstanceName = InstanceName;

            return ps;
        }

        /// <summary>
        /// Validates a configuration, by ensuring that only valid property-names have been used to configure the component.
        /// </summary>
        /// <returns></returns>
        public Boolean Validate() 
        {
            foreach (var propName in _rawProps.Keys) 
            {
                if (propName.Equals(ConfigurationManagerUtils.GlobalCommonLoglevel))
                    continue;

                if (!_registeredProperties.ContainsKey(propName))
                    return false;
            }

            return true;
        }

        public string GetInstanceName()
        {
            return _instanceName;
        }

        public bool IsInstanciated()
        {
            return _owner != null;
        }
    }
}
