using System;
using System.IO;
using System.Xml;
using Syn.Speech.Helper;
using Syn.Speech.Jsgf.Rule;

//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{

    /**
     * Grammar for GrXML W3C Standard
     * @author shmyrev
     *
     */
    public class GrXMLGrammar : JSGFGrammar
    {

        HashMap<String, JSGFRule> _rules;

        protected void LoadXML()
        {
            try
            {
                var xr = new XMLReader();
                _rules = new HashMap<string, JSGFRule>();
                var handler = new GrXMLHandler(BaseUrl, _rules);
                xr.setContentHandler(handler);
                xr.setErrorHandler(handler);
                xr.parse(BaseUrl);
                //is.close();
            }
            catch (XmlException e)
            {
                var msg = "Error while parsing line " + e.LineNumber + " of " + BaseUrl + ": " + e.Message;
                throw new IOException(msg);
            }
        }

        /**
         * Commit changes to all loaded grammars and all changes of grammar since
         * the last commitChange
         * 
         * @throws JSGFGrammarParseException
         * @throws JSGFGrammarException
         */

        public override void CommitChanges()
        {
            try
            {
                if (LoadGrammar)
                {
                    if (Manager == null)
                        GetGrammarManager();
                    LoadXML();
                    LoadGrammar = false;
                }

                RuleStack = new RuleStack();
                NewGrammar();

                FirstNode = CreateGrammarNode("<sil>");
                var finalNode = CreateGrammarNode("<sil>");
                finalNode.SetFinalNode(true);

                // go through each rule and create a network of GrammarNodes
                // for each of them

                foreach (var entry in _rules)
                {
                    var publicRuleGraph = new GrammarGraph(this);
                    RuleStack.Push(entry.Key, publicRuleGraph);
                    var graph = ProcessRule(entry.Value);
                    RuleStack.Pop();

                    FirstNode.Add(publicRuleGraph.StartNode, 0.0f);
                    publicRuleGraph.EndNode.Add(finalNode, 0.0f);
                    publicRuleGraph.StartNode.Add(graph.StartNode,
                            0.0f);
                    graph.EndNode.Add(publicRuleGraph.EndNode, 0.0f);
                }
                PostProcessGrammar();
            }
            catch (UriFormatException mue)
            {
                throw new IOException("bad base grammar URL " + BaseUrl + ' ' + mue);
            }
        }

    }

}
