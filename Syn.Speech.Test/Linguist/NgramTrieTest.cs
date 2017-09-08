using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Language.NGram.Trie;

namespace Syn.Speech.Test.Linguist
{
    [TestClass]
    public class NgramTrieTest
    {
        [TestMethod]
        public void TestTrieNGram()
        {
            var dictUrl = new URL("100.dict");
            var noisedictUrl = new URL("noisedict");

            var dictionary = new TextDictionary(dictUrl,
                                                   noisedictUrl,
                                                   null,
                                                   null,
                                                   new UnitManager());

            var lm = new URL("100.arpa.bin");
            var model = new NgramTrieModel("",
                                                            lm,
                                                            null,
                                                            100,
                                                            false,
                                                            3,
                                                            dictionary,
                                                            false,
                                                            1.0f,
                                                            1.0f,
                                                            1.0f
                                                            );
            dictionary.Allocate();
            model.Allocate();
            Assert.AreEqual(model.MaxDepth, 3);

            Word[] words = {
            new Word("huggins", null, false),
            new Word("daines", null, false)};
            Assert.IsTrue(Helper.CloseTo(model.GetProbability(new WordSequence(words)), -831, .001));

            Word[] words1 = {
            new Word("huggins", null, false),
            new Word("daines", null, false),
            new Word("david", null, false)};
            Assert.IsTrue(Helper.CloseTo(model.GetProbability(new WordSequence(words1)), -67637, .01));
        }
    }
}
