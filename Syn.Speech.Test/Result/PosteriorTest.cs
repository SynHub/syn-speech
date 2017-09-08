using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Results;
using Syn.Speech.Util;

namespace Syn.Speech.Test.Result
{
    [TestClass]
    public class PosteriorTest
    {
        [TestMethod]
        public void TestPosterior()
        {
            var logMath = LogMath.GetLogMath();

            var lattice = new Lattice();

            var a = lattice.AddNode("A", "A", 0, 0);
            var b = lattice.AddNode("B", "B", 0, 0);
            var c = lattice.AddNode("C", "C", 0, 0);
            var d = lattice.AddNode("D", "D", 0, 0);

            const double acousticAb = 4;
            const double acousticAc = 6;
            const double acousticCb = 1;
            const double acousticBd = 5;
            const double acousticCD = 2;

            lattice.InitialNode = a;
            lattice.TerminalNode = d;

            lattice.AddEdge(a, b, logMath.LinearToLog(acousticAb), 0);
            lattice.AddEdge(a, c, logMath.LinearToLog(acousticAc), 0);
            lattice.AddEdge(c, b, logMath.LinearToLog(acousticCb), 0);
            lattice.AddEdge(b, d, logMath.LinearToLog(acousticBd), 0);
            lattice.AddEdge(c, d, logMath.LinearToLog(acousticCD), 0);

            lattice.ComputeNodePosteriors(1.0f);
            const double pathAbd = acousticAb * acousticBd;
            const double pathAcbd = acousticAc * acousticCb * acousticBd;
            const double pathAcd = acousticAc * acousticCD;
            const double allPaths = pathAbd + pathAcbd + pathAcd;

            const double bPosterior = (pathAbd + pathAcbd) / allPaths;
            const double cPosterior = (pathAcbd + pathAcd) / allPaths;

            const double delta = 1e-4;

            Assert.AreEqual(logMath.LogToLinear((float)a.Posterior), 1.0, delta);
            Assert.AreEqual(logMath.LogToLinear((float)b.Posterior), bPosterior, delta);
            Assert.AreEqual(logMath.LogToLinear((float)c.Posterior), cPosterior, delta);
            Assert.AreEqual(logMath.LogToLinear((float)d.Posterior), 1.0, delta);
        }
    }
}
