using System.Globalization;
using System.IO;
using System.Text;

namespace Syn.Speech.Helper
{
    public class Scanner : StringReader
    {
        string _currentWord;

        public Scanner(string source) : base(source)
        {
            ReadNextWord();
        }

        private void ReadNextWord()
        {
            var sb = new StringBuilder();
            char nextChar;
            int next;
            do
            {
                next = Read();
                if (next < 0)
                    break;
                nextChar = (char)next;
                if (char.IsWhiteSpace(nextChar))
                    break;
                sb.Append(nextChar);
            } while (true);
            while ((Peek() >= 0) && (char.IsWhiteSpace((char)Peek())))
                Read();
            if (sb.Length > 0)
                _currentWord = sb.ToString();
            else
                _currentWord = null;
        }

        public bool HasNextInt()
        {
            if (_currentWord == null)
                return false;
            int dummy;
            return int.TryParse(_currentWord, out dummy);
        }

        public int NextInt()
        {
            try
            {
                return int.Parse(_currentWord, CultureInfo.InvariantCulture.NumberFormat);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public bool HasNextDouble()
        {
            if (_currentWord == null)
                return false;
            double dummy;
            return double.TryParse(_currentWord, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat,out dummy);
        }

        public double NextDouble()
        {
            try
            {
                return double.Parse(_currentWord, CultureInfo.InvariantCulture.NumberFormat);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public float NextFloat()
        {
            try
            {
                return float.Parse(_currentWord,CultureInfo.InvariantCulture.NumberFormat);
            }
            finally 
            {
                ReadNextWord();
            }
        }

        public string NextWord()
        {
            return _currentWord;
        }

        public bool HasNextFloat()
        {
            if (_currentWord == null) return false;
            float dummy;
            return float.TryParse(_currentWord, out dummy);
        }

        public bool HasNext()
        {
            return _currentWord != null;
        }
    }
}
