using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Alignment;
using Syn.Speech.Util;

namespace Syn.Speech.Test.Alignment
{
    [TestClass]
    public class UsEnglishWordExpanderTest
    {
          private static readonly Dictionary<string,string> TestData = new Dictionary<string, string>
            {
                 {"# . no, $ convertion.", ". no $ convertion"}, 
                 {"1, 2 3", "one two three"},
                 {"the answer is 42,", "the answer is forty two"}, 
                 {"587", "five hundred eighty seven"},
                 {"1903", "one thousand nine hundred three"}, 
                 {"12011", "twelve thousand eleven"}, 
                 {"126166", "one hundred twenty six thousand one hundred sixty six"},
                 {"9 3/4", "nine and three fourth 's"},
                 {"October 1st", "october first"},
                 {"May the 4th be with you", "may the fourth be with you"},
                 {"7-11", "seven to eleven"}, 
                 {"12, 35", "twelve thirty five"},
                 {"146%", "one hundred forty six percent"}, 
                 {"320'000", "three hundred twenty thousand"},
                 {"120,000", "one hundred twenty thousand"},
                 {"$35,000", "thirty five thousand dollars"}, 
                 {"$1000000", "one million dollars"}, 
                 {"U.S. economy", "u s economy"}, 
                 {"sweet home Greenbow, AL.", "sweet home greenbow alabama"},
                 {"Henry I", "henry the first"}, 
                 {"Chapter XVII", "chapter seventeen"}, 
                 {"don't, doesn't, won't, can't", "don't doesn't won't can't"}, 
                 {"I've we've", "i've we've"}, 
                 {"I've we've it's", "i've we've it's"}, 
                 {"Classics of 80s", "classics of eighties"}, 
                 {"In 1880s", "in eighteen eighties"}, 
                 {"Mulholland Dr.", "mulholland drive"}, 
                 {"dr. Jekyll and Mr. Hyde.","doctor jekyll and mister hyde"},
                 {"Mr. & Mrs. smith", "mister and missus smith"}, 
                 {"St. Louis Cardinals", "saint louis cardinals"}, 
                 {"St. Elmo's fire", "saint elmo's fire"}, 
                 {"elm st.", "elm street"},};

        
        private readonly ITextTokenizer _expander  = new UsEnglishTokenizer();


        [TestMethod]
        public void WordExpander_TextToWords()
        {
            foreach (var item in TestData.Keys)
            {
                var tokens = _expander.Expand(item);
                Assert.AreEqual(Utilities.Join(tokens), TestData[item]);
            }
        }
    }
}
