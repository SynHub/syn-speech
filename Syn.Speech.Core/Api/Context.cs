using System;
using System.Globalization;
using System.IO;
using Syn.Speech.FrontEnds.FrequencyWarp;
using Syn.Speech.FrontEnds.Util;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Properties;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Api
{
    /// <summary>
    ///  Helps to tweak configuration without touching XML-file directly.
    /// </summary>
    public class Context
    {
        private readonly ConfigurationManager _configurationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public Context(Configuration config)
            : this(new URL(URLType.Resource, Resources.default_config), config)
        {

        }

        /// <summary>
        /// Constructs builder using user-supplied XML configuration.
        /// </summary>
        /// <param name="path">path to XML-resource with configuration</param>
        /// <param name="config">the same instance of {@link Configuration}</param>
        public Context(URL path, Configuration config)
        {

            _configurationManager = new ConfigurationManager(path);

            SetAcousticModel(config.AcousticModelPath);
            SetDictionary(config.DictionaryPath);

            if (null != config.GrammarPath && config.UseGrammar)
                SetGrammar(config.GrammarPath, config.GrammarName);
            if (!String.IsNullOrEmpty(config.LanguageModelPath) && !config.UseGrammar)
                SetLanguageModel(config.LanguageModelPath);

            SetSampleRate(config.SampleRate);

            // Force ConfigurationManager to build the whole graph
            // in order to enable instance lookup by class.

            _configurationManager.Lookup("recognizer");
        }

        /// <summary>
        /// Sets acoustic model location.
        /// It also reads feat.params which should be located at the root of
        /// acoustic model and sets corresponding parameters of
        /// <see cref="MelFrequencyFilterBank2"/> instance.
        /// </summary>
        /// <param name="path">The path to directory with acoustic model files.</param>
        public void SetAcousticModel(String path)
        {
            SetLocalProperty("acousticModelLoader->location", path);
            SetLocalProperty("dictionary->fillerPath", path + "/noisedict");
        }

        /// <summary>
        /// Sets the dictionary.
        /// </summary>
        /// <param name="path">The path to directory with dictionary files.</param>
        public void SetDictionary(String path)
        {
            SetLocalProperty("dictionary->dictionaryPath", path);
        }

        /// <summary>
        /// Sets sampleRate.
        /// </summary>
        /// <param name="sampleRate">The sample rate of the input stream..</param>
        public void SetSampleRate(int sampleRate)
        {
            SetLocalProperty("dataSource->sampleRate", sampleRate.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets path to the grammar files. Enables static grammar and disables probabilistic language model.
        /// JSGF and GrXML formats are supported.
        /// </summary>
        /// <param name="path">The path to the grammar files.</param>
        /// <param name="name">The name of the main grammar to use.</param>
        public void SetGrammar(String path, string name)
        {
            // TODO: use a single param of type File, cache directory part
            if (name.EndsWith(".grxml"))
            {
                SetLocalProperty("grXmlGrammar->grammarLocation", path + name);
                SetLocalProperty("flatLinguist->grammar", "grXmlGrammar");
            }
            else
            {
                SetLocalProperty("jsgfGrammar->grammarLocation", path);
                SetLocalProperty("jsgfGrammar->grammarName", name);
                SetLocalProperty("flatLinguist->grammar", "jsgfGrammar");
            }
            SetLocalProperty("decoder->searchManager", "simpleSearchManager");
        }

        /// <summary>
        /// Sets path to the language model. Enables probabilistic language model and disables static grammar.
        /// Currently it supports ".lm", ".dmp" and ".bin" file formats.
        /// </summary>
        /// <param name="path">The path to the language model file.</param>
        /// <exception cref="System.ArgumentException">Unknown format extension:  + path</exception>
        public void SetLanguageModel(String path)
        {
            if (path.EndsWith(".lm"))
            {
                SetLocalProperty("simpleNGramModel->location", path);
                SetLocalProperty(
                    "lexTreeLinguist->languageModel", "simpleNGramModel");
            }
            else if (path.EndsWith(".dmp"))
            {
                SetLocalProperty("largeTrigramModel->location", path);
                SetLocalProperty(
                    "lexTreeLinguist->languageModel", "largeTrigramModel");
            }
            else
            {
                throw new ArgumentException("Unknown format extension: " + path);
            }
        }

        public void SetSpeechSource(Stream stream, TimeFrame timeFrame)
        {
            ((StreamDataSource)GetInstance(typeof(StreamDataSource))).SetInputStream(stream, timeFrame);
            SetLocalProperty("trivialScorer->frontend", "liveFrontEnd");
        }

        /// <summary>
        /// Sets byte stream as the speech source.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void SetSpeechSource(FileStream stream)
        {
            ((StreamDataSource)GetInstance(typeof(StreamDataSource))).SetInputStream(stream);
            SetLocalProperty("threadedScorer->frontend", "liveFrontEnd");
        }


        /// <summary>
        /// Sets property within a "component" tag in configuration.
        /// Use this method to alter "value" property of a "property" tag inside a
        /// "component" tag of the XML configuration.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public void SetLocalProperty(String name, Object value)
        {
            ConfigurationManagerUtils.SetProperty(_configurationManager, name, value.ToString());
        }


        /// <summary>
        /// Sets property of a top-level "property" tag.
        /// Use this method to alter "value" property of a "property" tag whose
        /// parent is the root tag "config" of the XML configuration.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public void SetGlobalProperty(String name, Object value)
        {
            _configurationManager.SetGlobalProperty(name, value.ToString());
        }


        /// <summary>
        /// Returns instance of the XML configuration by its class.
        /// </summary>
        /// <param name="clazz">The class to look up.</param>
        /// <returns>Instance of the specified class or null.</returns>
        public IConfigurable GetInstance(Type clazz)
        {
            return _configurationManager.Lookup(clazz);
        }

        /// <summary>
        /// Gets the Loader object used for loading the acoustic model.
        /// </summary>
        /// <returns>The loader object.</returns>
        public ILoader GetLoader()
        {
            return (ILoader)_configurationManager.Lookup("acousticModelLoader");
        }
    }
}
