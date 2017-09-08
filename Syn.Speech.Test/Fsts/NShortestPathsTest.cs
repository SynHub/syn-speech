using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Fsts.Operations;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Helper;
using Convert = Syn.Speech.Fsts.Convert;

namespace Syn.Speech.Test.Fsts
{
    [TestClass]
    public class NShortestPathsTest
    {
        [TestMethod]
        public void NShortestPaths_Test()
        {
            var url = new URL(Helper.FilesDirectory + "/fst/algorithms/shortestpath/A.fst");
            var path = url.File.DirectoryName + "/A";
            var fst = Convert.ImportFst(path, new TropicalSemiring());
            path = Path.Combine(url.File.DirectoryName, "nsp");
            var nsp = Convert.ImportFst(path, new TropicalSemiring());

            var fstNsp = NShortestPaths.Get(fst, 6, true);
            Assert.AreEqual(nsp, fstNsp);
        }
    }
}
