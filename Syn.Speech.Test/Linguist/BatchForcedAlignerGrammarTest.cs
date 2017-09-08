using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Language.Grammar;

namespace Syn.Speech.Test.Linguist
{
    [TestClass]
    public class BatchForcedAlignerGrammarTest
    {
        [TestMethod]
        public void Batch_ForcedAlignerGrammar()
        {
            var dictionaryUrl = new URL("cmudict-en-us.dict");
            var noisedictUrl = new URL("noisedict");

            IDictionary dictionary = new TextDictionary(dictionaryUrl,
                                                       noisedictUrl,
                                                       null,
                                                       null,
                                                       new UnitManager());

            URL url = new URL("BatchForcedAlignerGrammarTest.utts");
            BatchForcedAlignerGrammar grammar;
            grammar = new BatchForcedAlignerGrammar(url.Path,
                                                    true,
                                                    true,
                                                    true,
                                                    true,
                                                    dictionary);
            grammar.Allocate();
            Assert.IsTrue(Helper.IsOneOf(grammar.GetRandomSentence(), "one", "two", "three"));
        }
    }
}
