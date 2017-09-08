using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.NGram
{
    /// <summary>
    /// An ASCII ARPA language model loader. This loader makes no attempt to optimize storage, 
    /// so it can only load very small language models.
    /// Note that all probabilities in the grammar are stored in LogMath log base format. 
    /// Language Probabilities in the language model file are stored in log 10 base.
    /// </summary>
    public class SimpleNGramModel : LanguageModel
    {

        // ----------------------------
        // Configuration data
        // ----------------------------
        private LogMath _logMath;
        private URL _urlLocation;
        private float _unigramWeight;
        private IDictionary _dictionary;
        private int _desiredMaxDepth;
        private Dictionary<WordSequence, Probability> _map;
        private HashSet<string> _vocabulary;
        protected int LineNumber;
        protected StreamReader Reader;
        protected string FileName;
        private bool _allocated;
        private LinkedList<WordSequence> _tokens;

        public SimpleNGramModel(String location, IDictionary dictionary, float unigramWeight, int desiredMaxDepth)
            : this(ConfigurationManagerUtils.ResourceToUrl(location), dictionary, unigramWeight, desiredMaxDepth)
        {

        }

        public SimpleNGramModel(URL urlLocation, IDictionary dictionary, float unigramWeight, int desiredMaxDepth)
        {
            this._urlLocation = urlLocation;
            this._unigramWeight = unigramWeight;
            _logMath = LogMath.GetLogMath();
            this._desiredMaxDepth = desiredMaxDepth;
            this._dictionary = dictionary;
            _map = new HashMap<WordSequence, Probability>();
            _vocabulary = new HashSet<String>();
            _tokens = new LinkedList<WordSequence>();
        }

        public SimpleNGramModel()
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            _logMath = LogMath.GetLogMath();

            if (_allocated)
            {
                throw new RuntimeException("Can't change properties after allocation");
            }

            _urlLocation = ConfigurationManagerUtils.GetResource(PropLocation, ps);
            _unigramWeight = ps.GetFloat(PropUnigramWeight);
            _desiredMaxDepth = ps.GetInt(PropMaxDepth);
            _dictionary = (IDictionary)ps.GetComponent(PropDictionary);
            _map = new HashMap<WordSequence, Probability>();
            _vocabulary = new HashSet<String>();
            _tokens = new LinkedList<WordSequence>();
        }

        public override void Allocate()
        {
            _allocated = true;
            Load(_urlLocation, _unigramWeight, _dictionary);
            if (_desiredMaxDepth > 0)
            {
                if (_desiredMaxDepth < MaxDepth)
                {
                    MaxDepth = _desiredMaxDepth;
                }
            }
        }

        public override void Deallocate()
        {
            _allocated = false;
        }

        public string Name { get; private set; }

        /// <summary>
        /// Gets the n-gram probability of the word sequence represented by the word list
        /// </summary>
        /// <param name="wordSequence">the wordSequence</param>
        /// <returns>the probability of the word sequence in LogMath log base</returns>
        public override float GetProbability(WordSequence wordSequence)
        {
            float logProbability = 0.0f;
            Probability prob = GetProb(wordSequence);
            if (prob == null)
            {
                if (wordSequence.Size > 1)
                {
                    logProbability = GetBackoff(wordSequence.GetOldest())
                                     + GetProbability(wordSequence.GetNewest());
                }
                else
                { // if the single word is not in the model at all
                    // then its zero likelihood that we'll use it
                    logProbability = LogMath.LogZero;
                }
            }
            else
            {
                logProbability = prob.LogProbability;
            }
            // System.out.println("Search: " + wordSequence + " : "
            // + logProbability + " "
            // + logMath.logToLinear(logProbability));
            return logProbability;
        }

        /// <summary>
        /// Gets the smear term for the given wordSequence
        /// </summary>
        /// <param name="wordSequence">the word sequence</param>
        /// <returns>the smear term associated with this word sequence</returns>
        public override float GetSmear(WordSequence wordSequence)
        {
            return 0.0f; // TODO not implemented
        }

        /// <summary>
        /// Returns the backoff probability for the give sequence of words.
        /// </summary>
        /// <param name="wordSequence">The sequence of words.</param>
        /// <returns>The backoff probability in LogMath log base.</returns>
        public float GetBackoff(WordSequence wordSequence)
        {
            float logBackoff = 0.0f; // log of 1.0
            Probability prob = GetProb(wordSequence);
            if (prob != null)
            {
                logBackoff = prob.LogBackoff;
            }
            return logBackoff;
        }

        /// <summary>
        /// Returns the maximum depth of the language model
        /// </summary>
        /// <value>The maximum depth of the language model.</value>
        public override int MaxDepth { get; set; }

        /// <summary>
        /// Returns the set of words in the language model. The set is unmodifiable.
        /// </summary>
        /// <value>The unmodifiable set of words.</value>
        public override HashSet<string> Vocabulary
        {
            get { return _vocabulary; //TODO: should be a ReadOnlyCollection ( but has been set as such for performance )
            }
        }

        /// <summary>
        /// Gets the probability entry for the given word sequence or null if there is no entry.
        /// </summary>
        /// <param name="wordSequence">A word sequence.</param>
        /// <returns>The probability entry for the wordlist or null.</returns>
        private Probability GetProb(WordSequence wordSequence)
        {
            return _map[wordSequence];
        }

        /// <summary>
        /// Converts a wordList to a string.
        /// </summary>
        /// <param name="wordList">The wordlist.</param>
        /// <returns>the string</returns>
        private string ListToString(List<Word> wordList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var word in wordList)
                sb.Append(word).Append(' ');
            return sb.ToString();
        }

        /// <summary>
        /// Dumps the language model.
        /// </summary>
        public void Dump()
        {
            foreach (var entry in _map)
                this.LogInfo(entry.Key + " " + entry.Value);
        }

        /// <summary>
        /// Retrieves a string representation of the wordlist, suitable for map access
        /// </summary>
        /// <param name="wordList">The list of words.</param>
        /// <returns>a string representation of the word list</returns>
        private string GetRepresentation(List<String> wordList)
        {
            if (wordList.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            foreach (String word in wordList)
                sb.Append(word).Append('+');
            sb.Length = sb.Length - 1;
            return sb.ToString();
        }

        /// <summary>
        /// Loads the language model from the given location.
        /// </summary>
        /// <param name="location">The URL location of the model.</param>
        /// <param name="unigramWeightValue">The unigram weight.</param>
        /// <param name="dictionaryValue">The dictionary.</param>
        private void Load(URL location, float unigramWeightValue, IDictionary dictionaryValue)
        {
            string line;
            float logUnigramWeight = _logMath.LinearToLog(unigramWeightValue);
            float inverseLogUnigramWeight = _logMath
                    .LinearToLog(1.0 - unigramWeightValue);

            Open(location);
            // look for beginning of data
            ReadUntil("\\data\\");
            // look for ngram statements
            List<int> ngramList = new List<int>();
            while ((line = ReadLine()) != null)
            {
                if (line.StartsWith("ngram"))
                {
                    StringTokenizer st = new StringTokenizer(line, " \t\n\r\f=");
                    if (st.countTokens() != 3)
                    {
                        Corrupt("corrupt ngram field " + line + ' '
                                + st.countTokens());
                    }
                    st.nextToken();
                    int index = int.Parse(st.nextToken(), CultureInfo.InvariantCulture.NumberFormat);
                    int count = int.Parse(st.nextToken(), CultureInfo.InvariantCulture.NumberFormat);
                    ngramList.Insert(index - 1, count);
                    MaxDepth = Math.Max(index, MaxDepth);
                }
                else if (line.Equals("\\1-grams:"))
                {
                    break;
                }
            }
            int numUnigrams = ngramList[0] - 1;
            // -log(x) = log(1/x)
            float logUniformProbability = -_logMath.LinearToLog(numUnigrams);
            for (int index = 0; index < ngramList.Count; index++)
            {
                int ngram = index + 1;
                int ngramCount = ngramList[index];
                for (int i = 0; i < ngramCount; i++)
                {
                    StringTokenizer tok = new StringTokenizer(ReadLine());
                    int tokenCount = tok.countTokens();
                    if (tokenCount != ngram + 1 && tokenCount != ngram + 2)
                    {
                        Corrupt("Bad format");
                    }
                    float log10Prob = float.Parse(tok.nextToken(), CultureInfo.InvariantCulture.NumberFormat);
                    float log10Backoff = 0.0f;
                    // construct the WordSequence for this N-Gram
                    List<Word> wordList = new List<Word>(MaxDepth);
                    for (int j = 0; j < ngram; j++)
                    {
                        string word = tok.nextToken();
                        _vocabulary.Add(word);
                        Word wordObject = dictionaryValue.GetWord(word);
                        if (wordObject == null)
                        {
                            wordObject = Word.Unknown;
                        }
                        wordList.Add(wordObject);
                    }
                    WordSequence wordSequence = new WordSequence(wordList);
                    if (tok.hasMoreTokens())
                    {
                        log10Backoff = float.Parse(tok.nextToken(), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    float logProb = _logMath.Log10ToLog(log10Prob);
                    float logBackoff = _logMath.Log10ToLog(log10Backoff);
                    // Apply unigram weights if this is a unigram probability
                    if (ngram == 1)
                    {
                        float p1 = logProb + logUnigramWeight;
                        float p2 = logUniformProbability + inverseLogUnigramWeight;
                        logProb = _logMath.AddAsLinear(p1, p2);
                        // System.out
                        // .println("p1 " + p1 + " p2 " + p2 + " luw "
                        // + logUnigramWeight + " iluw "
                        // + inverseLogUnigramWeight + " lup "
                        // + logUniformProbability + " logprog "
                        // + logProb);
                    }
                    Put(wordSequence, logProb, logBackoff);
                }
                if (index < ngramList.Count - 1)
                {
                    string next = "\\" + (ngram + 1) + "-grams:";
                    ReadUntil(next);
                }
            }
            ReadUntil("\\end\\");
            Close();
        }

        /// <summary>
        ///  Puts the probability into the map.
        /// </summary>
        /// <param name="wordSequence">The tag for the prob.</param>
        /// <param name="logProb">The probability in log math base.</param>
        /// <param name="logBackoff">The backoff probability in log math base.</param>
        private void Put(WordSequence wordSequence, float logProb, float logBackoff)
        {
            // System.out.println("Putting " + wordSequence + " p " + logProb
            // + " b " + logBackoff);
            Java.Put(_map,wordSequence, new Probability(logProb, logBackoff));
            Java.Add(_tokens, wordSequence);
        }

        /// <summary>
        /// Returns a list of all the word sequences in the language model This
        /// method is used to create Finite State Transducers of the language model.
        /// </summary>
        /// <returns>Containing all the word sequences.</returns>
        public LinkedList<WordSequence> GetNGrams()
        {
            return _tokens;
        }

        /// <summary>
        /// Reads the next line from the LM file. Keeps track of line number.
        /// </summary>
        /// <returns></returns>
        private string ReadLine()
        {
            string line;
            LineNumber++;
            line = Reader.ReadLine();
            if (line == null)
            {
                Corrupt("Premature EOF");
            }
            return line.Trim();
        }

        /// <summary>
        /// the language model at the given location
        /// </summary>
        /// <param name="location">The path to the language model.</param>
        private void Open(URL location)
        {
            LineNumber = 0;
            FileName = location.Path;
            Reader = new StreamReader(FileName);
        }

        /// <summary>
        /// Reads from the input stream until the input matches the given string.
        /// </summary>
        /// <param name="match">The string to match on.</param>
        private void ReadUntil(String match)
        {
            try
            {
                while (!ReadLine().Equals(match))
                {
                }
            }
            catch (IOException ioe)
            {
                Corrupt("Premature EOF while waiting for " + match);
            }
        }

        /// <summary>
        /// Closes the language model file.
        /// </summary>
        private void Close()
        {
            Reader.Close();
            Reader = null;
        }

        /// <summary>
        /// Generates a 'corrupt' IO exception.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <exception cref="System.IO.IOException">Corrupt Language Model  + fileName+  at line  + lineNumber + ':' + reason</exception>
        private void Corrupt(string reason)
        {
            throw new IOException("Corrupt Language Model " + FileName+ " at line " + LineNumber + ':' + reason);
        }
    }

    /// <summary>
    /// Represents a probability and a backoff probability
    /// </summary>
    public class Probability
    {
        internal float LogProbability;
        internal float LogBackoff;

        /// <summary>
        /// Initializes a new instance of the <see cref="Probability"/> class.
        /// </summary>
        /// <param name="logProbability">The probability.</param>
        /// <param name="logBackoff">The backoff probability.</param>
        internal Probability(float logProbability, float logBackoff)
        {
            LogProbability = logProbability;
            LogBackoff = logBackoff;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "Prob: " + LogProbability + ' ' + LogBackoff;
        }
    }
}
