using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Fsts;
using Syn.Speech.Fsts.Operations;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Helper;
using Convert = Syn.Speech.Fsts.Convert;

namespace Syn.Speech.Test.Fsts
{
    [TestClass]
    public class ComposeEpsilonTest
    {
        [TestMethod]
        public void ComposeEpsilon_Test()
        {

            URL url = new URL(Helper.FilesDirectory+ "/fst/algorithms/composeeps/A.fst.txt");

            String path = url.File.DirectoryName + "/A";
            Fst fstA = Convert.ImportFst(path, new TropicalSemiring());
            path = url.File.DirectoryName + "/B";
            Fst fstB = Convert.ImportFst(path, new TropicalSemiring());
            path = Path.Combine(url.File.DirectoryName, "fstcomposeeps");
            Fst fstC = Convert.ImportFst(path, new TropicalSemiring());

            Fst fstComposed = Compose.Get(fstA, fstB, new TropicalSemiring());
            Assert.AreEqual(fstC,fstComposed);
        }
    }
}
