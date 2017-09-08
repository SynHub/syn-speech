using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;

namespace Syn.Speech.Test.Dictionary
{
    [TestClass]
    public class DictionaryTest
    {
        [TestMethod]
        public void FullDictionaryTest()
        {

            var dictUrl = new URL("cmudict-en-us.dict");
            var noiseDictUrl = new URL("noisedict");

            var dictionary = new TextDictionary(dictUrl, noiseDictUrl, null, null, new UnitManager());

            dictionary.Allocate();
            var word = dictionary.GetWord("one");

            Assert.AreEqual(word.GetPronunciations().Length,2);
            Assert.AreEqual(word.GetPronunciations()[0].ToString(), "one(W AH N )");
            Assert.AreEqual(word.GetPronunciations()[1].ToString(), "one(HH W AH N )");

            word = dictionary.GetWord("something_missing");
            Assert.AreEqual(word, null);

            Assert.AreEqual(dictionary.GetSentenceStartWord().Spelling,"<s>");
            Assert.AreEqual(dictionary.GetSentenceEndWord().Spelling, "</s>");
            Assert.AreEqual(dictionary.GetSilenceWord().Spelling,"<sil>");

            Assert.AreEqual(dictionary.GetFillerWords().Count(), 5);

        }

        [TestMethod]
        public void Dictionary_BadDictionary()
        {

           var dictUrl = new URL(Helper.FilesDirectory + "/linguist/dictionary/cmudict-en-us.dict");
            var noiseDictUrl = new URL("noisedict");

            var dictionary = new TextDictionary(dictUrl, noiseDictUrl, null, null, new UnitManager());


            bool failed = false;
            try
            {
                dictionary.Allocate();
            }
            catch (Exception)
            {
                failed = true;
            }

            Assert.IsTrue(failed);

        }
    }
}
