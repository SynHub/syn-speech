using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;

namespace Syn.Speech.Test.Linguist
{
    [TestClass]
    public class WordSequenceTest
    {

        private IDictionary dictionary;

        public WordSequenceTest()
        {
            var dictUrl =  new URL("cmudict-en-us.dict");
            var noiseDictUrl = new URL("noisedict");

            dictionary =new TextDictionary(dictUrl, noiseDictUrl, null, null, new UnitManager());
            dictionary.Allocate();
        }
        
        [TestMethod]
        public void WordSequence_Equals()
        {
            WordSequence ws = WordSequence.AsWordSequence(dictionary, "one", "two", "three");
            Assert.AreEqual(ws.Size, 3);
            Assert.AreEqual(WordSequence.AsWordSequence(dictionary, "one", "two", "three").ToString(), ws.ToString());
        }

        [TestMethod]
        public void WordSequence_GetOldest()
        {
            WordSequence ws = WordSequence.AsWordSequence(dictionary, "zero", "six", "one");
            Assert.AreEqual(WordSequence.AsWordSequence(dictionary,"zero","six").ToString(), ws.GetOldest().ToString());
            Assert.AreEqual(ws.GetOldest().GetOldest().ToString(), new WordSequence(ws.GetWord(0)).ToString());
        }

        [TestMethod]
        public void WordSequence_GetNewest()
        {
            WordSequence ws = WordSequence.AsWordSequence(dictionary, "one", "two", "three");
            Assert.AreEqual(WordSequence.AsWordSequence(dictionary,"two","three").ToString(),ws.GetNewest().ToString());
            Assert.AreEqual(ws.GetNewest().GetOldest().ToString(),ws.GetOldest().GetNewest().ToString());
        }

        [TestMethod]
        public void WordSequence_UnknownWords()
        {
            Assert.AreEqual(new WordSequence(Word.Unknown).ToString(), new WordSequence(Word.Unknown).ToString());
            Assert.AreNotEqual(new WordSequence(Word.Unknown, Word.Unknown, Word.Unknown).ToString(), new WordSequence(Word.Unknown, Word.Unknown).ToString());
        }

    }
}
