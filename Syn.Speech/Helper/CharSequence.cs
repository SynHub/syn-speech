using System.Text;

namespace Syn.Speech.Helper
{
    public class CharSequence
    {
        public static implicit operator CharSequence(string str)
        {
            return new StringCharSequence(str);
        }

        public static implicit operator CharSequence(StringBuilder str)
        {
            return new StringCharSequence(str.ToString());
        }
    }

    class StringCharSequence : CharSequence
    {
        readonly string str;

        public StringCharSequence(string str)
        {
            this.str = str;
        }

        public override string ToString()
        {
            return str;
        }
    }
}
