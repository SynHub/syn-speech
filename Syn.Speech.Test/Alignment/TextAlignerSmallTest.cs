using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Alignment;
using Syn.Speech.Helper;
using Syn.Speech.Util;

namespace Syn.Speech.Test.Alignment
{
    [TestClass]
    public class TextAlignerSmallTest
    {
        private readonly LongTextAligner _aligner;

        public TextAlignerSmallTest()
        {
            var url = new URL("transcription-small.txt");
            var words = new List<String>();
            var fileString = File.ReadAllText(url.Path);
            words.AddRange(fileString.Split(' ','\n','\r'));
            words.RemoveAll(item => item.Length==0);
            _aligner = new LongTextAligner(words, 2);
        }

        [TestMethod]
        public void TextAligner_SmallAlign()
        {
            var wordList = new Dictionary<string[], int[]>
            {
                {new[] {"foo", "foo"}, new[] {-1, -1}},
                {new[] {"foo", "baz"}, new[] {2, 3}},
                {new[] {"foo", "bar", "foo", "bar", "baz", "42"}, new[] {0, 1, 2, 4, 5, 6}},
                {new[] {"foo", "bar", "foo", "baz", "bar"}, new[] {0, 1, 2, 3, 4}}
            };

            foreach (var item in wordList.Keys)
            {
                Assert.IsTrue(Helper.Contains(Utilities.AsList(_aligner.Align(item.ToList())), wordList[item]));
            }
            
        }
    }
}
