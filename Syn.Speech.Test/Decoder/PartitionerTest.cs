using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Decoders.Search;
using Syn.Speech.Decoders.Search.Comparator;
using Syn.Speech.Helper;

namespace Syn.Speech.Test.Decoder
{
    [TestClass]
    public class PartitionerTest
    {
        public void TestSorted(Token[] tokens, int p)
        {
            for (var i = 0; i < p; i++)
            {
                Assert.IsTrue(tokens[i].Score >= tokens[p].Score);
            }
            for (var i = p; i < tokens.Length; i++)
            {
                Assert.IsTrue(tokens[i].Score <= tokens[p].Score);
            }
        }

        private static void PerformTestPartitionSizes(int absoluteBeamWidth, int tokenListSize, bool tokenListLarger)
        {

            var random = new Random((int)Java.CurrentTimeMillis());
            var partitioner = new Partitioner();

            var parent = new Token(null, 0);
            var tokens = new Token[tokenListSize];

            for (var i = 0; i < tokens.Length; i++)
            {
                var logTotalScore = (float)random.NextDouble();
                tokens[i] = new Token(parent, null, logTotalScore, 0.0f, 0.0f, i);
            }

            var r = partitioner.Partition(tokens, tokens.Length, absoluteBeamWidth);

            if (tokenListLarger)
            {
                Assert.AreEqual(r, absoluteBeamWidth - 1);
            }
            else
            {
                Assert.AreEqual(r, tokenListSize - 1);
            }

            var firstList = new List<Token>();
            if (r >= 0)
            {
                var lowestScore = tokens[r].Score;

                for (var i = 0; i <= r; i++)
                {
                    Assert.IsTrue(tokens[i].Score >= lowestScore);
                    firstList.Add(tokens[i]);
                }
                for (var i = r + 1; i < tokens.Length; i++)
                {
                    Assert.IsTrue(lowestScore > tokens[i].Score);
                }


                firstList.Sort(new ScoreableComparator());

                var secondList = Arrays.AsList(tokens);
                secondList.Sort(new ScoreableComparator());


                for (int i=0;i<firstList.Count;i++)
                {
                    var t1 = firstList[i];
                    var t2 = secondList[i];
                    Assert.AreEqual(t1, t2);
                }
            }
        }

        [TestMethod]
        public void Partition_Orders()
        {
            int p;
            var tokens = new Token[100000];
            var partitioner = new Partitioner();

            for (var i = 0; i < 100000; i++)
                tokens[i] = new Token(null, null, 1 - i, 0, 0, 0);
            p = partitioner.Partition(tokens, 100000, 3000);
            Assert.AreEqual(p, 2999);
            TestSorted(tokens, p);

            for (var i = 0; i < 100000; i++)
                tokens[i] = new Token(null, null, i, 0, 0, 0);
            p = partitioner.Partition(tokens, 100000, 3000);
            Assert.AreEqual(p, 2999);
            TestSorted(tokens, p);

            for (var i = 0; i < 100000; i++)
                tokens[i] = new Token(null, null, 0, 0, 0, 0);
            p = partitioner.Partition(tokens, 100000, 3000);
            Assert.AreEqual(p, 2999);
            TestSorted(tokens, p);

            for (var i = 0; i < 100000; i++) tokens[i] = new Token(null, null, (float)Java.Random(), 0, 0, 0);
            p = partitioner.Partition(tokens, 100000, 3000);
            Assert.AreEqual(p, 2999);
            TestSorted(tokens, p);
        }

        [TestMethod]
        public void Partition_Sizes()
        {

            const int absoluteBeamWidth = 1500;
            var tokenListSize = 3000;

            // Test 1 : (tokenListSize > absoluteBeamWidth)
            PerformTestPartitionSizes(absoluteBeamWidth, tokenListSize, true);

            // Test 2 : (tokenListSize == absoluteBeamWidth)
            tokenListSize = absoluteBeamWidth;
            PerformTestPartitionSizes(absoluteBeamWidth, tokenListSize, false);

            // Test 3 : (tokenListSize < absoluteBeamWidth)
            tokenListSize = 1000;
            PerformTestPartitionSizes(absoluteBeamWidth, tokenListSize, false);

            // Test 4 : (tokenListSize == 0)
            tokenListSize = 0;
            PerformTestPartitionSizes(absoluteBeamWidth, tokenListSize, false);
        }
    }
}
