using System;
using System.Collections.Generic;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Jsgf.Rule;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{
    public class GrXMLHandler : DefaultHandler
    {
        protected readonly HashMap<String, JSGFRule> TopRuleMap;
        JSGFRule _currentRule;
        URL _baseUrl;

        public GrXMLHandler(URL baseUrl, HashMap<String, JSGFRule> rules)
        {
            _baseUrl = baseUrl;
            TopRuleMap = rules;
            //this.logger = logger;
        }

        public override void StartElement(URL uri, string localName, string qName, Attributes attributes)
        {
            JSGFRule newRule = null;
            JSGFRule topRule = null;

            this.LogInfo("Starting element " + qName);
            if (qName.Equals("rule"))
            {
                String id = attributes.getValue("id");
                if (id != null)
                {
                    newRule = new JSGFRuleSequence(new List<JSGFRule>());
                    TopRuleMap.Put(id, newRule);
                    topRule = newRule;
                }
            }
            if (qName.Equals("item"))
            {
                String repeat = attributes.getValue("repeat");
                if (repeat != null)
                {
                    newRule = new JSGFRuleSequence(new List<JSGFRule>());
                    JSGFRuleCount ruleCount = new JSGFRuleCount(newRule, JSGFRuleCount.OnceOrMore);
                    topRule = ruleCount;
                }
                else
                {
                    newRule = new JSGFRuleSequence(new List<JSGFRule>());
                    topRule = newRule;
                }
            }
            if (qName.Equals("one-of"))
            {
                newRule = new JSGFRuleAlternatives(new List<JSGFRule>());
                topRule = newRule;
            }
            AddToCurrent(newRule, topRule);

        }

        public override void Characters(char[] buf, int offset, int len)
        {
            String item = new String(buf, offset, len).Trim();

            if (item.Length == 0)
                return;

            this.LogInfo("Processing text " + item);

            JSGFRuleToken newRule = new JSGFRuleToken(item);
            AddToCurrent(newRule, newRule);
            // Don't shift current
            _currentRule = newRule.Parent;
        }

        private void AddToCurrent(JSGFRule newRule, JSGFRule topRule)
        {

            if (newRule == null)
                return;

            if (_currentRule == null)
            {
                _currentRule = newRule;
                return;
            }

            if (_currentRule is JSGFRuleSequence)
            {
                JSGFRuleSequence ruleSequence = (JSGFRuleSequence)_currentRule;
                ruleSequence.Append(topRule);
                newRule.Parent = _currentRule;
                _currentRule = newRule;
            }
            else if (_currentRule is JSGFRuleAlternatives)
            {
                JSGFRuleAlternatives ruleAlternatives = (JSGFRuleAlternatives)_currentRule;
                ruleAlternatives.Append(topRule);
                newRule.Parent = _currentRule;
                _currentRule = newRule;
            }
        }

        public override void EndElement(URL uri, string localName, string qName)
        {
            this.LogInfo("Ending element " + qName);

            if (qName.Equals("item") || qName.Equals("one-of") || qName.Equals("rule"))
                _currentRule = _currentRule.Parent;
        }

        public override void Error(SAXParseException exception)
        {
            this.LogError(exception.Message);
        }
    }
}
