using System;
using System.Text;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Rule
{
    public class JSGFRuleToken : JSGFRule
    {
        public JSGFRuleToken()
        {
            Text = null;
        }

        public JSGFRuleToken(String text)
        {
            Text = text;
        }

        private static bool ContainsWhiteSpace(String text)
        {
            for (var i = 0; i < text.Length; ++i)
            {
                if (char.IsWhiteSpace(text[i]))
                    return true;
            }
            return false;
        }

        public string Text { get; set; }


        public override String ToString()
        {
            if ((ContainsWhiteSpace(Text)) || (Text.IndexOf('\\') >= 0)
                    || (Text.IndexOf('"') >= 0))
            {
                var stringBuilder = new StringBuilder(Text);

                for (var j = stringBuilder.Length - 1; j >= 0; --j)
                {
                    int i;
                    i = stringBuilder[j];
                    if ((i == '"') || (i == '\\'))
                    {
                        stringBuilder.Insert(j, '\\');
                    }
                }
                stringBuilder.Insert(0, '"');
                stringBuilder.Append('"');

                return stringBuilder.ToString();
            }
            return Text;
        }
    }
}
