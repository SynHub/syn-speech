using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.G2p;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
using Path = Syn.Speech.Linguist.G2p.Path;

namespace Syn.Speech.Linguist.Dictionary
{
    
/**
 * Creates a dictionary by quickly reading in an ASCII-based Sphinx-3 format
 * dictionary. When loaded the dictionary just loads each line of the dictionary
 * into the hash table, assuming that most words are not going to be used. Only
 * when a word is actually used is its pronunciations massaged into an array of
 * pronunciations.
 * <p/>
 * The format of the ASCII dictionary is the word, followed by spaces or tab,
 * followed by the pronunciation(s). For example, a digits dictionary will look
 * like:
 * <p/>
 * 
 * <pre>
 *  ONE HH W AH N
 *  ONE(2) W AH N
 *  TWO T UW
 *  THREE TH R IY
 *  FOUR F AO R
 *  FIVE F AY V
 *  SIX S IH K S
 *  SEVEN S EH V AH N
 *  EIGHT EY T
 *  NINE N AY N
 *  ZERO Z IH R OW
 *  ZERO(2) Z IY R OW
 *  OH OW
 * </pre>
 * <p/>
 * <p/>
 * In the above example, the words "one" and "zero" have two pronunciations
 * each.
 */

public class TextDictionary : IDictionary {

    // -------------------------------
    // Configuration data
    // --------------------------------

    protected URL wordDictionaryFile;
    protected URL fillerDictionaryFile;
    protected List<URL> addendaUrlList;

    // Replacement to use if word is missing
    private String wordReplacement;

    // G2P model to use if word replacement is not specified and word is missing
    protected URL g2pModelFile;
    protected int g2pMaxPron = 0;

    protected UnitManager unitManager;

    // -------------------------------
    // working data
    // -------------------------------
    protected HashMap<String, String> dictionary;
    protected HashMap<String, Word> wordDictionary;
    protected G2PConverter g2pDecoder;

    protected const String FILLER_TAG = "-F-";
    protected HashSet<String> fillerWords;
    protected bool allocated;

    public TextDictionary(String wordDictionaryFile, String fillerDictionaryFile, List<URL> addendaUrlList,
            bool addSilEndingPronunciation, String wordReplacement, UnitManager unitManager) :
     this(ConfigurationManagerUtils.ResourceToUrl(wordDictionaryFile), ConfigurationManagerUtils
                .ResourceToUrl(fillerDictionaryFile), addendaUrlList, wordReplacement, unitManager)
    {
       
    }

    public TextDictionary(URL wordDictionaryFile, URL fillerDictionaryFile, List<URL> addendaUrlList, String wordReplacement,
            UnitManager unitManager) {

        this.wordDictionaryFile = wordDictionaryFile;
        this.fillerDictionaryFile = fillerDictionaryFile;
        this.addendaUrlList = addendaUrlList;
        this.wordReplacement = wordReplacement;
        this.unitManager = unitManager;
    }

    public TextDictionary(URL wordDictionaryFile, URL fillerDictionaryFile, List<URL> addendaUrlList,
            bool addSilEndingPronunciation, String wordReplacement, UnitManager unitManager, URL g2pModelFile, int g2pMaxPron)
        :this(wordDictionaryFile, fillerDictionaryFile, addendaUrlList, wordReplacement, unitManager)
    {
        this.g2pModelFile = g2pModelFile;
        this.g2pMaxPron = g2pMaxPron;
    }

    public TextDictionary() {

    }

    /*
     * (non-Javadoc)
     * 
     * @see
     * edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util
     * .props.PropertySheet)
     */

    public override void NewProperties(PropertySheet ps)  {

        wordDictionaryFile = ConfigurationManagerUtils.GetResource(PropDictionary, ps);
        fillerDictionaryFile = ConfigurationManagerUtils.GetResource(PropFillerDictionary, ps);
        addendaUrlList = ps.GetResourceList(PropAddenda);
        wordReplacement = ps.GetString(PropWordReplacement);
        unitManager = (UnitManager) ps.GetComponent(PropUnitManager);
        g2pModelFile = ConfigurationManagerUtils.GetResource(PropG2PModelPath, ps);
        g2pMaxPron = ps.GetInt(PropG2PMaxPronunciations);
    }

    /**
     * Get the word dictionary file
     * 
     * @return the URL of the word dictionary file
     */
    public URL GetWordDictionaryFile() {
        return wordDictionaryFile;
    }

    /**
     * Get the filler dictionary file
     * 
     * @return the URL of the filler dictionary file
     */
    public URL GetFillerDictionaryFile() {
        return fillerDictionaryFile;
    }

    /*
     * (non-Javadoc)
     * 
     * @see edu.cmu.sphinx.linguist.dictionary.Dictionary#allocate()
     */

