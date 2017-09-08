using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Fsts;
using Syn.Speech.Fsts.Operations;
using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Test.Fsts
{
    [TestClass]
    public class ArcSortTest
    {
        private Fst CreateOsorted()
        {
            var fst = new Fst(new TropicalSemiring());

            var s1 = new State(0f);
            var s2 = new State(0f);
            var s3 = new State(0f);

            // State 0
            fst.AddState(s1);
            s1.AddArc(new Arc(4, 1, 0f, s3));
            s1.AddArc(new Arc(5, 2, 0f, s3));
            s1.AddArc(new Arc(2, 3, 0f, s2));
            s1.AddArc(new Arc(1, 4, 0f, s2));
            s1.AddArc(new Arc(3, 5, 0f, s2));

            // State 1
            fst.AddState(s2);
            s2.AddArc(new Arc(3, 1, 0f, s3));
            s2.AddArc(new Arc(1, 2, 0f, s3));
            s2.AddArc(new Arc(2, 3, 0f, s2));

            // State 2 (final)
            fst.AddState(s3);

            return fst;
        }

        private Fst CreateIsorted()
        {
            var fst = new Fst(new TropicalSemiring());

            var s1 = new State(0f);
            var s2 = new State(0f);
            var s3 = new State(0f);

            // State 0
            fst.AddState(s1);
            s1.AddArc(new Arc(1, 4, 0f, s2));
            s1.AddArc(new Arc(2, 3, 0f, s2));
            s1.AddArc(new Arc(3, 5, 0f, s2));
            s1.AddArc(new Arc(4, 1, 0f, s3));
            s1.AddArc(new Arc(5, 2, 0f, s3));

            // State 1
            fst.AddState(s2);
            s2.AddArc(new Arc(1, 2, 0f, s3));
            s2.AddArc(new Arc(2, 3, 0f, s2));
            s2.AddArc(new Arc(3, 1, 0f, s3));

            // State 2 (final)
            fst.AddState(s3);

            return fst;
        }

        private Fst CreateUnsorted()
        {
            var fst = new Fst(new TropicalSemiring());

            var s1 = new State(0f);
            var s2 = new State(0f);
            var s3 = new State(0f);

            // State 0
            fst.AddState(s1);
            s1.AddArc(new Arc(1, 4, 0f, s2));
            s1.AddArc(new Arc(3, 5, 0f, s2));
            s1.AddArc(new Arc(2, 3, 0f, s2));
            s1.AddArc(new Arc(5, 2, 0f, s3));
            s1.AddArc(new Arc(4, 1, 0f, s3));

            // State 1
            fst.AddState(s2);
            s2.AddArc(new Arc(2, 3, 0f, s2));
            s2.AddArc(new Arc(3, 1, 0f, s3));
            s2.AddArc(new Arc(1, 2, 0f, s3));

            // State 2 (final)
            fst.AddState(s3);

            return fst;
        }

        [TestMethod]
        public void ArcSort_Test()
        {
            // Input label sort test
            var fst1 = CreateUnsorted();
            var fst2 = CreateIsorted();
            Assert.AreNotEqual(fst1, fst2);
            ArcSort.Apply(fst1, new ILabelCompare());
            Assert.AreEqual(fst1, fst2);

            // Output label sort test
            fst1 = CreateUnsorted();
            fst2 = CreateOsorted();
            Assert.AreNotEqual(fst1, fst2);
            ArcSort.Apply(fst1, new OLabelCompare());
            Assert.AreEqual(fst1,fst2);
        }
    }
}
