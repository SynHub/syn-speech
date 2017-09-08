using System;
using System.Collections.Generic;
using Syn.Speech.Decoders.Search;
using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// Provides recognition results. Results can be partial or final. A result should not be modified
    /// before it is a final result. Note that a result may not contain all possible information.
    ///
    /// The following methods are not yet defined but should be:
    ///
    ///
    /// public Result getDAG(int compressionLevel);
    ///
    /// </summary>
    public class Result
    {
        private Boolean _isFinal;
        private readonly bool _toCreateLattice;

        /// <summary>
        /// Determines if the result is a final result. A final result is guaranteed to no longer be
        /// modified by the SearchManager that generated it. Non-final results can be modifed by a
        /// <code>SearchManager.recognize</code> calls.
        /// </summary>
        /// <returns>true if the result is a final result</returns>

        /// <summary>
        ///Creates a result
        ///@param activeList the active list associated with this result
        ///@param resultList the result list associated with this result
        ///@param frameNumber the frame number for this result.
        ///@param isFinal if true, the result is a final result
        /// <summary>
        public Result(AlternateHypothesisManager alternateHypothesisManager,
                ActiveList activeList, List<Token> resultList, int frameNumber,
                Boolean isFinal, Boolean wordTokenFirst, bool toCreateLattice)
            : this(activeList, resultList, frameNumber, isFinal, toCreateLattice)
        {
            this.AlternateHypothesisManager = alternateHypothesisManager;
            this.WordTokenFirst = wordTokenFirst;
        }


        /// <summary>
        ///Creates a result
        ///@param activeList the active list associated with this result
        ///@param resultList the result list associated with this result
        ///@param frameNumber the frame number for this result.
        ///@param isFinal if true, the result is a final result. This means that the last frame in the
        ///       speech segment has been decoded.
        /// <summary>
        public Result(ActiveList activeList, List<Token> resultList,
            int frameNumber, bool isFinal, bool toCreateLattice)
        {
            this.ActiveTokens = activeList;
            this.ResultTokens = resultList;
            FrameNumber = frameNumber;
            _isFinal = isFinal;
            this._toCreateLattice = toCreateLattice;
            LogMath = LogMath.GetLogMath();
        }


        /// <summary>
        ///Determines if the result is a final result. A final result is guaranteed to no longer be
        ///modified by the SearchManager that generated it. Non-final results can be modifed by a
        ///<code>SearchManager.recognize</code> calls.
        ///@return true if the result is a final result
        /// <summary>
        public Boolean IsFinal()
        {
            return _isFinal;
        }

        /// <summary>
        ///Returns the log math used for this Result.
        ///
        ///@return the log math used
        /// <summary>
        public LogMath LogMath { get; private set; }

        /// <summary>
        ///Returns a list of active tokens for this result. The list contains zero or active
        ///<code>Token</code> objects that represents the leaf nodes of all active branches in the
        ///result (sometimes referred to as the 'lattice').
        ///<p/>
        ///The lattice is live and may be modified by a SearchManager during a recognition. Once the
        ///Result is final, the lattice is fixed and will no longer be modified by the SearchManager.
        ///Applications can modify the lattice (to prepare for a re-recognition, for example) only after
        ///<code>isFinal</code> returns <code>true</code>
        ///
        ///@return a list containing the active tokens for this result
        ///@see Token
        /// <summary>
        public ActiveList ActiveTokens { get; private set; }

        /// <summary>
        ///Returns a list of result tokens for this result. The list contains zero or more result
        ///<code>Token</code> objects that represents the leaf nodes of all final branches in the result
        ///(sometimes referred to as the 'lattice').
        ///<p/>
        ///The lattice is live and may be modified by a SearchManager during a recognition. Once the
        ///Result is final, the lattice is fixed and will no longer be modified by the SearchManager.
        ///Applications can modify the lattice (to prepare for a re-recognition, for example) only after
        ///<code>isFinal</code> returns <code>true</code>
        ///
        ///return a list containing the final result tokens for this result
        ///see Token
        /// <summary>
        /// 
        public List<Token> ResultTokens { get; private set; }

        /// <summary>
        ///Returns the AlternateHypothesisManager Used to construct a Lattice
        ///
        ///@return the AlternateHypothesisManager
        /// <summary>
        public AlternateHypothesisManager AlternateHypothesisManager { get; private set; }

        /// <summary>
        ///Returns the current frame number
        ///
        ///@return the frame number
        /// <summary>
        public int FrameNumber { get; private set; }

        /// <summary>
        ///Returns the best scoring final token in the result. A final token is a token that has reached
        ///a final state in the current frame.
        ///
        ///@return the best scoring final token or null
        /// <summary>
        public Token GetBestFinalToken()
        {
            Token bestToken = null;
            foreach (Token token in ResultTokens)
            {
                if (bestToken == null || token.Score > bestToken.Score)
                {
                    bestToken = token;
                }
            }
            return bestToken;
        }

        /// <summary>
        ///Returns the best scoring token in the result. First, the best final token is retrieved. A
        ///final token is one that has reached the final state in the search space. If no final tokens
        ///can be found, then the best, non-final token is returned.
        ///
        ///@return the best scoring token or null
        /// <summary>
        public Token GetBestToken()
        {
            Token bestToken = GetBestFinalToken();

            if (bestToken == null)
            {
                bestToken = GetBestActiveToken();
            }

            return bestToken;
        }

        /// <summary>
        ///Returns the best scoring token in the active set
        ///
        ///@return the best scoring token or null
        /// <summary>
        public Token GetBestActiveToken()
        {
            Token bestToken = null;
            if (ActiveTokens != null)
            {
                foreach (Token token in ActiveTokens)
                {
                    if (bestToken == null
                            || token.Score > bestToken.Score)
                    {
                        bestToken = token;
                    }
                }
            }
            return bestToken;
        }

        /// <summary>
        ///Searches through the n-best list to find the the branch that matches the given string
        ///
        ///@param text the string to search for
        ///@return the token at the head of the branch or null
        /// <summary>
        public Token FindToken(String text)
        {
            text = text.Trim();
            foreach (Token token in ResultTokens)
            {
                if (text.Equals(token.GetWordPathNoFiller()))
                {
                    return token;
                }
            }
            return null;
        }

        /// <summary>
        ///Searches through the n-best list to find the the branch that matches the beginning of the
        ///given string
        ///
        ///@param text the string to search for
        ///@return the list token at the head of the branch
        /// <summary>
        public List<Token> FindPartialMatchingTokens(String text)
        {
            List<Token> list = new List<Token>();
            text = text.Trim();
            foreach (Token token in ActiveTokens)
            {
                if (text.StartsWith(token.GetWordPathNoFiller()))
                {
                    list.Add(token);
                }
            }
            return list;
        }

        /// <summary>
        ///Returns the best scoring token that matches the beginning of the given text.
        ///
        ///@param text the text to match
        /// <summary>
        public Token GetBestActiveParitalMatchingToken(String text)
        {
            List<Token> matchingList = FindPartialMatchingTokens(text);
            Token bestToken = null;
            foreach (Token token in matchingList)
            {
                if (bestToken == null || token.Score > bestToken.Score)
                {
                    bestToken = token;
                }
            }
            return bestToken;
        }

        /// <summary>
        ///Returns detailed frame statistics for this result
        ///
        ///@return frame statistics for this result as an array, with one element per frame or
        ///        <code>null</code> if no frame statistics are available.
        /// <summary>
        public FrameStatistics[] GetFrameStatistics()
        {
            return null; // [[[ TBD: write me ]]]
        }

        /// <summary>
        ///Gets the starting frame number for the result. Note that this method is currently not
        ///implemented, and always returns zero.
        ///
        ///@return the starting frame number for the result
        /// <summary>
        public int StartFrame
        {
            get { return 0; }
        }

        /// <summary>
        ///Gets the ending frame number for the result. Note that this method is currently not
        ///implemented, and always returns zero.
        ///
        ///@return the ending frame number for the result
        /// <summary>
        public int EndFrame
        {
            get { return 0; // [[[ TBD: write me ]]]
            }
        }

        /// <summary>
        ///Gets the feature frames associated with this result
        ///
        ///@return the set of feature frames associated with this result, or null if the frames are not
        ///        available.
        /// <summary>
        public ICollection<IData> GetDataFrames()
        {
            // find the best token, and then trace back for all the features
            Token token = GetBestToken();

            if (token == null)
                return null;

            var featureList = new List<IData>();  //TODO: Check behaviour because a LinkedList is supposed to be used here

            do
            {
                IData feature = token.Data;
                if (feature != null)
                    featureList.Insert(0, feature);

                token = token.Predecessor;
            } while (token != null);

            return featureList;
        }

        /// <summary>
        ///Returns the string of the best result, removing any filler words. This method first attempts
        ///to return the best final result, that is, the result that has reached the final state of the
        ///search space. If there are no best final results, then the best non-final result, that is,
        ///the one that did not reach the final state, is returned.
        ///
        ///@return the string of the best result, removing any filler words
        /// <summary>
        public string GetBestResultNoFiller()
        {
            Token token = GetBestToken();
            if (token == null)
            {
                return "";
            }
            return token.GetWordPathNoFiller();
        }

        /// <summary>
        ///Returns the string of the best final result, removing any filler words. A final result is a
        ///path that has reached the final state. A Result object can also contain paths that did not
        ///reach the final state, and those paths are not returned by this method.
        ///
        ///@return the string of the best result, removing any filler words, or null if there are no
        ///        best results
        /// <summary>
        public string GetBestFinalResultNoFiller()
        {
            Token token = GetBestFinalToken();
            if (token == null)
            {
                return "";
            }
            return token.GetWordPathNoFiller();
        }


        /// <summary>
        ///The method is used when the application wants the phonemes on the best final path. Note that
        ///words may have more than one pronunciation, so this is not equivalent to the word path e.g.
        ///one[HH,W,AH,N] to[T,UW] three[TH,R,IY]
        ///
        ///@return the string of words and associated phonemes on the best path
        /// <summary>
        public string GetBestPronunciationResult()
        {
            Token token = GetBestFinalToken();
            if (token == null)
            {
                return "";
            }
            return token.GetWordPath(false, true);
        }


        /// <summary>
        ///Returns the string of words (with timestamp) for this token.
        ///
        ///@param withFillers true if we want filler words included, false otherwise
        ///@param wordTokenFirst true if the word tokens come before other types of tokens
        ///@return the string of words
        /// <summary>
        public List<WordResult> GetTimedBestResult(Boolean withFillers)
        {
            Token token = GetBestToken();
            if (token == null)
            {
                return new List<WordResult>();
            }
            else
            {
                if (WordTokenFirst)
                {
                    return GetTimedWordPath(token, withFillers);
                }
                else
                {
                    return GetTimedWordTokenLastPath(token, withFillers);
                }
            }
        }


        /// <summary>
        ///Returns the string of words (with timestamp) for this token. This method assumes that the
        ///word tokens come before other types of token.
        ///
        ///@param withFillers true if we want filler words, false otherwise
        ///@return list of word with timestamps
        /// <summary>
        public List<WordResult> GetTimedWordPath(Token token, Boolean withFillers)
        {
            // Get to the first emitting token.
            while (token != null && !token.IsEmitting)
            {
                token = token.Predecessor;
            }

            List<WordResult> result = new List<WordResult>();
            if (token != null)
            {
                IData prevWordFirstFeature = token.Data;
                IData prevFeature = prevWordFirstFeature;
                token = token.Predecessor;

                while (token != null)
                {
                    if (token.IsWord)
                    {
                        Word word = token.GetWord();
                        if (withFillers || !word.IsFiller)
                        {
                            TimeFrame timeFrame =
                                    new TimeFrame(
                                            ((FloatData)prevFeature).CollectTime,
                                            ((FloatData)prevWordFirstFeature).CollectTime);
                            result.Add(new WordResult(word, timeFrame, token.Score, 1.0f));
                        }
                        prevWordFirstFeature = prevFeature;
                    }
                    IData feature = token.Data;
                    if (feature != null)
                    {
                        prevFeature = feature;
                    }
                    token = token.Predecessor;
                }
            }
            result.Reverse();
            return result;
        }


        /// <summary>
        ///Returns the string of words for this token, each with the starting sample number as the
        ///timestamp. This method assumes that the word tokens come after the unit and hmm tokens.
        ///
        ///@return the string of words, each with the starting sample number
        /// <summary>
        public List<WordResult> GetTimedWordTokenLastPath(Token token, Boolean withFillers)
        {
            Word word = null;
            IData lastFeature = null;
            IData lastWordFirstFeature = null;
            List<WordResult> result = new List<WordResult>();

            while (token != null)
            {
                if (token.IsWord)
                {
                    if (word != null && lastFeature != null)
                    {
                        if (withFillers || !word.IsFiller)
                        {
                            TimeFrame timeFrame = new TimeFrame(((FloatData)lastFeature).CollectTime,
                                    ((FloatData)lastWordFirstFeature).CollectTime);
                            result.Add(new WordResult(word, timeFrame, token.Score, 1.0f));
                        }
                        word = token.GetWord();
                        lastWordFirstFeature = lastFeature;
                    }
                    word = token.GetWord();
                }
                IData feature = token.Data;
                if (feature != null)
                {
                    lastFeature = feature;
                    if (lastWordFirstFeature == null)
                    {
                        lastWordFirstFeature = lastFeature;
                    }
                }
                token = token.Predecessor;
            }
            result.Reverse();
            return result;
        }

        /// <summary> Returns a string representation of this object/// <summary>
        public override string ToString()
        {
            Token token = GetBestToken();
            if (token == null)
            {
                return "";
            }
            else
            {
                return token.GetWordPath();
            }
        }


        /// <summary>
        ///Sets the results as a final result
        ///
        ///@param finalResult if true, the result should be made final
        /// <summary>
        public void SetFinal(Boolean finalResult)
        {
            _isFinal = finalResult;
        }


        /// <summary>
        ///Determines if the Result is valid. This is used for testing and debugging
        ///
        /// @return true if the result is properly formed.
        /// <summary>
        public Boolean Validate()
        {
            Boolean valid = true;
            foreach (Token token in ActiveTokens)
            {
                if (!token.Validate())
                {
                    valid = false;
                    token.DumpTokenPath();
                }
            }
            return valid;
        }


        /// <summary>
        ///Retrieves the reference text. The reference text is a transcript of the text that was spoken.
        ///
        ///@return the reference text or null if no reference text exists.
        /// <summary>
        public string ReferenceText { get; set; }


        /**
    * Getter for wordTokenFirst flag
    * 
    * @return true if word tokens goes first, before data tokens 
    */

        public bool WordTokenFirst { get; private set; }
    }
}
