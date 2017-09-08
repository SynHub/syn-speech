using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Tools.Bandwidth;
using Path = System.IO.Path;

namespace Syn.Speech.Test.Util
{
    [TestClass]
    public class BandDetectorTest
    {
        [TestMethod]
        public void BandDetectionTest()
        {
            var detector = new BandDetector();
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Audio");
            var path = Path.Combine(audioDirectory, "10001-90210-01803-8khz.wav");
            Assert.IsTrue(detector.Bandwidth(path));
            path = Path.Combine(audioDirectory, "10001-90210-01803.wav");
            Assert.IsFalse(detector.Bandwidth(path));
        }
    }
}