    public override void Allocate() {
        if (!allocated) {
            dictionary = new HashMap<String, String>();
            wordDictionary = new HashMap<String, Word>();

            Timer loadTimer = TimerPool.GetTimer(this, "Load Dictionary");
            fillerWords = new HashSet<String>();

            loadTimer.Start();

            this.LogInfo("Loading dictionary from: " + wordDictionaryFile);

            LoadDictionary(wordDictionaryFile.OpenStream(), false);

            LoadCustomDictionaries(addendaUrlList);

            this.LogInfo("Loading filler dictionary from: " + fillerDictionaryFile);

            LoadDictionary(fillerDictionaryFile.OpenStream(), true);

            if (g2pModelFile != null && !g2pModelFile.Path.Equals("")) {
                g2pDecoder = new G2PConverter(g2pModelFile);
            }
            loadTimer.Stop();
        }

    }

    /*
     * (non-Javadoc)
     * 
     * @see edu.cmu.sphinx.linguist.dictionary.Dictionary#deallocate()
     */

    public override void Deallocate() {
        if (allocated) {
            dictionary = null;
            g2pDecoder = null;
            allocated = false;
        }
    }

    /**
     * Loads the given simple dictionary from the given InputStream. The
     * InputStream is assumed to contain ASCII data.
     * 
     * @param inputStream
     *            the InputStream of the dictionary
     * @param isFillerDict
     *            true if this is a filler dictionary, false otherwise
     * @throws java.io.IOException
     *             if there is an error reading the dictionary
     */
    protected void LoadDictionary(Stream inputStream, bool isFillerDict)  {
        var br = new StreamReader(inputStream);
        String line;

        while ((line = br.ReadLine()) != null) {
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) {
                continue;
            }
            int spaceIndex = GetSpaceIndex(line);
            if (spaceIndex < 0) {
                throw new Error("Error loading word: " + line);
            }
            String word = line.JSubString(0, spaceIndex);

            // Add numeric index if the word is repeating.
            if (dictionary.ContainsKey(word)) {
                int index = 2;
                String wordWithIdx;
                do {
                    wordWithIdx = String.Format("{0}({1})", word, index++);
                } while (dictionary.ContainsKey(wordWithIdx));
                word = wordWithIdx;
            }

            if (isFillerDict) {
                dictionary.Put(word, (FILLER_TAG + line));
                fillerWords.Add(word);
            } else {
                dictionary.Put(word, line);
            }
        }

        br.Close();
        inputStream.Close();
    }

    private int GetSpaceIndex(String line) {
        for (int i = 0; i < line.Length; i++) {
            if (line[i] == ' ' || line[i] == '\t')
                return i;
        }
        return -1;
    }

    /**
     * Gets a context independent unit. There should only be one instance of any
     * CI unit
     * 
     * @param name
     *            the name of the unit
     * @param isFiller
     *            if true, the unit is a filler unit
     * @return the unit
     */
    protected virtual Unit GetCIUnit(String name, bool isFiller) {
        return unitManager.GetUnit(name, isFiller, Context.EmptyContext);
    }

    /**
     * Returns the sentence start word.
     * 
     * @return the sentence start word
     */
    public override Word GetSentenceStartWord() {
        return GetWord(SentenceStartSpelling);
    }

    /**
     * Returns the sentence end word.
     * 
     * @return the sentence end word
     */
    public override Word GetSentenceEndWord() {
        return GetWord(SentenceEndSpelling);
    }

    /**
     * Returns the silence word.
     * 
     * @return the silence word
     */
    public override Word GetSilenceWord() {
        return GetWord(SilenceSpelling);
    }


    /**
     * Returns a Word object based on the spelling and its classification. The
     * behavior of this method is also affected by the properties
     * wordReplacement and g2pModel
     * 
     * @param text
     *            the spelling of the word of interest.
     * @return a Word object
     * @see edu.cmu.sphinx.linguist.dictionary.Word
     */
    public override Word GetWord(String text) {
        Word wordObject = wordDictionary.Get(text);

        if (wordObject != null) {
            return wordObject;
        }

        String word = dictionary.Get(text);
        if (word == null) { // deal with 'not found' case
            this.LogInfo("The dictionary is missing a phonetic transcription for the word '" + text + "'");
            if (wordReplacement != null) {
                wordObject = GetWord(wordReplacement);
            } else if (g2pModelFile != null && !g2pModelFile.Path.Equals("")) {
                this.LogInfo("Generating phonetic transcription(s) for the word '" + text + "' using g2p model");
                wordObject = ExtractPronunciation(text);
                wordDictionary.Put(text, wordObject);
            }
        } else { // first lookup for this string
            wordObject = ProcessEntry(text);
        }

        return wordObject;
    }

