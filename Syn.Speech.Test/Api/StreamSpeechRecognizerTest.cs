using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Api;

namespace Syn.Speech.Test.Api
{
    [TestClass]
    public class StreamSpeechRecognizerTest
    {
        private Configuration _configuration;
        private StreamSpeechRecognizer _speechRecognizer;
        static readonly string ModelsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Models");
        readonly string _dictionaryPath = Path.Combine(ModelsDirectory, "cmudict-en-us.dict");
        readonly string _languageModelPath = Path.Combine(ModelsDirectory, "en-us.lm");

        [TestMethod]
        public void Transcribe_UsingGrammar()
        {  
            _configuration = new Configuration
            {
                AcousticModelPath = ModelsDirectory,
                DictionaryPath = _dictionaryPath,
                LanguageModelPath = _languageModelPath,
                UseGrammar = true,
                GrammarName = "hello",
                GrammarPath = ModelsDirectory
            };

            var audioFile = Path.Combine(Directory.GetCurrentDirectory(), "Audio", "robot.wav");
            _speechRecognizer = new StreamSpeechRecognizer(_configuration);
            _speechRecognizer.StartRecognition(new FileStream(audioFile, FileMode.Open));

            var result = _speechRecognizer.GetResult();
            Assert.IsNotNull(result);
            Assert.AreEqual("the time is now exactly twenty five to one", result.GetHypothesis());
        }

        [TestMethod]
        public void Transcribe_UsingGrammar_Continuous()
        {
            _configuration = new Configuration
            {
                AcousticModelPath = ModelsDirectory,
                DictionaryPath = _dictionaryPath,
                LanguageModelPath = _languageModelPath,
                UseGrammar = true,
                GrammarName = "hello",
                GrammarPath = ModelsDirectory
            };

            _speechRecognizer = new StreamSpeechRecognizer(_configuration);

            for (int i = 1; i <=3; i++)//TODO: Reading 3 or more files in a row causes test fail (The same happens in CMU Sphinx4)
            {
                var audioFile = Path.Combine(Directory.GetCurrentDirectory(), "Audio", string.Format("Long Audio {0}.wav", i));
                var stream = new FileStream(audioFile, FileMode.Open);
                _speechRecognizer.StartRecognition(stream);
                var result = _speechRecognizer.GetResult();
                _speechRecognizer.StopRecognition();
                Assert.IsNotNull(result);
                var hypothesis = result.GetHypothesis();
                Assert.IsTrue(hypothesis.Contains("the time is now exactly twenty five to one") || hypothesis.Contains("there's three left on the left side the one closest to us"));
            }
        }
    }
}
