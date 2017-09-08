
using System.Globalization;
using System.IO;
using Syn.Speech.Helper;

//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    /// <summary>
    /// Implements a finite state machine that checks if a given string is pronounceable. 
    /// If it is pronounceable, the method <code>accept()</code> will return true.
    /// </summary>
    public class PronounceableFsm 
    {
        private const string VocabSize = "VOCAB_SIZE";
        private const string NumOfTransitions = "NUM_OF_TRANSITIONS";
        private const string Transitions = "TRANSITIONS";
        /// <summary>
        /// The vocabulary size
        /// </summary>
        protected internal int VocabularySize;
        /// <summary>
        /// The transitions of this FSM
        /// </summary>
        private int[] _transitions;
        /// <summary>
        ///  Whether we should scan the input string from the front.
        /// </summary>
        protected internal bool ScanFromFront;

        /// <summary>
        /// Constructs a PronounceableFSM with information in the given URL.
        /// </summary>
        /// <param name="fileInfo">path To File</param>
        /// <param name="scanFromFront">Indicates whether this FSM should scan the input string from the front, or from the back.</param>
        public PronounceableFsm(FileInfo fileInfo, bool scanFromFront)
        {
            ScanFromFront = scanFromFront;
            //InputStream inputStream = url.openStream();
            LoadText(File.ReadAllText(fileInfo.FullName));
        }

        public PronounceableFsm(string stringValue, bool scanFromFront)
        {
            ScanFromFront = scanFromFront;
            LoadText(stringValue);
        }

        /// <summary>
        /// Constructs a PronounceableFSM with the given attributes.
        /// </summary>
        /// <param name="vocabularySize">The vocabulary size of the FSM.</param>
        /// <param name="transitions">The transitions of the FSM.</param>
        /// <param name="scanFromFront">Indicates whether this FSM should scan the input string from the front, or from the back.</param>
        public PronounceableFsm(int vocabularySize, int[] transitions, bool scanFromFront)
        {
            VocabularySize = vocabularySize;
            _transitions = transitions;
            ScanFromFront = scanFromFront;
        }

        /// <summary>
        /// Loads the ASCII specification of this FSM from the given InputStream.
        /// </summary>
        /// <param name="toRead">he input stream to load from.</param>
        private void LoadText(string toRead)
        {
            var reader = new StringReader(toRead);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.StartsWith("***"))
                {
                    if (line.StartsWith(VocabSize))
                    {
                        VocabularySize = ParseLastInt(line);
                    }
                    else if (line.StartsWith(NumOfTransitions))
                    {
                        int transitionsSize = ParseLastInt(line);
                        _transitions = new int[transitionsSize];
                    }
                    else if (line.StartsWith(Transitions))
                    {
                        var stringTokenizer = new StringTokenizer(line);
                        var transition = stringTokenizer.nextToken();
                        int i = 0;
                        while (stringTokenizer.hasMoreTokens() && i < _transitions.Length)
                        {
                            transition = stringTokenizer.nextToken().Trim();
                            _transitions[i++] = int.Parse(transition, CultureInfo.InvariantCulture.NumberFormat);
                        }
                    }
                }
            }
            reader.Close();
        }

        /// <summary>
        ///  Returns the integer value of the last integer in the given string.
        /// </summary>
        /// <param name="line">The line to parse the integer from.</param>
        /// <returns>An integer</returns>
        private static int ParseLastInt(string line)
        {
            string lastInt = line.Trim().Substring(line.LastIndexOf(" ", System.StringComparison.Ordinal));
            return int.Parse(lastInt.Trim(), CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Causes this FSM to transition to the next state given the current state and input symbol.
        /// </summary>
        /// <param name="state">The current state.</param>
        /// <param name="symbol">The input symbol.</param>
        /// <returns></returns>
        private int Transition(int state, int symbol)
        {
            for (int i = state; i < _transitions.Length; i++)
            {
                if ((_transitions[i] % VocabularySize) == symbol)
                {
                    return (_transitions[i] / VocabularySize);
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks to see if this finite state machine accepts the given input string.
        /// </summary>
        /// <param name="inputString">The input string to be tested.</param>
        /// <returns>true if this FSM accepts, false if it rejects</returns>
        public virtual bool Accept(string inputString)
        {
            int symbol;
            int state = Transition(0, '#');
            int leftEnd = inputString.Length - 1;
            int start = (ScanFromFront) ? 0 : leftEnd;

            for (int i = start; 0 <= i && i <= leftEnd; )
            {
                char c = inputString[i];
                if (c == 'n' || c == 'm')
                {
                    symbol = 'N';
                }
                else if ("aeiouy".IndexOf(c) != -1)
                {
                    symbol = 'V';
                }
                else
                {
                    symbol = c;
                }
                state = Transition(state, symbol);
                if (state == -1)
                {
                    return false;
                }
                else if (symbol == 'V')
                {
                    return true;
                }
                if (ScanFromFront)
                {
                    i++;
                }
                else
                {
                    i--;
                }
            }
            return false;
        }
    }
}
