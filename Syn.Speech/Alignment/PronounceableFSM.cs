//PATROLLED
using System.IO;
using Syn.Speech.Helper;

namespace Syn.Speech.Alignment
{
    /// <summary>
    /// Implements a finite state machine that checks if a given string is pronounceable. 
    /// If it is pronounceable, the method <code>accept()</code> will return true.
    /// </summary>
    public class PronounceableFSM 
    {
        private const string VOCAB_SIZE = "VOCAB_SIZE";
        private const string NUM_OF_TRANSITIONS = "NUM_OF_TRANSITIONS";
        private const string TRANSITIONS = "TRANSITIONS";
        /// <summary>
        /// The vocabulary size
        /// </summary>
        protected internal int vocabularySize;
        /// <summary>
        /// The transitions of this FSM
        /// </summary>
        protected internal int[] transitions;
        /// <summary>
        ///  Whether we should scan the input string from the front.
        /// </summary>
        protected internal bool scanFromFront;

        /// <summary>
        /// Constructs a PronounceableFSM with information in the given URL.
        /// </summary>
        /// <param name="fileInfo">path To File</param>
        /// <param name="scanFromFront">Indicates whether this FSM should scan the input string from the front, or from the back.</param>
        public PronounceableFSM(FileInfo fileInfo, bool scanFromFront)
        {
            this.scanFromFront = scanFromFront;
            //InputStream inputStream = url.openStream();
            loadText(File.ReadAllText(fileInfo.FullName));
        }

        public PronounceableFSM(string stringValue, bool scanFromFront)
        {
            this.scanFromFront = scanFromFront;
            loadText(stringValue);
        }



        /// <summary>
        /// Constructs a PronounceableFSM with the given attributes.
        /// </summary>
        /// <param name="vocabularySize">The vocabulary size of the FSM.</param>
        /// <param name="transitions">The transitions of the FSM.</param>
        /// <param name="scanFromFront">Indicates whether this FSM should scan the input string from the front, or from the back.</param>
        public PronounceableFSM(int vocabularySize, int[] transitions, bool scanFromFront)
        {
            this.vocabularySize = vocabularySize;
            this.transitions = transitions;
            this.scanFromFront = scanFromFront;
        }

        /// <summary>
        /// Loads the ASCII specification of this FSM from the given InputStream.
        /// </summary>
        /// <param name="toRead">he input stream to load from.</param>
        private void loadText(string toRead)
        {
            var reader = new StringReader(toRead);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.StartsWith("***"))
                {
                    if (line.StartsWith(VOCAB_SIZE))
                    {
                        vocabularySize = parseLastInt(line);
                    }
                    else if (line.StartsWith(NUM_OF_TRANSITIONS))
                    {
                        int transitionsSize = parseLastInt(line);
                        transitions = new int[transitionsSize];
                    }
                    else if (line.StartsWith(TRANSITIONS))
                    {
                        var stringTokenizer = new StringTokenizer(line);
                        var transition = stringTokenizer.nextToken();
                        int i = 0;
                        while (stringTokenizer.hasMoreTokens() && i < transitions.Length)
                        {
                            transition = stringTokenizer.nextToken().Trim();
                            //transitions[i++] = Integer.parseInt(transition);
                            transitions[i++] = int.Parse(transition);
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
        private int parseLastInt(string line)
        {
            string lastInt = line.Trim().Substring(line.LastIndexOf(" "));
            return int.Parse(lastInt.Trim());
        }


        /// <summary>
        /// Causes this FSM to transition to the next state given the current state and input symbol.
        /// </summary>
        /// <param name="state">The current state.</param>
        /// <param name="symbol">The input symbol.</param>
        /// <returns></returns>
        private int transition(int state, int symbol)
        {
            for (int i = state; i < transitions.Length; i++)
            {
                if ((transitions[i] % vocabularySize) == symbol)
                {
                    return (transitions[i] / vocabularySize);
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks to see if this finite state machine accepts the given input string.
        /// </summary>
        /// <param name="inputString">The input string to be tested.</param>
        /// <returns>true if this FSM accepts, false if it rejects</returns>
        public virtual bool accept(string inputString)
        {
            int symbol;
            int state = transition(0, '#');
            int leftEnd = inputString.Length - 1;
            int start = (scanFromFront) ? 0 : leftEnd;

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
                state = transition(state, symbol);
                if (state == -1)
                {
                    return false;
                }
                else if (symbol == 'V')
                {
                    return true;
                }
                if (scanFromFront)
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
