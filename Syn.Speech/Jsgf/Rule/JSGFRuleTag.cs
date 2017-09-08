using System;
using System.Text;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Rule
{
    public class JSGFRuleTag : JSGFRule
    {
        private JSGFRule _rule;
        private String _tag;

        public JSGFRuleTag()
        {
            Rule = null;
            Tag = null;
        }

        public JSGFRuleTag(JSGFRule rule, String tag)
        {
            Rule = rule;
            Tag = tag;
        }

        private static String EscapeTag(String tag)
        {
            var stringBuilder = new StringBuilder(tag);

            if ((tag.IndexOf('}') >= 0) || (tag.IndexOf('\\') >= 0)
                    || (tag.IndexOf('{') >= 0))
            {
                for (int i = stringBuilder.Length - 1; i >= 0; --i)
                {
                    int j = stringBuilder[i];
                    if ((j == '\\') || (j == '}') || (j == '{'))
                    {
                        stringBuilder.Insert(i, '\\');
                    }
                }
            }
            return stringBuilder.ToString();
        }

        public JSGFRule Rule
        {
            get { return _rule; }
            set { this._rule = value; }
        }

        public string Tag
        {
            get { return _tag; }
            set
            {
                if (value == null)
                    this._tag = "";
                else
                    this._tag = value;
            }
        }


        public override String ToString()
        {
            String str = " {" + EscapeTag(_tag) + "}";

            if ((_rule is JSGFRuleToken) || (_rule is JSGFRuleName))
            {
                return _rule + str;
            }
            return "(" + _rule + ")" + str;
        }
    }
}
