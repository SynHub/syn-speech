//Incomplete because the test itself fails in Java due to missing resource files.
namespace Syn.Speech.Test.Result
{
    //[TestClass]
    //public class LatticeIOTest
    //{

    //    private readonly FileInfo _latFile = new FileInfo("tmp.lat");
    //    private readonly FileInfo _slfFile = new FileInfo("tmp.slf");

    //    public LatticeIOTest()
    //    {
    //        if (!File.Exists(_latFile.FullName)) File.Create(_latFile.FullName);
    //        if (!File.Exists(_slfFile.FullName)) File.Create(_slfFile.FullName);
    //    }

    //    [TestMethod]
    //    public void TestLatticeIO()
    //    {
    //        
    //        var audioFileUrl =new URL(Helper.FilesDirectory + "/result/green.wav");
    //        var configUrl =new URL(Helper.FilesDirectory + "/result/config.xml");
    //        var lm = new URL(Helper.FilesDirectory + "/result/hellongram.trigram.lm");

    //        var cm = new ConfigurationManager(configUrl);
    //        ConfigurationManagerUtils.SetProperty(cm, "trigramModel", "location", lm.ToString());

    //        var recognizer = (Recognizer)cm.Lookup("recognizer");
    //        var dataSource = (StreamDataSource)cm.Lookup(typeof(StreamDataSource));

    //        var ais = new WaveFile(audioFileUrl.Path);
    //        dataSource.SetInputStream(ais.Stream);

    //        recognizer.Allocate();
    //        var lattice = new Lattice(recognizer.Recognize());
    //        new LatticeOptimizer(lattice).Optimize();
    //        lattice.ComputeNodePosteriors(1.0f);
    //        lattice.Dump(_latFile.FullName);
    //        lattice.DumpSlf(new StreamWriter(_slfFile.FullName));
    //        var latLattice = new Lattice(_latFile.FullName);
    //        latLattice.ComputeNodePosteriors(1.0f);
    //        var slfLattice = Lattice.ReadSlf(_slfFile.FullName);
    //        slfLattice.ComputeNodePosteriors(1.0f);
    //        var latIt = lattice.GetWordResultPath().GetEnumerator();
    //        var latLatIt = latLattice.GetWordResultPath().GetEnumerator();
    //        var slfLatIt = slfLattice.GetWordResultPath().GetEnumerator();
    //        while (latIt.MoveNext())
    //        {
    //            WordResult latWord = latIt.Current;
    //            WordResult latLatWord = latLatIt.Current;
    //            WordResult slfLatWord = slfLatIt.Current;
    //            Assert.AreEqual(latWord.Word.ToString(), latLatWord.Word.ToString());
    //            Assert.AreEqual(latWord.Word.ToString(), slfLatWord.Word.ToString());
    //            Assert.AreEqual(latWord.TimeFrame.Start, latLatWord.TimeFrame.Start);
    //        }
    //        Assert.AreEqual(lattice.TerminalNode.ViterbiScore, latLattice.TerminalNode.ViterbiScore, 0.001);
    //        Assert.AreEqual(lattice.TerminalNode.ViterbiScore, slfLattice.TerminalNode.ViterbiScore, 0.001);
    //    }
    //}
}
