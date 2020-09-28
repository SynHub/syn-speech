using System;
using System.Collections.Generic;
using System.Text;
//REFACTORED
namespace Syn.Speech.Jsgf.Rule
{
    public class JSGFRuleSequence : JSGFRule
    {
        public JSGFRuleSequence()
        {
            Rules = null;
        }

        public JSGFRuleSequence(List<JSGFRule> rules)
        {
            Rules = rules;
        }

        public void Append(JSGFRule rule)
        {
            if (Rules == null)
            {
                throw new NullReferenceException("null rule to append");
            }
            Rules.Add(rule);
        }

        public List<JSGFRule> Rules { get; set; }


        public override String ToString()
        {
            if (Rules.Count == 0)
            {
                return "<NULL>";
            }
            var sb = new StringBuilder();
            for (int i = 0; i < Rules.Count; ++i)
            {
                if (i > 0)
                    sb.Append(' ');

                JSGFRule r = Rules[i];
                if ((r is JSGFRuleAlternatives) || (r is JSGFRuleSequence))
                    sb.Append("( ").Append(r).Append(" )");
                else
                {
                    sb.Append(r);
                }
            }
            return sb.ToString();
        }
    }
}
