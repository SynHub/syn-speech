using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Util;

namespace Syn.Speech.Test.Linguist
{
    [TestClass]
    public class MixtureComponentTest
    {
        static MixtureComponentTest()
        {
            LogMath.SetUseTable(true);
        }

        [TestMethod]
        public void MixtureComponent_UnivariateDensity()
        {
            const float minX = 10;
            const float maxX = 30;
            const float resolution = 0.1f;

            const float mean = 20;
            const float var = 3;

            var gaussian = new MixtureComponent(new[] { mean }, new[] { var });

            for (var curX = minX; curX <= maxX; curX += resolution)
            {
                var gauLogScore = gaussian.GetScore(new FloatData(new[] { curX }, 16000, 0));

                var manualScore = (1 / Math.Sqrt(var * 2 * Math.PI)) * Math.Exp((-0.5 / var) * (curX - mean) * (curX - mean));
                var gauScore = LogMath.GetLogMath().LogToLinear((float)gauLogScore);

                Assert.AreEqual(manualScore, gauScore, 1E-5);
            }
        }

        [TestMethod]
        public void MixtureComponent_UnivariateMeanTransformation()
        {
            const float mean = 20;
            const float var = 0.001f;

            var gaussian = new MixtureComponent(new[] { mean }, new[] { new float[] { 2 } }, new float[] { 5 }, new[] { var }, null, null);
            Assert.IsTrue(LogMath.GetLogMath().LogToLinear(gaussian.GetScore(new[] { 2 * mean + 5 })) > 10);
        }

#if DEBUG
        [TestMethod]
        public void MixtureComponent_Clone()
        {
            var gaussian = new MixtureComponent(new float[] { 2 }, new[] { new float[] { 3 } }, new float[] { 4 }, new float[] { 5 }, new[] { new float[] { 6 } }, new float[] { 7 });

            var clonedGaussian = gaussian.Clone();

            Assert.IsTrue(!clonedGaussian.Equals(gaussian));

            Assert.IsTrue(gaussian.Mean != clonedGaussian.Mean);
       
            Assert.IsTrue(gaussian.Variance != clonedGaussian.Variance);

            Assert.IsTrue( gaussian.GetScore(new float[] {2}) == clonedGaussian.GetScore(new float[] { 2 }));
        }
#endif
    }
}
