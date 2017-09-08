using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Language.Grammar;

namespace Syn.Speech.Test.Linguist
{
    [TestClass]
    public class FSTGrammarTest
    {
        [TestMethod]
        public void FSTGrammar_ForcedAlignGrammar()
        {
            var dictionaryUrl =new URL("FSTGrammarTest.dic");
            var noisedictUrl = new URL("noisedict");

            var dictionary = new TextDictionary(dictionaryUrl,
                                                       noisedictUrl,
                                                       null,
                                                       null,
                                                       new UnitManager());

            URL url =new URL("FSTGrammarTest.gram");
            var grammar = new FSTGrammar(url.Path,
                                                true,
                                                true,
                                                true,
                                                true,
                                                dictionary);
            grammar.Allocate();
            Assert.IsTrue(grammar.GrammarNodes.Count==14);
        }
    }
}
