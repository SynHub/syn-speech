using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Rule
{

    public class JSGFRuleCount : JSGFRule
    {
        private int _count;

        public static int Optional = 2;
        public static int OnceOrMore = 3;
        public static int ZeroOrMore = 4;

        public JSGFRuleCount()
        {
            Rule = null;
            Count = Optional;
        }

        public JSGFRuleCount(JSGFRule rule, int count)
        {
            Rule = rule;
            Count = count;
        }

        public int Count
        {
            get { return _count; }
            set
            {
                if ((value != Optional) && (value != ZeroOrMore)
                    && (value != OnceOrMore))
                {
                    return;
                }
                this._count = value;
            }
        }

        public JSGFRule Rule { get; set; }


        public override String ToString()
        {
            if (_count == Optional)
            {
                return '[' + Rule.ToString() + ']';
            }
            String str = null;

            if ((Rule is JSGFRuleToken) || (Rule is JSGFRuleName))
                str = Rule.ToString();
            else
            {
                str = '(' + Rule.ToString() + ')';
            }

            if (_count == ZeroOrMore)
                return str + " *";
            if (_count == OnceOrMore)
            {
                return str + " +";
            }
            return str + "???";
        }
    }

}
