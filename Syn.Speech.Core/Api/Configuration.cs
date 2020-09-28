//PATROLLED + REFACTORED
namespace Syn.Speech.Api
{
    /// <summary>
    /// Represents common configuration options.
    /// This configuration is used by high-level recognition classes.
    /// <see cref="SpeechAligner"/>
    /// <see cref="LiveSpeechRecognizer"/>
    /// <see cref="StreamSpeechRecognizer"/> 
    /// </summary>
    public class Configuration
    {
        public Configuration()
        {
            SampleRate = 16000;
        }

        /// <summary>
        /// Gets or sets path to acoustic model.
        /// </summary>
        /// <value>
        /// The acoustic model path.
        /// </value>
        public string AcousticModelPath { get; set; }

        /// <summary>
        /// Gets or sets the path to dictionary.
        /// </summary>
        /// <value>
        /// The dictionary path.
        /// </value>
        public string DictionaryPath { get; set; }

        /// <summary>
        /// Gets or sets the path to language model.
        /// </summary>
        /// <value>
        /// The language model path.
        /// </value>
        public string LanguageModelPath { get; set; }

        /// <summary>
        /// Gets or sets the path to grammar resources.
        /// </summary>
        /// <value>
        /// The grammar path.
        /// </value>
        public string GrammarPath { get; set; }

        /// <summary>
        /// Gets or sets the grammar name.
        /// </summary>
        /// <value>
        /// The name of the grammar.
        /// </value>
        public string GrammarName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fixed grammar should be used instead of language model.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use grammar]; otherwise, <c>false</c>.
        /// </value>
        public bool UseGrammar { get; set; }

        /// <summary>
        /// Gets or sets the config sample rate. Default value is 16000
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public int SampleRate { get; set; }

    }
}
