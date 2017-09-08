using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// Some static utility methods which ease the handling of system configurations.
    /// </summary>
    public class ConfigurationManagerUtils
    {
        /// <summary>
        /// this pattern matches strings of the form '${word}' 
        /// </summary>
        private static Regex _globalSymbolPattern = new Regex(@"\$\{(?<id>\w+)\}");

        /// <summary>
        /// A common property (used by all components) that sets the log level for the component.
        /// </summary>
        public static string GlobalCommonLoglevel = "logLevel";

        /// <summary>
        /// The default file suffix of configuration files.
        /// </summary>
        public static string CMFileSuffix = ".sxl";

        /// <summary>
        /// disabled constructor because the class is just a collection of utilities for handling system configurations
        /// </summary>
        private ConfigurationManagerUtils()
        {
        }

        /// <summary>
        /// Validates that only annotated property names have been used to setup this instance of {@code
        /// util.props.ConfigurationManager}
        /// </summary>
        /// <param name="cm"></param>
        /// <returns>{@code true} if it is a valid configuration.</returns>
        public Boolean ValidateConfiguration(ConfigurationManager cm)
        {
            foreach (var compName in cm.GetComponentNames())
            {
                if (!cm.GetPropertySheet(compName).Validate())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Strips the ${ and } off of a global symbol of the form ${symbol}.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string StripGlobalSymbol(String symbol)
        {
            //MatchCollection matcher = globalSymbolPattern.Matches(symbol);
            //if (matcher.Count>0)
            //{
            //    return matcher[0].Groups[0].Value;
            //}
            //else
            //{
            //    return symbol;
            //}

            return symbol.Replace("${", String.Empty).Replace("}", String.Empty);

        }

        public static List<string> ToStringList(Object obj)
        {
            var result = new List<string>();
            if (!(obj is List<object>))
                return null;

            foreach (var o in (List<object>)obj)
            {
                if (o != null)
                {
                    result.Add(o.ToString());
                }
            }
            return result;
        }

        public static Uri GetUrl(String file)
        {
            try
            {
                return new Uri(file);
            }
            catch (UriFormatException e)
            {
                throw e;
            }

            return null;
        }

        /// <summary>
        /// the replacement of xml/sxl suffix is not necessary and just done to improve readability
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        public static string GetLogPrefix(ConfigurationManager cm)
        {
            if (cm.ConfigUrl != null)
                return cm.ConfigUrl.File.FullName.Replace(".sxl", "").Replace(".xml", "") + '.';
            else
                return "S4CM.";
        }

        /// <summary>
        ///  Shows the current configuration
        /// </summary>
        /// <param name="cm">The cm.</param>
        public static void ShowConfig(ConfigurationManager cm)
        {
            Console.Out.WriteLine(" ============ config ============= ");
            foreach (var allName in cm.GetInstanceNames(typeof(IConfigurable)))
            {
                ShowConfig(cm, allName);
            }
        }

        /// <summary>
        /// Show the configuration for the component with the given name.
        /// </summary>
        /// <param name="cm">The Configuration Manager.</param>
        /// <param name="name">The component name.</param>
        public static void ShowConfig(ConfigurationManager cm, String name)
        {
            //        Symbol symbol = cm.getsymbolTable.get(name);

            if (!cm.GetComponentNames().Contains(name))
            {
                Console.Out.WriteLine("No component: " + name);
                return;
            }
            Console.Out.WriteLine(name + ':');

            var properties = cm.GetPropertySheet(name);

            foreach (var propertyName in properties.GetRegisteredProperties())
            {
                Console.Out.WriteLine("    " + propertyName + " = ");
                Object obj;
                obj = properties.GetRaw(propertyName);
                if (obj is String)
                {
                    Console.Out.WriteLine(obj);
                }
                else if (obj is IList)
                {
                    var l = (IList)obj;
                    for (var k = l.GetEnumerator(); k.MoveNext(); )
                    { //TODO: Check Behaviour
                        Console.Out.WriteLine(k.Current);
                        if (k.MoveNext())
                        {
                            Console.Out.WriteLine(", ");
                        }
                    }
                    Console.Out.WriteLine();
                }
                else
                {
                    Console.Out.WriteLine("[DEFAULT]");
                }
            }
        }

        /// <summary>
        /// Applies the system properties to the raw property map. System properties should be of the form
        /// compName[paramName]=paramValue
        /// </summary>
        /// <param name="rawMap"></param>
        /// <param name="global"></param>
        public static void ApplySystemProperties(Dictionary<string, RawPropertyData> rawMap, Dictionary<string, string> global)
        {

            // search for parameters of the form component[parameter]=value
            // these go in the property sheet for the component

            // look for parameters of the form foo=bar
            // these go in the global map

            var envVariables = Environment.GetEnvironmentVariables();
            if (envVariables != null)
            {
                foreach (DictionaryEntry propertyInfo in envVariables)
                {
                    global.Add(propertyInfo.Key.ToString(), propertyInfo.Value.ToString());
                }
            }
            //    Properties props = System.getProperties();
            //    for (Enumeration<?> e = props.keys(); e.hasMoreElements();) {
            //        string param = (String) e.nextElement();
            //        string value = props.getProperty(param);

            //        int lb = param.indexOf('[');
            //        int rb = param.indexOf(']');

            //        if (lb > 0 && rb > lb) {
            //            string compName = param.substring(0, lb);
            //            string paramName = param.substring(lb + 1, rb);
            //            RawPropertyData rpd = rawMap.get(compName);
            //            if (rpd != null) {
            //                rpd.add(paramName, value);
            //            } else {
            //                throw new InternalConfigurationException(compName, param,
            //                        "System property attempting to set parameter "
            //                                + " for unknown component " + compName
            //                                + " (" + param + ')');
            //            }
            //        }


            //        else if (param.indexOf('.') == -1) {
            //            global.put(param, value);
            //        }
            //    }
        }

        /// <summary>
        /// Gets a resource associated with the given parameter name given an property sheet.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="ps">The property sheet which contains the property.</param>
        /// <param name="fillWithLocalPath">if set to <c>true</c> [fill with local path].</param>
        /// <returns>The resource associated with the name or NULL if it doesn't exist.</returns>//TODO: CHANGE TO RETURN URL TYPE
        public static URL GetResource(String name, PropertySheet ps, Boolean fillWithLocalPath = true)
        {

            var location = ps.GetString(name);
            if (location == null)
            {
                throw new InternalConfigurationException(ps.InstanceName, name, "Required resource property '" + name + "' not set");
            }

            try
            {
                var url = ResourceToUrl(location);

                if (url == null)
                {
                    throw new InternalConfigurationException(ps.InstanceName, name, "Can't locate " + location);
                }
                return url;
            }
            catch (Exception e)
            {
                throw new InternalConfigurationException(e, ps.InstanceName, name, "Bad URL " + location + e.Message);
            }

        }

        //static Pattern jarPattern = Pattern.Compile("resource:(.*)", Pattern.CASE_INSENSITIVE);
        //TODO: FIND BETTER REPLACEMENT FOR THIS FUNCTION
        public static URL ResourceToUrl(String location)
        {
            return new URL(location);
        }


        /// <summary>
        /// Why do we need this method? The reason is, that we would like to avoid this method to be part of the
        /// <code>PropertySheet</code>-API. In some circumstances it is nevertheless required to get access to the managing
        /// <code>ConfigurationManager</code>.
        /// </summary>
        /// <param name="ps">The ps.</param>
        /// <returns></returns>
        public static ConfigurationManager GetPropertyManager(PropertySheet ps)
        {
            return ps.GetPropertyManager();
        }

        public static bool IsImplementingInterface(Type aClass, Type interfaceClass)
        {
            Debug.Assert(interfaceClass.IsInterface);

            var superClass = aClass.BaseType;
            if (superClass != null && IsImplementingInterface(superClass, interfaceClass))
                return true;

            foreach (var curInterface in aClass.GetInterfaces())
            {
                if (curInterface == interfaceClass || IsImplementingInterface(curInterface, interfaceClass))
                    return true;
            }

            return false;
        }

        public static bool IsSubClass(Type aClass, Type possibleBaseClass)
        {
            while (aClass != null && !(aClass == typeof(Object)))
            {
                aClass = aClass.BaseType;

                if (aClass != null && aClass == possibleBaseClass)
                    return true;
            }

            return false;
        }


        /// <summary>
        ///  <code>true</code> if <code>aClass</code> is either equal to <code>poosibleParent</code>, 
        /// a subclass of it, or implements it if <code>possibleParent</code> is an interface.
        /// </summary>
        public static bool IsDerivedClass(Type derived, Type parent)
        {
            return parent.IsAssignableFrom(derived);
        }

        public static Dictionary<string, List<PropertySheet>> ListAllsPropNames(ConfigurationManager cm)
        {
            var allProps = new Dictionary<string, List<PropertySheet>>();
            foreach (var configName in cm.GetComponentNames())
            {
                var ps = cm.GetPropertySheet(configName);
                foreach (var propName in ps.GetRegisteredProperties())
                {
                    if (!allProps.ContainsKey(propName))
                    {
                        allProps.Put(propName, new List<PropertySheet>());
                    }
                    allProps[propName].Add(ps);
                }
            }

            return allProps;
        }

        public static void SetProperty(ConfigurationManager cm, string propName, string propValue)
        {
            Debug.Assert(propValue != null);

            var allProps = ListAllsPropNames(cm);
            var configurableNames = cm.GetComponentNames();

            if (!allProps.ContainsKey(propName) && !propName.Contains("->") && !configurableNames.Contains(propName))
            {
                throw new Exception("No property or configurable " + propName + " in configuration " + cm.ConfigUrl + "!");
            }

            if (configurableNames.Contains(propName))
            {
                //var confClass = Activator.CreateInstance() as IConfigurable;
                SetClass(cm.GetPropertySheet(propName), Type.GetType(propValue, true));

                return;
            }

            if (!propName.Contains("->") && allProps[propName].Count > 1)
            {
                throw new Exception("Property-name: " + propName + " is ambiguous with respect to configuration " + cm.ConfigUrl + ". User componentName->propName to disambiguate your request");
            }

            string componentName;

            if (propName.Contains("->"))
            {
                var splitProp = propName.Split(new[] { "->" }, StringSplitOptions.None);
                componentName = splitProp[0];
                propName = splitProp[1];
            }
            else
            {
                componentName = allProps[propName][0].GetInstanceName();
            }

            SetProperty(cm, componentName, propName, propValue);
        }

        public static void SetProperty(ConfigurationManager cm, string componentName, string propName, string propValue)
        {

            // now set the property
            var ps = cm.GetPropertySheet(componentName);
            if (ps == null)
                throw new SystemException("Component '" + propName + "' is not registered to this system configuration '");

            // set the value to null if the string content is 'null
            if (propValue.Equals("null"))
                propValue = null;

            switch (ps.GetType(propName))
            {
                case PropertyType.Boolean:
                    ps.SetBoolean(propName, Convert.ToBoolean(propValue));
                    break;
                case PropertyType.Double:
                    ps.SetDouble(propName, Convert.ToDouble(propValue, CultureInfo.InvariantCulture.NumberFormat));
                    break;
                case PropertyType.Int:
                    ps.SetInt(propName, Convert.ToInt32(propValue, CultureInfo.InvariantCulture.NumberFormat));
                    break;
                case PropertyType.String:
                    ps.SetString(propName, propValue);
                    break;
                case PropertyType.Component:
                    ps.SetComponent(propName, propValue, null);
                    break;
                case PropertyType.ComponentList:
                    var compNames = new List<String>();
                    if (propValue != null)
                        foreach (var component in propValue.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            compNames.Add(component.Trim());
                        }

                    ps.SetComponentList(propName, compNames, null);
                    break;
                default:
                    throw new SystemException("unknown property-type");
            }
        }

        public static void SetClass(PropertySheet ps, Type confClass)
        {
            if (ps.IsInstanciated())
            {
                throw new Exception("configurable " + ps.GetInstanceName() + " has already been instantiated");
            }

            ps.SetConfigurableClass(confClass);
        }

        public static void Save(ConfigurationManager cm, FileInfo cmLocation)
        {
            if (!cmLocation.Name.EndsWith(CMFileSuffix))
                Console.WriteLine("WARNING: Serialized s4-configuration should have the suffix '" + CMFileSuffix + '\'');

            Debug.Assert(cm != null);
            try
            {
                var pw = new StreamWriter(cmLocation.FullName, false, Encoding.UTF8);
                var configXML = ToXML(cm);
                pw.Write(configXML);
                pw.Flush();
                pw.Close();
            }
            catch (FileNotFoundException e1)
            {
                e1.PrintStackTrace();
            }
        }


        /// <summary>
        /// Converts a configuration manager instance into a xml-string .
        /// </summary>
        /// <param name="cm">The cm.</param>
        ///<remarks>This methods will not instantiate configurables.</remarks>
        public static String ToXML(ConfigurationManager cm)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n");
            sb.Append("\n<!--    Sphinx-4 Configuration file--> \n\n");

            sb.Append("<config>");

            var pattern = Pattern.Compile("\\$\\{(\\w+)\\}");

            var globalProps = cm.GetGlobalProperties();
            foreach (var entry in globalProps)
            {
                var propName = entry.Key;

                var matcher = pattern.Matcher(propName);
                propName = matcher.Matches() ? matcher.Group(1) : propName;

                sb.Append("\n\t<property name=\"").Append(propName).Append("\" value=\"").Append(entry.Value).Append("\"/>");
            }

            foreach (var instanceName in cm.GetComponentNames())
                sb.Append("\n\n").Append(PropSheet2XML(instanceName, cm.GetPropertySheet(instanceName)));

            sb.Append("\n</config>");
            return sb.ToString();
        }

        private static String PropSheet2XML(String instanceName, PropertySheet ps)
        {
            var sb = new StringBuilder();
            sb.Append("\t<component name=\"").Append(instanceName).Append("\" type=\"").Append(ps.GetConfigurableClass().GetType().Name).Append("\">");

            foreach (var propName in ps.GetRegisteredProperties())
            {
                var predec = "\n\t\t<property name=\"" + propName + "\" ";
                if (ps.GetRawNoReplacement(propName) == null)
                    continue;  // if the property was net defined within the xml-file

                switch (ps.GetType(propName))
                {
                    case PropertyType.ComponentList:
                        sb.Append("\n\t\t<propertylist name=\"").Append(propName).Append("\">");
                        var compNames = ToStringList(ps.GetRawNoReplacement(propName));
                        foreach (var compName in compNames)
                            sb.Append("\n\t\t\t<item>").Append(compName).Append("</item>");
                        sb.Append("\n\t\t</propertylist>");
                        break;
                    default:
                        sb.Append(predec).Append("value=\"").Append(ps.GetRawNoReplacement(propName)).Append("\"/>");
                        break;
                }
            }

            sb.Append("\n\t</component>\n\n");
            return sb.ToString();
        }
    }
}
