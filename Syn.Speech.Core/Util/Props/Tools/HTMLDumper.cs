using System;
using System.Collections;
using System.IO;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props.Tools
{
    /// <summary>
    /// Dumps a given configuration manager to HTML.
    /// @author Holger Brandl
    /// </summary>
    public class HTMLDumper
    {
        /// <summary>
        /// Dumps the config as a set of HTML tables.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="path">where to output the HTML.</param>
        public static void ShowConfigAsHTML(ConfigurationManager configurationManager, String path)
        {
            var streamWriter = new StreamWriter(path);
            DumpHeader(streamWriter);
            foreach (var componentName in configurationManager.GetInstanceNames(typeof(IConfigurable)))
            {
                DumpComponentAsHTML(streamWriter, componentName, configurationManager.GetPropertySheet(componentName));
            }
            DumpFooter(streamWriter);
            streamWriter.Close();
        }


        /// <summary>
        /// Dumps the footer for HTML output.
        /// </summary>
        /// <param name="streamWriter">The output stream.</param>
        public static void DumpFooter(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("</body>");
            streamWriter.WriteLine("</html>");
        }

        /// <summary>
        /// Dumps the header for HTML output.
        /// </summary>
        /// <param name="streamWriter">The output stream.</param>
        public static void DumpHeader(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("<html><head>");
            streamWriter.WriteLine("    <title> Sphinx-4 Configuration</title");
            streamWriter.WriteLine("</head>");
            streamWriter.WriteLine("<body>");
        }

        /// <summary>
        /// Dumps the given component as HTML to the given stream.
        /// </summary>
        /// <param name="streamWriter">Where to dump the HTML.</param>
        /// <param name="name">The name of the component to dump.</param>
        /// <param name="properties">The properties.</param>
        public static void DumpComponentAsHTML(StreamWriter streamWriter, String name, PropertySheet properties)
        {
            streamWriter.WriteLine("<table border=1>");
            //        out.println("<table border=1 width=\"%80\">");
            streamWriter.Write("    <tr><th bgcolor=\"#CCCCFF\" colspan=2>");
            //       out.print("<a href="")
            streamWriter.Write(name);
            streamWriter.Write("</a>");
            streamWriter.WriteLine("</td></tr>");

            streamWriter.WriteLine("    <tr><th bgcolor=\"#CCCCFF\">Property</th><th bgcolor=\"#CCCCFF\"> Value</th></tr>");
            var propertyNames = properties.GetRegisteredProperties();

            foreach (var propertyName in propertyNames)
            {
                streamWriter.Write("    <tr><th align=\"leftt\">" + propertyName + "</th>");
                Object obj;
                obj = properties.GetRaw(propertyName);
                if (obj is String)
                {
                    streamWriter.WriteLine("<td>" + obj + "</td></tr>");
                }
                else if (obj is IList)
                {
                    var l = (IList)obj;
                    streamWriter.WriteLine("    <td><ul>");
                    foreach (var listElement in l)
                    {
                        streamWriter.WriteLine("        <li>" + listElement + "</li>");
                    }
                    streamWriter.WriteLine("    </ul></td>");
                }
                else
                {
                    streamWriter.WriteLine("<td>DEFAULT</td></tr>");
                }
            }
            streamWriter.WriteLine("</table><br>");
        }
    }
}
