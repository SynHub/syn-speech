using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Language.NGram.Large;

namespace Syn.Speech.Test.Linguist
{
    [TestClass]
    public class LargeNGramTest
    {
        [TestMethod]
        public void LargeNGram_Ngram()
        {
            var dictUrl = new URL("100.dict");
            var noisedictUrl = new URL("noisedict");

            var dictionary = new TextDictionary(dictUrl,
                                                   noisedictUrl,
                                                   null,
                                                   null,
                                                   new UnitManager());

            var lm = new URL("100.arpa.dmp");
            var model = new LargeTrigramModel("",
                                                            lm,
                                                            null,
                                                            100,
                                                            100,
                                                            false,
                                                            3,
                                                            dictionary,
                                                            false,
                                                            1.0f,
                                                            1.0f,
                                                            1.0f,
                                                            false);

            dictionary.Allocate();
            model.Allocate();
            Assert.AreEqual(model.MaxDepth, 3);

            Word[] words = {
            new Word("huggins", null, false),
            new Word("daines", null, false)};
            Assert.IsTrue(Helper.CloseTo(model.GetProbability(new WordSequence(words)), -830.862, .001));

            Word[] words1 = {
            new Word("huggins", null, false),
            new Word("daines", null, false),
            new Word("david", null, false)};
            Assert.IsTrue(Helper.CloseTo(model.GetProbability(new WordSequence(words1)), -67625.77, .01));
        }

       
        
    }
}