    private Word ExtractPronunciation(String text) {
        Word wordObject;
        var paths = g2pDecoder.Phoneticize(text, g2pMaxPron);
        var pronunciations = new LinkedList<Pronunciation>();
        foreach (Path p in paths) {
            int unitCount = p.GetPath().Count;
            var units = new List<Unit>(unitCount);
            foreach (String token in p.GetPath()) {
                units.Add(GetCIUnit(token, false));
            }
            if (units.Count == 0) {
                units.Add(UnitManager.Silence);
            }
            pronunciations.Add(new Pronunciation(units));
        }
       
        //Pronunciation[] pronunciationsArray = pronunciations.ToArray(new Pronunciation[pronunciations.Count]);
        Pronunciation[] pronunciationsArray = pronunciations.ToArray(); //Todo: Check Semantics
        wordObject = CreateWord(text, pronunciationsArray, false);
        foreach (Pronunciation pronunciation in pronunciationsArray) {
            pronunciation.SetWord(wordObject);
        }
        return wordObject;
    }

    /**
     * Create a Word object with the given spelling and pronunciations, and
     * insert it into the dictionary.
     * 
     * @param text
     *            the spelling of the word
     * @param pronunciation
     *            the pronunciation of the word
     * @param isFiller
     *            if <code>true</code> this is a filler word
     * @return the word
     */
    private Word CreateWord(String text, Pronunciation[] pronunciation, bool isFiller) {
        Word word = new Word(text, pronunciation, isFiller);
        dictionary.Put(text, word.ToString());
        return word;
    }

    /**
     * Processes a dictionary entry. When loaded the dictionary just loads each
     * line of the dictionary into the hash table, assuming that most words are
     * not going to be used. Only when a word is actually used is its
     * pronunciations massaged into an array of pronunciations.
     */
    private Word ProcessEntry(String word) {
        var pronunciations = new LinkedList<Pronunciation>();
        String line;
        int count = 0;
        bool isFiller = false;

        do {
            count++;
            String lookupWord = word;
            if (count > 1) {
                lookupWord = lookupWord + '(' + count + ')';
            }
            line = dictionary.Get(lookupWord);
            if (line != null) {
                var st = new StringTokenizer(line);

                String tag = st.nextToken();
                isFiller = tag.StartsWith(FILLER_TAG);
                int unitCount = st.countTokens();

                var units = new List<Unit>(unitCount);
                for (int i = 0; i < unitCount; i++) {
                    String unitName = st.nextToken();
                    units.Add(GetCIUnit(unitName, isFiller));
                }
                pronunciations.Add(new Pronunciation(units));
            }
        } while (line != null);

        //Pronunciation[] pronunciationsArray = pronunciations.toArray(new Pronunciation[pronunciations.size()]);
        Pronunciation[] pronunciationsArray = pronunciations.ToArray();//Todo: Check Semantics
        Word wordObject = CreateWord(word, pronunciationsArray, isFiller);

        foreach (Pronunciation pronunciation in pronunciationsArray) {
            pronunciation.SetWord(wordObject);
        }
        wordDictionary.Put(word, wordObject);

        return wordObject;
    }

    /**
     * Returns a string representation of this TextDictionary in alphabetical
     * order.
     * 
     * @return a string representation of this dictionary
     */

    public override String ToString() {
        var sorted = new SortedDictionary<String, String>(dictionary);
        StringBuilder result = new StringBuilder();

        foreach (var entry in sorted) {
            result.Append(entry.Key);
            result.Append("   ").Append(entry.Value).Append('\n');
        }

        return result.ToString();
    }

    /**
     * Gets the set of all filler words in the dictionary
     * 
     * @return an array (possibly empty) of all filler words
     */
    public override Word[] GetFillerWords() {
        Word[] fillerWordArray = new Word[fillerWords.Count];
        int index = 0;
        foreach (String spelling in fillerWords) {
            fillerWordArray[index++] = GetWord(spelling);
        }
        return fillerWordArray;
    }

    /**
     * Dumps this FastDictionary to System.out.
     */
    public void Dump() {
        Console.Write(ToString());
    }

    /**
     * Loads the dictionary with a list of URLs to custom dictionary resources
     * 
     * @param addenda
     *            the list of custom dictionary URLs to be loaded
     * @throws IOException
     *             if there is an error reading the resource URL
     */
    private void LoadCustomDictionaries(List<URL> addenda)  {
        if (addenda != null) {
            foreach (URL addendumUrl in addenda) {
                this.LogInfo("Loading addendum dictionary from: " + addendumUrl);
                LoadDictionary(addendumUrl.OpenStream(), false);
            }
        }
    }

}
}
