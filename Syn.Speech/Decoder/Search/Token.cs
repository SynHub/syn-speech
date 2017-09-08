using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Syn.Speech.Common;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Decoder.Scorer;
using Syn.Speech.Linguist;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// Represents a single state in the recognition trellis. Subclasses of a token are used to represent the various
    /// emitting state.
    /// All scores are maintained in LogMath log base
    /// </summary>
    public class Token: IScoreable
    {
        private static int curCount;
        private static int lastCount;
        private static String scoreFmt = "0.0000000E00";
        private static String numFmt = "0000";

        private Token predecessor;
        CultureInfo culture;
        

        private float logLanguageScore;
        private float logTotalScore;
        private float logInsertionScore;
        private float logAcousticScore;
        private float logWorkingScore;
    
        private ISearchState searchState;

        //private int location;
        private int frameNumber;
        private IData myData;

        /// <summary>
        /// A collection of arbitrary properties assigned to this token. This field becomes lazy initialized to reduce
        /// memory footprint.
        /// </summary>
        private Dictionary<String, Object> tokenProps;
        /// <summary>
        /// Internal constructor for a token. Used by classes Token, CombineToken, ParallelToken
        /// </summary>
        /// <param name="predecessor">the predecessor for this token</param>
        /// <param name="state">the SentenceHMMState associated with this token</param>
        /// <param name="logTotalScore">the total entry score for this token (in LogMath log base)</param>
        /// <param name="logInsertionScore"></param>
        /// <param name="logLanguageScore">the language score associated with this token (in LogMath log base)</param>
        /// <param name="frameNumber">the frame number associated with this token</param>
        public Token(Token predecessor,
                 ISearchState state,
                 float logTotalScore,
                 float logInsertionScore,
                 float logLanguageScore,
                 int frameNumber)
        {
            this.predecessor = predecessor;
            this.searchState = state;
            this.logTotalScore = logTotalScore;
            this.logInsertionScore = logInsertionScore;
            this.logLanguageScore = logLanguageScore;
            this.frameNumber = frameNumber;
            //this.location = -1;
            curCount++;
        }
        /// <summary>
        /// Creates the initial token with the given word history depth
        /// </summary>
        /// <param name="state">the SearchState associated with this token</param>
        /// <param name="frameNumber">the frame number for this token</param>
        public Token(ISearchState state, int frameNumber)
            :this(null, state, 0.0f, 0.0f, 0.0f, frameNumber)
        {
            
        }
        /// <summary>
        /// Creates a Token with the given acoustic and language scores and predecessor.
        /// </summary>
        /// <param name="predecessor">the predecessor Token</param>
        /// <param name="logTotalScore">the log acoustic score</param>
        /// <param name="logAcousticScore">the log language score</param>
        /// <param name="logInsertionScore"></param>
        /// <param name="logLanguageScore"></param>
        public Token(Token predecessor,
                 float logTotalScore,
                 float logAcousticScore,
                 float logInsertionScore,
                 float logLanguageScore)
            : this(predecessor, null, logTotalScore, logInsertionScore, logLanguageScore, 0)
        {
            this.logAcousticScore = logAcousticScore;
        }
        /// <summary>
        /// Returns the predecessor for this token, or null if this token has no predecessors
        /// </summary>
        /// <returns></returns>
        public Token getPredecessor()
        {
            return predecessor;
        }
        /// <summary>
        /// Returns the frame number for this token. Note that for tokens that are associated with non-emitting states, the
        /// frame number represents the next frame number.  For emitting states, the frame number represents the current
        /// frame number.
        /// </summary>
        /// <returns></returns>
        public int getFrameNumber() 
        {
            return frameNumber;
        }
        /// <summary>
        /// Sets the feature for this Token.
        /// </summary>
        /// <param name="data"></param>
        public void setData(IData data) 
        {
            myData = data;
        }
        /// <summary>
        /// Returns the feature for this Token.
        /// </summary>
        /// <returns></returns>
        public IData getData() 
        {
            return myData;
        }

        /// <summary>
        /// Returns the score for the token. The score is a combination of language and acoustic scores
        /// </summary>
        /// <returns></returns>
        public float getScore() 
        {
            return logTotalScore;
        }


        /// <summary>
        /// Calculates a score against the given feature. The score can be retrieved 
        /// with get score. The token will keep a reference to the scored feature-vector.
        /// </summary>
        /// <param name="feature">the feature to be scored</param>
        /// <returns>the score for the feature</returns>
        public float calculateScore(IData feature) 
        {
        
            logAcousticScore = ((IScoreProvider) searchState).getScore(feature);

            logTotalScore += logAcousticScore;

            setData(feature);

            return logTotalScore;
        }
    
        public float[] calculateComponentScore(IData feature)
        {
    	    return ((IScoreProvider) searchState).getComponentScore(feature);
        }

        /// <summary>
        /// Normalizes a previously calculated score
        /// </summary>
        /// <param name="maxLogScore">the score to normalize this score with</param>
        /// <returns>the normalized score</returns>
        public float normalizeScore(float maxLogScore) 
        {
            logTotalScore -= maxLogScore;
            logAcousticScore -= maxLogScore;
            return logTotalScore;
        }

        /// <summary>
        /// Gets the working score. The working score is used to maintain non-final
        /// scores during the search. Some search algorithms such as bushderby use
        /// the working score
        /// </summary>
        /// <returns>the working score (in logMath log base)</returns>
        public float getWorkingScore() 
        {
            return logWorkingScore;
        }

        /// <summary>
        /// Sets the working score for this token
        /// </summary>
        /// <param name="logScore">the working score (in logMath log base)</param>
        public void setWorkingScore(float logScore) 
        {
            logWorkingScore = logScore;
        }

        /// <summary>
        /// Sets the score for this token
        /// </summary>
        /// <param name="logScore">the new score for the token (in logMath log base)</param>
        public void setScore(float logScore) 
        {
            this.logTotalScore = logScore;
        }
        /// <summary>
        /// Returns the language score associated with this token
        /// </summary>
        /// <returns></returns>
        public float getLanguageScore() 
        {
            return logLanguageScore;
        }

        /// <summary>
        /// Returns the insertion score associated with this token.
        /// Insertion score is the score of the transition between
        /// states. It might be transition score from the acoustic model,
        /// phone insertion score or word insertion probability from
        /// the linguist.

        /// </summary>
        /// <returns></returns>
        public float getInsertionScore() 
        {
            return logInsertionScore;
        }
        /// <summary>
        /// Returns the acoustic score for this token (in logMath log base).
        /// Acoustic score is a sum of frame GMM.
        /// </summary>
        /// <returns></returns>
        public float getAcousticScore() 
        {
            return logAcousticScore;
        }
        /// <summary>
        /// Returns the SearchState associated with this token
        /// </summary>
        /// <returns></returns>
        public ISearchState getSearchState() 
        {
            return searchState;
        }
        /// <summary>
        /// Determines if this token is associated with an emitting state. An emitting state is a state that can be scored
        /// acoustically.
        /// </summary>
        /// <returns></returns>
        public bool isEmitting() 
        {
            return searchState.isEmitting();
        }
        /// <summary>
        /// Determines if this token is associated with a final SentenceHMM state.
        /// </summary>
        /// <returns></returns>
        public bool isFinal() 
        {
            return searchState.isFinal();
        }

        /// <summary>
        /// Determines if this token marks the end of a word
        /// </summary>
        /// <returns></returns>
        public bool isWord() 
        {
            return searchState is IWordSearchState;
        }
        /// <summary>
        /// Retrieves the string representation of this object
        /// </summary>
        /// <returns></returns>
    
        public override String ToString() 
        {
            return
                getFrameNumber().ToString(numFmt) + ' ' +
                getScore().ToString(scoreFmt) + ' ' +
                getAcousticScore().ToString(scoreFmt) + ' ' +
                getLanguageScore().ToString(scoreFmt) + ' ' +
                getSearchState() + (tokenProps == null ? "" : " " + tokenProps);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new Exception("dummy Token serializer");
        }


        /// <summary>
        /// dumps a branch of tokens 
        /// </summary>
        public void dumpTokenPath() 
        {
            dumpTokenPath(true);
        }
        /// <summary>
        /// dumps a branch of tokens
        /// </summary>
        /// <param name="includeHMMStates">if true include all sentence hmm states</param>
        public void dumpTokenPath(Boolean includeHMMStates) 
        {
            Token token = this;
            List<Token> list = new List<Token>();

            while (token != null) 
            {
                list.Add(token);
                token = token.getPredecessor();
            }
            for (int i = list.Count - 1; i >= 0; i--) 
            {
                token = list[i];
                if (includeHMMStates ||
                        (!(token.getSearchState() is IHMMSearchState))) {
                    Console.Out.WriteLine("  " + token);
                }
            }

            Console.Out.WriteLine();
        }

        /// <summary>
        /// Returns the string of words leading up to this token.
        /// </summary>
        /// <param name="wantFiller">if true, filler words are added</param>
        /// <param name="wantPronunciations">if true append [ phoneme phoneme ... ] after each word</param>
        /// <returns></returns>
        public String getWordPath(Boolean wantFiller, Boolean wantPronunciations) 
        {
            StringBuilder sb = new StringBuilder();
            Token token = this;

            while (token != null) 
            {
                if (token.isWord()) 
                {
                    IWordSearchState wordState =(IWordSearchState) token.getSearchState();
                    IPronunciation pron = wordState.getPronunciation();
                    IWord word = wordState.getPronunciation().getWord();

          //Console.Out.WriteLine(token.getFrameNumber() + " " + word + " " + token.logLanguageScore + " " + token.logAcousticScore);

                    if (wantFiller || !word.isFiller()) 
                    {
                        if (wantPronunciations) 
                        {
                            sb.Insert(0, ']');
                            IUnit[] u = pron.getUnits();
                            for (int i = u.Length - 1; i >= 0; i--) 
                            {
                                if (i < u.Length - 1) 
                                    sb.Insert(0, ',');
                                sb.Insert(0, u[i].getName());
                            }
                            sb.Insert(0, '[');
                        }
                        sb.Insert(0, word.getSpelling());
                        sb.Insert(0, ' ');
                    }
                }
                token = token.getPredecessor();
            }
            return sb.ToString().Trim();
        }
        /// <summary>
        /// Returns the string of words for this token, with no embedded filler words
        /// </summary>
        /// <returns>the string of words</returns>
        public String getWordPathNoFiller() 
        {
            return getWordPath(false, false);
        }
        /// <summary>
        /// Returns the string of words for this token, with embedded silences
        /// </summary>
        /// <returns>the string of words</returns>
        public String getWordPath() 
        {
            return getWordPath(true, false);
        }
        /// <summary>
        /// Returns the string of words and units for this token, with embedded silences.
        /// </summary>
        /// <returns>the string of words and units</returns>public IWord getWord() 
        public String getWordUnitPath() 
        {
            StringBuilder sb = new StringBuilder();
            Token token = this;

            while (token != null) 
            {
                ISearchState searchState = token.getSearchState();
                if (searchState is IWordSearchState) 
                {
                    IWordSearchState wordState = (IWordSearchState) searchState;
                    IWord word = wordState.getPronunciation().getWord();
                    sb.Insert(0, ' ' + word.getSpelling());
                } 
                else if (searchState is IUnitSearchState) 
                {
                    IUnitSearchState unitState = (IUnitSearchState) searchState;
                    IUnit unit = unitState.getUnit();
                    sb.Insert(0, ' ' + unit.getName());
                }
                token = token.getPredecessor();
            }
            return sb.ToString().Trim();
        }
        /// <summary>
        /// Returns the word of this Token, the search state is a WordSearchState. If the search state is not a
        /// WordSearchState, return null.
        /// </summary>
        /// <returns>the word of this Token, or null if this is not a word token</returns>
        public IWord getWord() 
        {
            if (isWord()) 
            {
                IWordSearchState wordState = (IWordSearchState) searchState;
                return wordState.getPronunciation().getWord();
            } 
            else {
                return null;
            }
        }
        /// <summary>
        /// Shows the token count
        /// </summary>
        public static void showCount() 
        {
            Console.Out.WriteLine("Cur count: " + curCount + " new " +
                    (curCount - lastCount));
            lastCount = curCount;
        }

        /// <summary>
        /// Returns the location of this Token in the ActiveList. In the HeapActiveList implementation, it is the index of
        /// the Token in the array backing the heap.
        /// </summary>
        /// <returns></returns>
        //public int getLocation() 
        //{
        //    return location;
        //}

        ///// <summary>
        ///// Sets the location of this Token in the ActiveList.
        ///// </summary>
        ///// <param name="location"></param>
        //public void setLocation(int location) 
        //{
        //    this.location = location;
        //}
        /// <summary>
        /// Determines if this branch is valid
        /// </summary>
        /// <returns>true if the token and its predecessors are valid</returns>
        public Boolean validate() 
        {
            return true;
        }
        /// <summary>
        /// Return the DecimalFormat object for formatting the print out of scores.
        /// </summary>
        /// <returns>the DecimalFormat object for formatting score print outs</returns>
        protected static String getScoreFormat() 
        {
            return scoreFmt;
        }

        /// <summary>
        /// Return the DecimalFormat object for formatting the print out of numbers
        /// </summary>
        /// <returns></returns>
        protected static String getNumberFormat() 
        {
            return numFmt;
        }


        /// <summary>
        /// Returns the application object
        /// </summary>
        /// <returns></returns>
        //public Dictionary<String, Object> getTokenProps() 
        //{
        //    if (tokenProps == null)
        //        tokenProps = new Dictionary<String, Object>();

        //    return tokenProps;
        //}

        /// <summary>
        ///         
        ///A {@code Scoreable} comparator that is used to order scoreables according to their score,
        /// in descending order.
        ///<p>Note: since a higher score results in a lower natural order,
        /// statements such as {@code Collections.min(list, Scoreable.COMPARATOR)}
        /// actually return the Scoreable with the <b>highest</b> score,
        /// in contrast to the natural meaning of the word "min".   
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        int IComparer<IScoreable>.Compare(IScoreable t1, IScoreable t2)
        {
            if (t1.getScore() > t2.getScore())
            {
                return -1;
            }
            else if (t1.getScore() == t2.getScore())
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }


        //public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        //{
        //    throw new NotImplementedException();
        //}



        public void update(Token predecessor, ISearchState nextState,
          float logEntryScore, float insertionProbability,
          float languageProbability, int currentFrameNumber)
        {
            this.predecessor = predecessor;
            this.searchState = nextState;
            this.logTotalScore = logEntryScore;
            this.logInsertionScore = insertionProbability;
            this.logLanguageScore = languageProbability;
            this.frameNumber = currentFrameNumber;
        }
    }
}
