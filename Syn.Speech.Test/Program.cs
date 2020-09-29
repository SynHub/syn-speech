using Syn.Speech.Api;
using Syn.Speech.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Syn.Speech.Test
{
    class Program
    {
        private static Configuration _configuration;
        private static StreamSpeechRecognizer _recognizer;

        #region Helper Methods
        static void InitializeEngine()
        {
            Console.WriteLine(@"Started...");
            Logger.LogReceived += Logger_LogReceived;

            _configuration = new Configuration
            {
                AcousticModelPath = ("Models"),
                DictionaryPath = ("cmudict-en-us.dict"),
                LanguageModelPath = ("en-us.lm.dmp"),
            };

            Console.WriteLine(@"Use Grammar ? (Y/N)");
            var answer = Console.ReadLine();
            if (answer != null && answer.ToLower().Contains("y"))
            {
                _configuration.UseGrammar = true;
                _configuration.GrammarPath = "Models";
                _configuration.GrammarName = "hello";
            }

            _recognizer = new StreamSpeechRecognizer(_configuration);
        }

        static void StartRecognition()
        {
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Audio");
            var audioCollection = new Dictionary<string, FileInfo>();

            foreach (var item in Directory.GetFiles(audioDirectory))
            {
                var audioFile = new FileInfo(item);
                audioCollection.Add(audioFile.Name, audioFile);
            }

            var audioListString = string.Join(" \n", audioCollection.Keys);

            Console.WriteLine(@"Which file to use -> {0}", audioListString);
            var response = Console.ReadLine();
            if (string.IsNullOrEmpty(response)) response = "robot.wav";

            _recognizer.StartRecognition(new FileStream(Path.Combine(audioDirectory, response), FileMode.Open));

            Console.WriteLine(@"Press any key to start Speech Recognition...");
            Console.ReadLine();

            var result = _recognizer.GetResult();
            _recognizer.StopRecognition();
            if (result != null)
            {
                Console.WriteLine(result.GetHypothesis());
            }
        }
        #endregion

        #region Event Handling
        static void Logger_LogReceived(object sender, LogReceivedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
        #endregion

        static void Main()
        {
            InitializeEngine();

            StartRecognition();

            Console.WriteLine(@"Retry on another file ? (Y/N)");
            var readLine = Console.ReadLine();
            if (readLine != null && readLine.ToLower().Contains("y"))
            {
                StartRecognition();
            }
            
            Console.WriteLine(@"Ended...Restart ? (Y/N)");
            var answer = Console.ReadLine();
            if (answer != null && answer.ToLower().Contains("y"))
            {
                Main();
            }
        }
    }
}
