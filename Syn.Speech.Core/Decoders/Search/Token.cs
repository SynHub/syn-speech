using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Syn.Speech.Decoders.Scorer;
using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Dictionary;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// Represents a single state in the recognition trellis. Subclasses of a token are used to represent the various
    /// emitting state.
    /// All scores are maintained in LogMath log base
    /// </summary>
    public class Token : IScoreable
    {
        private static int _curCount;
        private static int _lastCount;

        /// <summary>
        /// Internal constructor for a token. Used by classes Token, CombineToken, ParallelToken
        /// </summary>
        /// <param name="predecessor">the predecessor for this token</param>
        /// <param name="state">the SentenceHMMState associated with this token</param>
        /// <param name="logTotalScore">the total entry score for this token (in LogMath log base)</param>
        /// <param name="logInsertionScore"></param>
        /// <param name="logLanguageScore">the language score associated with this token (in LogMath log base)</param>
        /// <param name="frameNumber">the frame number associated with this token</param>
        public Token(Token predecessor, ISearchState state, float logTotalScore, float logInsertionScore,float logLanguageScore, int frameNumber)
        {
            Predecessor = predecessor;
            SearchState = state;
            Score = logTotalScore;
            InsertionScore = logInsertionScore;
            LanguageScore = logLanguageScore;
            FrameNumber = frameNumber;
            //this.location = -1;
            _curCount++;
        }
        /// <summary>
        /// Creates the initial token with the given word history depth
        /// </summary>
        /// <param name="state">the SearchState associated with this token</param>
        /// <param name="frameNumber">the frame number for this token</param>
        public Token(ISearchState state, int frameNumber)
            : this(null, state, 0.0f, 0.0f, 0.0f, frameNumber)
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
        public Token(Token predecessor, float logTotalScore, float logAcousticScore, float logInsertionScore, float logLanguageScore)
            : this(predecessor, null, logTotalScore, logInsertionScore, logLanguageScore, 0)
        {
            AcousticScore = logAcousticScore;
        }

        static Token()
        {
            NumberFormat = "0000";
            ScoreFormat = "0.0000000E00";
        }

        /// <summary>
        /// Returns the predecessor for this token, or null if this token has no predecessors
        /// </summary>
        /// <value></value>
        public Token Predecessor { get; private set; }

        /// <summary>
        /// Returns the frame number for this token. Note that for tokens that are associated with non-emitting states, the
        /// frame number represents the next frame number.  For emitting states, the frame number represents the current
        /// frame number.
        /// </summary>
        /// <value></value>
        public int FrameNumber { get; private set; }

        /// <summary>
        /// Gets or sets the feature for this Token.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public IData Data { get; set; }

        /// <summary>
        /// Gets or sets the score for the token. The score is a combination of language and acoustic scores.
        /// while setting the new score for the token is (in logMath log base)
        /// </summary>
        /// <value></value>
        public float Score { get; set; }


        /// <summary>
        /// Calculates a score against the given feature. The score can be retrieved 
        /// with get score. The token will keep a reference to the scored feature-vector.
        /// </summary>
        /// <param name="feature">the feature to be scored</param>
        /// <returns>the score for the feature</returns>
        public virtual float CalculateScore(IData feature)
        {
            AcousticScore = ((IScoreProvider)SearchState).GetScore(feature);

            Score += AcousticScore;

            Data = feature;

            return Score;
        }

        public float[] CalculateComponentScore(IData feature)
        {
            return ((IScoreProvider)SearchState).GetComponentScore(feature);
        }

        /// <summary>
        /// Normalizes a previously calculated score
        /// </summary>
        /// <param name="maxLogScore">the score to normalize this score with</param>
        /// <returns>the normalized score</returns>
        public float NormalizeScore(float maxLogScore)
        {
            Score -= maxLogScore;
            AcousticScore -= maxLogScore;
            return Score;
        }

        /// <summary>
        /// Gets the working score. The working score is used to maintain non-final
        /// scores during the search. Some search algorithms such as bushderby use
        /// the working score.
        /// Sets the working score for this token.
        /// </summary>
        /// <value>the working score (in logMath log base)</value>
        public float WorkingScore
        {
            get;
            //the working score (in logMath log base)
            set;
        }


        /// <summary>
        /// Returns the language score associated with this token
        /// </summary>
        /// <value></value>
        public float LanguageScore { get; private set; }

        /// <summary>
        /// Returns the insertion score associated with this token.
        /// Insertion score is the score of the transition between
        /// states. It might be transition score from the acoustic model,
        /// phone insertion score or word insertion probability from
        /// the linguist.
        /// </summary>
        /// <value></value>
        public float InsertionScore { get; private set; }

        /// <summary>
        /// Returns the acoustic score for this token (in logMath log base).
        /// Acoustic score is a sum of frame GMM.
        /// </summary>
        /// <value></value>
        public float AcousticScore { get; private set; }

        /// <summary>
        /// Returns the SearchState associated with this token
        /// </summary>
        /// <value></value>
        public ISearchState SearchState { get; private set; }

        /// <summary>
        /// Determines if this token is associated with an emitting state. An emitting state is a state that can be scored
        /// acoustically.
        /// </summary>
        /// <value></value>
        public bool IsEmitting
        {
            get { return SearchState.IsEmitting; }
        }

        /// <summary>
        /// Determines if this token is associated with a final SentenceHMM state.
        /// </summary>
        /// <value></value>
        public bool IsFinal
        {
            get { return SearchState.IsFinal; }
        }

        /// <summary>
        /// Determines if this token marks the end of a word
        /// </summary>
        /// <value></value>
        public bool IsWord
        {
            get { return SearchState is IWordSearchState; }
        }

        /// <summary>
        /// Retrieves the string representation of this object
        /// </summary>
        /// <returns></returns>

        public override string ToString()
        {
            return
                FrameNumber.ToString(NumberFormat) + ' ' +
                Score.ToString(ScoreFormat) + ' ' +
                AcousticScore.ToString(ScoreFormat) + ' ' +
                LanguageScore.ToString(ScoreFormat) + ' ' +
                SearchState;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new Exception("dummy Token serializer");
        }


        /// <summary>
        /// dumps a branch of tokens 
        /// </summary>
        public void DumpTokenPath()
        {
            DumpTokenPath(true);
        }
        /// <summary>
        /// dumps a branch of tokens
        /// </summary>
        /// <param name="includeHmmStates">if true include all sentence hmm states</param>
        public void DumpTokenPath(Boolean includeHmmStates)
        {
            var token = this;
            var list = new List<Token>();

            while (token != null)
            {
                list.Add(token);
                token = token.Predecessor;
            }
            for (var i = list.Count - 1; i >= 0; i--)
            {
                token = list[i];
                if (includeHmmStates ||
                        (!(token.SearchState is IHMMSearchState)))
                {
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
        public string GetWordPath(Boolean wantFiller, Boolean wantPronunciations)
        {
            var sb = new StringBuilder();
            var token = this;

            while (token != null)
            {
                if (token.IsWord)
                {
                    var wordState = (IWordSearchState)token.SearchState;
                    var pron = wordState.Pronunciation;
                    var word = wordState.Pronunciation.Word;

                    //Console.Out.WriteLine(token.getFrameNumber() + " " + word + " " + token.logLanguageScore + " " + token.logAcousticScore);

                    if (wantFiller || !word.IsFiller)
                    {
                        if (wantPronunciations)
                        {
                            sb.Insert(0, ']');
                            var u = pron.Units;
                            for (var i = u.Length - 1; i >= 0; i--)
                            {
                                if (i < u.Length - 1)
                                    sb.Insert(0, ',');
                                sb.Insert(0, u[i].Name);
                            }
                            sb.Insert(0, '[');
                        }
                        sb.Insert(0, word.Spelling);
                        sb.Insert(0, ' ');
                    }
                }
                token = token.Predecessor;
            }
            return sb.ToString().Trim();
        }
        /// <summary>
        /// Returns the string of words for this token, with no embedded filler words
        /// </summary>
        /// <returns>the string of words</returns>
        public string GetWordPathNoFiller()
        {
            return GetWordPath(false, false);
        }
        /// <summary>
        /// Returns the string of words for this token, with embedded silences
        /// </summary>
        /// <returns>the string of words</returns>
        public string GetWordPath()
        {
            return GetWordPath(true, false);
        }
        /// <summary>
        /// Returns the string of words and units for this token, with embedded silences.
        /// </summary>
        /// <returns>the string of words and units</returns>public IWord getWord() 
        public string GetWordUnitPath()
        {
            var sb = new StringBuilder();
            var token = this;

            while (token != null)
            {
                var searchState = token.SearchState;
                if (searchState is IWordSearchState)
                {
                    var wordState = (IWordSearchState)searchState;
                    var word = wordState.Pronunciation.Word;
                    sb.Insert(0, ' ' + word.Spelling);
                }
                else if (searchState is IUnitSearchState)
                {
                    var unitState = (IUnitSearchState)searchState;
                    var unit = unitState.Unit;
                    sb.Insert(0, ' ' + unit.Name);
                }
                token = token.Predecessor;
            }
            return sb.ToString().Trim();
        }
        /// <summary>
        /// Returns the word of this Token, the search state is a WordSearchState. If the search state is not a
        /// WordSearchState, return null.
        /// </summary>
        /// <returns>the word of this Token, or null if this is not a word token</returns>
        public Word GetWord()
        {
            if (IsWord)
            {
                var wordState = (IWordSearchState)SearchState;
                return wordState.Pronunciation.Word;
            }
            return null;
        }

        /// <summary>
        /// Shows the token count
        /// </summary>
        public static void ShowCount()
        {
            Console.Out.WriteLine("Cur count: " + _curCount + " new " +
                    (_curCount - _lastCount));
            _lastCount = _curCount;
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
        public Boolean Validate()
        {
            return true;
        }

        /// <summary>
        /// Return the DecimalFormat object for formatting the print out of scores.
        /// </summary>
        /// <value>the DecimalFormat object for formatting score print outs</value>
        protected static string ScoreFormat { get; private set; }

        /// <summary>
        /// Return the DecimalFormat object for formatting the print out of numbers
        /// </summary>
        /// <value></value>
        protected static string NumberFormat { get; private set; }

        ///  <summary>  
        /// A {@code Scoreable} comparator that is used to order scoreables according to their score,
        ///  in descending order.
        ///      Note: since a higher score results in a lower natural order,
        ///      statements such as {@code Collections.min(list, Scoreable.COMPARATOR)}
        ///      actually return the Scoreable with the <b>highest</b> score,
        ///      in contrast to the natural meaning of the word "min".   
        ///  </summary>
        ///  <param name="t1"></param>
        ///  <param name="t2"></param>
        ///  <returns></returns>
        int IComparer<IScoreable>.Compare(IScoreable t1, IScoreable t2)
        {
            if (t1.Score > t2.Score)
            {
                return -1;
            }
            if (t1.Score == t2.Score)
            {
                return 0;
            }
            return 1;
        }

        public void Update(Token predecessor, ISearchState nextState, float logEntryScore, float insertionProbability, float languageProbability, int currentFrameNumber)
        {
            Predecessor = predecessor;
            SearchState = nextState;
            Score = logEntryScore;
            InsertionScore = insertionProbability;
            LanguageScore = languageProbability;
            FrameNumber = currentFrameNumber;
        }
    }
}
