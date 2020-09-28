using System;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Linguist.Dictionary
{
    
    /// <summary>
    /// Provides a generic interface to a dictionary. The dictionary is responsible for determining how a word is
    /// pronounced.
    /// </summary>
    /// 
    public abstract class IDictionary: IConfigurable
    {
        /// <summary>
        ///  Spelling of the sentence start word. 
        /// </summary>
        public static string SentenceStartSpelling = "<s>";
        /// <summary>
        /// Spelling of the sentence end word. 
        /// </summary>
        static public string SentenceEndSpelling = "</s>";
        /// <summary>
        /// Spelling of the 'word' that marks a silence 
        /// </summary>
        public static string SilenceSpelling = "<sil>";

        /// <summary>
        /// The property for the dictionary file path. 
        /// </summary>
        [S4String]
        public static string PropDictionary = "dictionaryPath";

        /// <summary>
        /// The property for the g2p model file path. 
        /// </summary>
        [S4String(DefaultValue = "")]
        public static string PropG2PModelPath = "g2pModelPath";

        /// <summary>
        /// The property for the g2p model file path. 
        /// </summary>
        [S4Integer(DefaultValue = 1)]
        public static string PropG2PMaxPronunciations = "g2pMaxPron";

        /// <summary>
        /// The property for the filler dictionary file path. 
        /// </summary>
        [S4String]
        public static string PropFillerDictionary = "fillerPath";

        /// <summary>
        /// The property that specifies whether to add a duplicate SIL-ending pronunciation. 
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropAddSilEndingPronunciation = "addSilEndingPronunciation";

        /// <summary>
        /// The property that specifies the word to substitute when a lookup fails to find the word in the
        /// dictionary. If this is not set, no substitute is performed.
        /// </summary>
        [S4String(Mandatory = false)]
        public static string PropWordReplacement = "wordReplacement";

        /// <summary>
        /// The property that specifies whether the dictionary should return null if a word is not found in
        /// the dictionary, or whether it should throw an error. If true, a null is returned for words that are not found in
        /// the dictionary (and the 'PROP_WORD_REPLACEMENT' property is not set).
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropAllowMissingWords = "allowMissingWords";

        /**
        /// The property that specifies whether the Dictionary.getWord() method should return a Word object even if the
        /// word does not exist in the dictionary. If this property is true, and property allowMissingWords is also true, the
        /// method will return a Word, but the Word will have null Pronunciations. Otherwise, the method will return null.
        /// This property is usually only used for testing purposes.
         */
        [S4Boolean(DefaultValue = false)]
        public static string PropCreateMissingWords = "createMissingWords";

        /** The property that defines the name of the unit manager that is used to convert strings to Unit objects */
        [S4Component(Type = typeof(UnitManager), DefaultClass = typeof(UnitManager))] 
        public static string PropUnitManager  = "unitManager";

        /**
        /// The property for the custom dictionary file paths. This addenda property points to a possibly
        /// empty list of URLs to dictionary addenda.  Each addendum should contain word pronunciations in the same Sphinx-3
        /// dictionary format as the main dictionary.  Words in the addendum are added after the words in the main dictionary
        /// and will override previously specified pronunciations.  If you wish to extend the set of pronunciations for a
        /// particular word, add a new pronunciation by number.  For example, in the following addendum, given that the
        /// aforementioned main dictionary is specified, the pronunciation for 'EIGHT' will be overridden by the addenda,
        /// while the pronunciation for 'SIX' and 'ZERO' will be augmented and a new pronunciation for 'ELEVEN' will be
        /// added.
        /// <pre>
        ///          EIGHT   OW T
        ///          SIX(2)  Z IH K S
        ///          ZERO(3)  Z IY Rl AH
        ///          ELEVEN   EH L EH V AH N
        /// </pre>
         */
        [S4String(Mandatory = false)]
        public static string PropAddenda = "addenda";

        /// <summary>
        /// Returns a Word object based on the spelling and its classification. The behavior of this method is also affected
        /// by the properties wordReplacement, allowMissingWords, and createMissingWords.
        /// </summary>
        /// <param name="text">the spelling of the word of interest.</param>
        /// <returns>a Word object</returns>
        public abstract Word GetWord(String text);
    
        /// <summary>
        /// Returns the sentence start word.
        /// </summary>
        /// <returns></returns>
        public abstract Word GetSentenceStartWord();

        /// <summary>
        /// Returns the sentence end word.
        /// </summary>
        /// <returns></returns>
        public abstract Word GetSentenceEndWord();

        /// <summary>
        /// Returns the silence word.
        /// </summary>
        /// <returns>the silence word</returns>
        public abstract Word GetSilenceWord();

        /// <summary>
        /// Gets the set of all filler words in the dictionary
        /// </summary>
        /// <returns>an array (possibly empty) of all filler words</returns>
        public abstract Word[] GetFillerWords();

        /// <summary>
        /// Allocates the dictionary
        /// </summary>
        public abstract void Allocate();

        /// <summary>
        /// Deallocates the dictionary
        /// </summary>
        public abstract void Deallocate();


        public abstract void NewProperties(PropertySheet ps);

    }
}
