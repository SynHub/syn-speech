using System;
using System.Collections;
using System.IO;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props.Tools
{
    /// <summary>
    /// Dumps a given configuration manager as GDL.
    /// @author Holger Brandl
    /// </summary>
    public class GDLDumper
    {

        /// <summary>
        /// Dumps the given component as GDL to the given stream.
        /// </summary>
        /// <param name="cm">The cm.</param>
        /// <param name="out">Where to dump the GDL.</param>
        /// <param name="name">The name of the component to dump.</param>
        public static void DumpComponentAsGDL(ConfigurationManager cm, StreamWriter @out, String name)
        {

            @out.WriteLine("node: {title: \"" + name + "\" color: " + GetColor(cm, name) + '}');

            var ps = cm.GetPropertySheet(name);
            var propertyNames = ps.GetRegisteredProperties();

            foreach (var propertyName in propertyNames)
            {
                var propType = ps.GetType(propertyName);
                var val = ps.GetRaw(propertyName);

                if (val != null)
                {
                    if (propType == PropertyType.Component)
                    {
                        @out.WriteLine("edge: {source: \"" + name
                                + "\" target: \"" + val + "\"}");
                    }
                    else if (propType == PropertyType.ComponentList)
                    {
                        var list = (IList)val;
                        foreach (var listElement in list)
                        {
                            @out.WriteLine("edge: {source: \"" + name
                                    + "\" target: \"" + listElement + "\"}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dumps the config as a GDL plot.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="path">Where to output the GDL.</param>
        public static void ShowConfigAsGDL(ConfigurationManager configurationManager, String path)
        {
            var @out = new StreamWriter(path);
            DumpGDLHeader(@out);
            foreach (var componentName in configurationManager.GetInstanceNames(typeof(IConfigurable)))
            {
                DumpComponentAsGDL(configurationManager, @out, componentName);
            }
            DumpGDLFooter(@out);
            @out.Close();
        }


        /// <summary>
        /// Outputs the GDL header
        /// </summary>
        /// <param name="streamWriter">The output stream.</param>
        public static void DumpGDLHeader(StreamWriter streamWriter)
        {
            streamWriter.WriteLine(" graph: {title: \"unix evolution\" ");
            streamWriter.WriteLine("         layoutalgorithm: tree");
            streamWriter.WriteLine("          scaling        : 2.0");
            streamWriter.WriteLine("          colorentry 42  : 152 222 255");
            streamWriter.WriteLine("     node.shape     : ellipse");
            streamWriter.WriteLine("      node.color     : 42 ");
            streamWriter.WriteLine("node.height    : 32  ");
            streamWriter.WriteLine("node.fontname  : \"helvB08\"");
            streamWriter.WriteLine("edge.color     : darkred");
            streamWriter.WriteLine("edge.arrowsize :  6    ");
            streamWriter.WriteLine("node.textcolor : darkblue ");
            streamWriter.WriteLine("splines        : yes");
        }


        /**
         * Gets the color for the given component
         *
         * @param ConfigurationManager
         * @param componentName        the name of the component @return the color name for the component
         */
        public static String GetColor(ConfigurationManager configurationManager, String componentName)
        {
            try
            {
                var c = configurationManager.Lookup(componentName);
                var cls = c.GetType();
                if (cls.Name.IndexOf(".recognizers") > 1)
                { //TODO: Change this if namespace is renamed.
                    return "cyan";
                }
                if (cls.Name.IndexOf(".tools") > 1)
                {
                    return "darkcyan";
                }
                if (cls.Name.IndexOf(".decoders") > 1)
                { //TODO: Change this if namespace is renamed.
                    return "green";
                }
                if (cls.Name.IndexOf(".frontend") > 1)
                {
                    return "orange";
                }
                if (cls.Name.IndexOf(".acoustic") > 1)
                {
                    return "turquoise";
                }
                if (cls.Name.IndexOf(".linguist") > 1)
                {
                    return "lightblue";
                }
                if (cls.Name.IndexOf(".instrumentation") > 1)
                {
                    return "lightgrey";
                }
                if (cls.Name.IndexOf(".util") > 1)
                {
                    return "lightgrey";
                }
            }
            catch (PropertyException e)
            {
                return "black";
            }
            return "darkgrey";
        }



        /// <summary>
        /// Dumps the footer for GDL output.
        /// </summary>
        /// <param name="streamWriter">The output stream.</param>
        public static void DumpGDLFooter(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("}");
        }
    }

}
