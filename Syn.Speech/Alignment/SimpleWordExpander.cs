using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
//PATROLLED
using Syn.Speech.Helper;

namespace Syn.Speech.Alignment
{
  public class SimpleWordExpander : IWordExpander
  {
      public virtual List<string> expand(string text)
    {
        text = text.Replace('’', '\'');
        text = text.Replace('‘', ' ');
        text = text.Replace('”', ' ');
        text = text.Replace('“', ' ');
        text = text.Replace('»', ' ');
        text = text.Replace('«', ' ');
        text = text.Replace('–', '-');
        text = text.Replace('—', ' ');
        text = text.Replace('…', ' ');

        text = text.Replace(" - ", " ");
        text = text.ReplaceAll("[,.?:!;?()/_*%]", " ");
        text = text.ToLower();
          
        return Arrays.asList(Regex.Split(text,"\\s+"));
    }
  }
}
