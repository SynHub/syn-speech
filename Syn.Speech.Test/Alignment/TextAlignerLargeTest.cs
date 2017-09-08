using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Alignment;
using Syn.Speech.Helper;
using Syn.Speech.Util;

namespace Syn.Speech.Test.Alignment
{
    [TestClass]
    public class TextAlignerLargeTest
    {
        //private readonly List<string> _database;
        //private readonly LongTextAligner _aligner;

        //public TextAlignerLargeTest()
        //{
        //    var rnd = new Random(42);
        //    _database = new List<String>();
        //    string[] dictionary = { "foo", "bar", "baz", "quz" };
        //    for (int i = 0; i < 100000; ++i)
        //    {
        //        _database.Add(dictionary[rnd.Next(dictionary.Length)]);
        //    }
        //    _aligner = new LongTextAligner(_database, 3);
        //}

        //[TestMethod]
        //public void TextAligner_ShortSequence()
        //{
        //    var query =  new List<string>(_database.SubList(100, 200));
        //    var ids = new int[query.Count()];
        //    for (int i = 0; i < query.Count(); ++i)ids[i] = 100 + i;
        //    var expected = Utilities.AsList(_aligner.Align(query));
        //    Assert.IsTrue(Helper.Contains(expected, ids));
        //}


        //[TestMethod]
        //public void TextAligner_LongSequence()
        //{
        //    var query = new List<string>(_database.SubList(1999, 8777));
        //    Assert.IsTrue(Helper.Contains(Utilities.AsList(_aligner.Align(query)),1));
        //}
    }
}
