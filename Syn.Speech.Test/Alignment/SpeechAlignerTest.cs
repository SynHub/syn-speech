using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Alignment;
using Syn.Speech.Util;

namespace Syn.Speech.Test.Alignment
{
    [TestClass]
    public class SpeechAlignerTest
    {
        [TestMethod]
        public void SpeechAligner_ShouldAlign()
        {
            Align(AsList("foo"), AsList("bar"), -1);
            Align(AsList("foo"), AsList("foo"), 0);
            Align(AsList("foo", "bar"), AsList("foo"), 0);
            Align(AsList("foo", "bar"), AsList("bar"), 1);
            Align(AsList("foo"), AsList("foo", "bar"), 0, -1);
            Align(AsList("bar"), AsList("foo", "bar"), -1, 0);
            Align(AsList("foo", "bar", "baz"), AsList("foo", "baz"), 0, 2);
            Align(AsList("foo", "bar", "42", "baz", "qux"), AsList("42", "baz"), 2, 3);
        }

        private void Align(List<string> database, List<string> query, params int[] result)
        {
            var aligner = new LongTextAligner(database, 1);
            int[] alignment = aligner.Align(query);
            Assert.IsTrue(Helper.Contains(Utilities.AsList(alignment),result));
        }

        private List<string> AsList(params string[] value)
        {
            return new List<string>(value);
        } 
    }
}
