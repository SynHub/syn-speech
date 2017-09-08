using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Fsts;
using Syn.Speech.Fsts.Operations;
using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Test.Fsts
{
    [TestClass]
    public class ProjectTest
    {

        private Fst CreateFst()
        {
            var ts = new TropicalSemiring();
            var fst = new Fst(ts);

            var s1 = new State(ts.Zero);
            var s2 = new State(ts.Zero);
            var s3 = new State(ts.Zero);
            var s4 = new State(2f);

            // State 0
            fst.AddState(s1);
            s1.AddArc(new Arc(1, 5, 1f, s2));
            s1.AddArc(new Arc(2, 4, 3f, s2));
            fst.SetStart(s1);

            // State 1
            fst.AddState(s2);
            s2.AddArc(new Arc(3, 3, 7f, s2));
            s2.AddArc(new Arc(4, 2, 5f, s3));

            // State 2
            fst.AddState(s3);
            s3.AddArc(new Arc(5, 1, 9f, s4));

            // State 3
            fst.AddState(s4);

            return fst;
        }

        private Fst CreatePi()
        {
            var ts = new TropicalSemiring();
            var fst = new Fst(ts);
            var s1 = new State(ts.Zero);
            var s2 = new State(ts.Zero);
            var s3 = new State(ts.Zero);
            var s4 = new State(2f);

            // State 0
            fst.AddState(s1);
            s1.AddArc(new Arc(1, 1, 1f, s2));
            s1.AddArc(new Arc(2, 2, 3f, s2));
            fst.SetStart(s1);

            // State 1
            fst.AddState(s2);
            s2.AddArc(new Arc(3, 3, 7f, s2));
            s2.AddArc(new Arc(4, 4, 5f, s3));

            // State 2
            fst.AddState(s3);
            s3.AddArc(new Arc(5, 5, 9f, s4));

            // State 3
            fst.AddState(s4);

            return fst;
        }

        private Fst CreatePo()
        {
            var ts = new TropicalSemiring();
            var fst = new Fst(ts);

            var s1 = new State(ts.Zero);
            var s2 = new State(ts.Zero);
            var s3 = new State(ts.Zero);
            var s4 = new State(2f);

            // State 0
            fst.AddState(s1);
            s1.AddArc(new Arc(5, 5, 1f, s2));
            s1.AddArc(new Arc(4, 4, 3f, s2));
            fst.SetStart(s1);

            // State 1
            fst.AddState(s2);
            s2.AddArc(new Arc(3, 3, 7f, s2));
            s2.AddArc(new Arc(2, 2, 5f, s3));

            // State 2
            fst.AddState(s3);
            s3.AddArc(new Arc(1, 1, 9f, s4));

            // State 3
            fst.AddState(s4);

            return fst;
        }



        [TestMethod]
        public void Project_Test()
        {
            Console.WriteLine("Testing Project...");
            // Project on Input label
            Fst fst = CreateFst();
            Fst p = CreatePi();
            Project.Apply(fst, ProjectType.Input);
            Assert.IsTrue(fst.Equals(p));

            // Project on Output label
            fst = CreateFst();
            p = CreatePo();
            Project.Apply(fst, ProjectType.Output);
            Assert.IsTrue(fst.Equals(p));

            Console.WriteLine("Testing Project Completed!\n");
        }
    }
}
