using System.Collections.Generic;
using System.Text.RegularExpressions;
//PATROLLED + REFACTORED
using Syn.Speech.Helper;

namespace Syn.Speech.Alignment
{
    public class SimpleTextTokenizer : ITextTokenizer
    {
        public virtual List<string> Expand(string text)
        {
            text = text.Replace('’', '\'');
            text = text.Replace('‘', ' ');
            text = text.Replace('”', ' ');
            text = text.Replace('“', ' ');
            text = text.Replace('"', ' ');
            text = text.Replace('»', ' ');
            text = text.Replace('«', ' ');
            text = text.Replace('–', '-');
            text = text.Replace('—', ' ');
            text = text.Replace('…', ' ');

            text = text.Replace(" - ", " ");
            //text = text.ReplaceAll("[,.?:!;?()/_*%]", " ");
            text = text.ReplaceAll("[/_*%]", " ");
            text = text.ToLower();

            return Arrays.AsList(Regex.Split(text, "[.,?:!;()]"));
        }
    }
}
