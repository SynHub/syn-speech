using System.Text.RegularExpressions;

namespace Syn.Speech.Helper
{
    internal class Pattern
    {
        public const int CASE_INSENSITIVE = 1;
        public const int DOTALL = 2;
        public const int MULTILINE = 4;
        private readonly Regex regex;


        private Pattern(Regex r, string patterString)
        {
            regex = r;
            PatternString = patterString;
        }

        public static Pattern Compile(string pattern)
        {
            return new Pattern(new Regex(pattern, RegexOptions.Compiled),pattern);
        }

        public static Pattern Compile(string pattern, int flags)
        {
            var compiled = RegexOptions.Compiled;
            if ((flags & 1) != CASE_INSENSITIVE)
            {
                compiled |= RegexOptions.IgnoreCase;
            }
            if ((flags & 2) != DOTALL)
            {
                compiled |= RegexOptions.Singleline;
            }
            if ((flags & 4) != MULTILINE)
            {
                compiled |= RegexOptions.Multiline;
            }

            
            return new Pattern(new Regex(pattern, compiled),pattern);
        }

        public Matcher Matcher(string txt)
        {
            return new Matcher(regex, txt, PatternString);
        }

        public string PatternString { get; set; }
    }
}
