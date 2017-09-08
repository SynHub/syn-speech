using System;
using System.Text.RegularExpressions;

namespace Syn.Speech.Helper
{
    internal class Matcher
    {
        private int current;
        internal MatchCollection matches;
        internal readonly Regex Regex;
        private string _inputString;
        private string _patternString;


        internal Matcher(Regex regex, string inputString, string patternString)
        {
            this.Regex = regex;
            this._inputString = inputString;
            this._patternString = patternString;
        }

        public int End()
        {
            if ((matches == null) || (current >= matches.Count))
            {
                throw new InvalidOperationException();
            }
            return (matches[current].Index + matches[current].Length);
        }

        public bool Find()
        {
            if (matches == null)
            {
                matches = Regex.Matches(_inputString);
                current = 0;
            }
            return (current < matches.Count);
        }

        public bool Find(int index)
        {
            matches = Regex.Matches(_inputString, index);
            current = 0;
            return (matches.Count > 0);
        }

        public string Group(int n)
        {
            if ((matches == null) || (current >= matches.Count))
            {
                throw new InvalidOperationException();
            }
            Group grp = matches[current].Groups[n];
            return grp.Success ? grp.Value : null;
        }

        public bool Matches()
        {
            var regex = new Regex(string.Format("^{0}$", _patternString));
            return regex.IsMatch(_inputString);
        }

        public string ReplaceFirst(string txt)
        {
            return Regex.Replace(_inputString, txt, 1);
        }

        public Matcher Reset(CharSequence str)
        {
            return Reset(str.ToString());
        }

        public Matcher Reset(string str)
        {
            matches = null;
            this._inputString = str;
            return this;
        }

        public int Start()
        {
            if ((matches == null) || (current >= matches.Count))
            {
                throw new InvalidOperationException();
            }
            return matches[current].Index;
        }
    }
}
