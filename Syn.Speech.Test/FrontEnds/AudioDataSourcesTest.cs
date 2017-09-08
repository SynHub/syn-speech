using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.Util;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.FrontEnds
{
    [TestClass]
    public class AudioDataSourcesTest
    {
        private static int _numFileStarts;
        private static int _numFileEnds;

        public class CustomAudioFileProcessListener : IAudioFileProcessListener
        {
            public void NewProperties(PropertySheet ps)
            {

            }

            public void AudioFileProcStarted(FileInfo audioFile)
            {
                _numFileStarts++;
            }

            public void AudioFileProcFinished(FileInfo audioFile)
            {
                _numFileEnds++;
            }
        }

        [TestMethod]
        public void AudioFile_Sources()
        {
            RunAssert(Helper.FilesDirectory + "/frontend/test.wav");
        }

        [TestMethod]
        public void AudioFile_8KhzSource()
        {
            var dataSource = ConfigurationManager.GetInstance<AudioFileDataSource>();
            // Test simple WAV.
            var file = new URL(Helper.FilesDirectory + "/frontend/test8k.wav");
            dataSource.SetAudioFile(file, null);
            Assert.IsTrue(dataSource.GetData() is DataStartSignal);
            var d = dataSource.GetData();
            Assert.IsTrue(dataSource.GetData() is DoubleData);
            Assert.AreEqual(((DoubleData)d).SampleRate, 8000);
            while ((d = dataSource.GetData()) is DoubleData);
            Assert.IsTrue(d is DataEndSignal);
        }

        [TestMethod]
        public void AudioFile_ConcatDataSource()
        {
            var dataSource = ConfigurationManager.GetInstance<ConcatAudioFileDataSource>();
            dataSource.AddNewFileListener(new CustomAudioFileProcessListener());

            var fileName = Helper.FilesDirectory + "/frontend/" + GetType().Name + ".drv";
            var tmpFile = File.Create(fileName);
            var pw = new StreamWriter(tmpFile);
            var path1 = Helper.FilesDirectory + "/frontend/test1.wav";
            var path2 = Helper.FilesDirectory + "/frontend/test2.wav";
            var path3 = Helper.FilesDirectory + "/frontend/test3.wav";
            pw.WriteLine(path1);
            pw.WriteLine(path2);
            pw.Write(path3);
            pw.Close();

            dataSource.SetBatchFile(new FileInfo(fileName));
            Assert.IsTrue(dataSource.GetData() is DataStartSignal);
            Assert.IsTrue(dataSource.GetData() is DoubleData);

            IData d;
            while ((d = dataSource.GetData()) is DoubleData);
            Assert.IsTrue(d is DataEndSignal);

            Assert.AreEqual(_numFileStarts, 3);
            Assert.AreEqual(_numFileEnds, 3);
        }

        private void RunAssert(string fileName)
        {
            var dataSource = ConfigurationManager.GetInstance<AudioFileDataSource>();
            var file = new URL(fileName);
            dataSource.SetAudioFile(file, null);
            Assert.IsTrue(dataSource.GetData() is DataStartSignal);
            Assert.IsTrue(dataSource.GetData() is DoubleData);

            IData d;
            while ((d = dataSource.GetData()) is DoubleData);
            Assert.IsTrue(d is DataEndSignal);
        }
    }
}
