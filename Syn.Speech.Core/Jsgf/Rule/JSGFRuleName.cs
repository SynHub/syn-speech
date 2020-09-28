using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Rule
{
    public class JSGFRuleName : JSGFRule
    {

        protected String FullRuleName;

        protected String PackageName;
        protected String SimpleGrammarName;
        protected String SimpleRuleName;

        public String ResolvedRuleName;

        public static JSGFRuleName Null = new JSGFRuleName("NULL");

        public static JSGFRuleName Void = new JSGFRuleName("VOID");

        public JSGFRuleName() : this("NULL")
        {

        }

        public JSGFRuleName(String name)
        {
            // System.out.println ("Building rule name " + name);
            SetRuleName(name);
        }

        public String GetFullGrammarName()
        {
            // System.out.println ("Getting full grammar name from " + fullRuleName);
            if (PackageName != null)
            {
                return PackageName + "." + SimpleGrammarName;
            }
            // System.out.println ("Result is " + simpleGrammarName);
            return SimpleGrammarName;
        }

        public String GetPackageName()
        {
            return PackageName;
        }

        public String GetRuleName()
        {
            return FullRuleName;
        }

        public String GetSimpleGrammarName()
        {
            return SimpleGrammarName;
        }

        public String GetSimpleRuleName()
        {
            return SimpleRuleName;
        }

        public bool IsLegalRuleName()
        {
            return IsLegalRuleName(FullRuleName);
        }

        public static bool IsLegalRuleName(String name)
        {
            if (name == null)
            {
                return false;
            }
            name = StripRuleName(name);

            if (name.EndsWith(".*"))
            {
                name = name.Substring(0, name.Length - 2);
            }

            if (name.Length == 0)
            {
                return false;
            }

            if ((name.StartsWith(".")) || (name.EndsWith("."))
                    || (name.IndexOf("..") >= 0))
            {
                return false;
            }

            StringTokenizer localStringTokenizer = new StringTokenizer(name, ".");

            while (localStringTokenizer.hasMoreTokens())
            {
                String str = localStringTokenizer.nextToken();

                int i = str.Length;

                if (i == 0)
                    return false;

                for (int j = 0; j < i; ++j)
                {
                    if (!(IsRuleNamePart(str[j])))
                        return false;
                }
            }
            return true;
        }

        public static bool IsRuleNamePart(char c)
        {
            if (Java.IsValidIdentifier(c))//TODO: Check Behaviour
            {
                return true;
            }
            return ((c == '!') || (c == '#') || (c == '%')
                    || (c == '&') || (c == '(')
                    || (c == ')') || (c == '+')
                    || (c == ',') || (c == '-')
                    || (c == '/') || (c == ':')
                    || (c == ';') || (c == '=')
                    || (c == '@') || (c == '[')
                    || (c == '\\') || (c == ']')
                    || (c == '^') || (c == '|') || (c == '~'));
        }

        public void SetRuleName(String ruleName)
        {
            String strippedName = StripRuleName(ruleName);
            FullRuleName = strippedName;

            int j = strippedName.LastIndexOf('.');

            if (j < 0)
            {
                PackageName = null;
                SimpleGrammarName = null;
                SimpleRuleName = strippedName;
            }
            else
            {
                int i = strippedName.LastIndexOf('.', j - 1);

                if (i < 0)
                {
                    PackageName = null;
                    SimpleGrammarName = strippedName.Substring(0, j);
                    SimpleRuleName = strippedName.Substring(j + 1);
                }
                else
                {
                    PackageName = strippedName.Substring(0, i);
                    SimpleGrammarName = strippedName.Substring(i + 1, j);
                    SimpleRuleName = strippedName.Substring(j + 1);
                }
            }
        }

        public static String StripRuleName(String name)
        {
            if ((name.StartsWith("<")) && (name.EndsWith(">")))
            {
                return name.Substring(1, name.Length - 1);
            }
            return name;
        }


        public override String ToString()
        {
            return "<" + FullRuleName + ">";
        }
    }
}
