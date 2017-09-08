using System;
using System.IO;
using Syn.Logging;
using Syn.Speech.Api;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test
{
    public class Program2
    {
        static Configuration _speechConfiguration;
        static StreamSpeechRecognizer _speechRecognizer;

        static void LogReceived(object sender, LogReceivedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void Main()
        {
            Logger.LogReceived += LogReceived;
            var modelsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Models");
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Audio");

            if (!Directory.Exists(modelsDirectory) || !Directory.Exists(audioDirectory))
            {
                Console.WriteLine("No Models or Audio directory found!! Aborting...");
                Console.ReadLine();
                return;
            }

            _speechConfiguration = new Configuration
            {
                AcousticModelPath = modelsDirectory,
                DictionaryPath = Path.Combine(modelsDirectory, "cmudict-en-us.dict"),
                LanguageModelPath = Path.Combine(modelsDirectory, "en-us.lm.dmp"),
                UseGrammar = true,
                GrammarPath = modelsDirectory,
                GrammarName = "hello"
            };

            _speechRecognizer = new StreamSpeechRecognizer(_speechConfiguration);
            for (int i = 1; i <= 3; i++)
            {
                var audioFile = Path.Combine(audioDirectory, String.Format("Long Audio {0}.wav", i));
                var stream = new FileStream(audioFile, FileMode.Open);
                if (i == 3)
                {
                    System.Diagnostics.Trace.WriteLine("checking");
                }
                _speechRecognizer.StartRecognition(stream);
                var result = _speechRecognizer.GetResult();
                _speechRecognizer.StopRecognition();
                if (result != null)
                {
                    Console.WriteLine(string.Format("Result: {0}",i) + result.GetHypothesis());
                }
                else
                {
                    Console.WriteLine("Result: {0}", "Sorry! Coudn't Transcribe");
                }
                var instance = ConfigurationManager.GetInstance<SpeechMarker>();
                Console.WriteLine(instance.ToString());
                stream.Close();
            }
            Console.WriteLine("DONE!");
            Console.ReadLine();
        }
    }
}
