using System;
using System.Collections.Generic;
using System.Linq;
using Syn.Speech.Common;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Decoder.Search;
using Syn.Speech.FrontEnd;
using Syn.Speech.Util;

namespace Syn.Speech.Result
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
        private ActiveList activeList;
        private List<Token> resultList;
        private AlternateHypothesisManager alternateHypothesisManager;
        private Boolean _isFinal=false;
        private String reference =String.Empty;
        private Boolean wordTokenFirst;
        private int currentFrameNumber=-1;
        private LogMath logMath = null;

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
                Boolean isFinal, Boolean wordTokenFirst) 
            : this(activeList, resultList, frameNumber, isFinal)
        {
            this.alternateHypothesisManager = alternateHypothesisManager;
            this.wordTokenFirst = wordTokenFirst;
        }


        /// <summary>
        ///Creates a result
        ///@param activeList the active list associated with this result
        ///@param resultList the result list associated with this result
        ///@param frameNumber the frame number for this result.
        ///@param isFinal if true, the result is a final result. This means that the last frame in the
        ///       speech segment has been decoded.
        /// <summary>
        public Result(ActiveList activeList, List<Token> resultList,int frameNumber, Boolean isFinal) 
        {
            this.activeList = activeList;
            this.resultList = resultList;
            this.currentFrameNumber = frameNumber;
            this._isFinal = isFinal;
            logMath = LogMath.getLogMath();
        }


        /// <summary>
        ///Determines if the result is a final result. A final result is guaranteed to no longer be
        ///modified by the SearchManager that generated it. Non-final results can be modifed by a
        ///<code>SearchManager.recognize</code> calls.
        ///@return true if the result is a final result
        /// <summary>
        public Boolean isFinal() 
        {
            return _isFinal;
        }

        /// <summary>
        ///Returns the log math used for this Result.
        ///
        ///@return the log math used
        /// <summary>
        public LogMath getLogMath() 
        {
            return logMath;
        }

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
        public ActiveList getActiveTokens() 
        {
            return activeList;
        }

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
        public List<Token> getResultTokens() 
        {
            return resultList;
        }

        /// <summary>
        ///Returns the AlternateHypothesisManager Used to construct a Lattice
        ///
        ///@return the AlternateHypothesisManager
        /// <summary>
        public AlternateHypothesisManager getAlternateHypothesisManager() 
        {
            return alternateHypothesisManager;
        }

        /// <summary>
        ///Returns the current frame number
        ///
        ///@return the frame number
        /// <summary>
        public int getFrameNumber() 
        {
            return currentFrameNumber;
        }

        /// <summary>
        ///Returns the best scoring final token in the result. A final token is a token that has reached
        ///a final state in the current frame.
        ///
        ///@return the best scoring final token or null
        /// <summary>
        public Token getBestFinalToken() 
        {
            Token bestToken = null;
            foreach (Token token in resultList) 
            {
                if (bestToken == null || token.getScore() > bestToken.getScore()) 
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
        public Token getBestToken() 
        {
            Token bestToken = getBestFinalToken();

            if (bestToken == null) {
                bestToken = getBestActiveToken();
            }

            return bestToken;
        }

        /// <summary>
        ///Returns the best scoring token in the active set
        ///
        ///@return the best scoring token or null
        /// <summary>
        public Token getBestActiveToken() 
        {
            Token bestToken = null;
            if (activeList != null) {
                foreach (Token token in activeList.getTokens()) 
                {
                    if (bestToken == null
                            || token.getScore() > bestToken.getScore()) 
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
        public Token findToken(String text) 
        {
            text = text.Trim();
            foreach (Token token in resultList) 
            {
                if (text.Equals(token.getWordPathNoFiller())) 
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
        public List<Token> findPartialMatchingTokens(String text) 
        {
            List<Token> list = new List<Token>();
            text = text.Trim();
            foreach (Token token in activeList.getTokens()) 
            {
                if (text.StartsWith(token.getWordPathNoFiller())) 
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
        public Token getBestActiveParitalMatchingToken(String text) 
        {
            List<Token> matchingList = findPartialMatchingTokens(text);
            Token bestToken = null;
            foreach (Token token in matchingList) 
            {
                if (bestToken == null || token.getScore() > bestToken.getScore()) 
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
        public IFrameStatistics[] getFrameStatistics() 
        {
            return null; // [[[ TBD: write me ]]]
        }

        /// <summary>
        ///Gets the starting frame number for the result. Note that this method is currently not
        ///implemented, and always returns zero.
        ///
        ///@return the starting frame number for the result
        /// <summary>
        public int getStartFrame() 
        {
            return 0;
        }

        /// <summary>
        ///Gets the ending frame number for the result. Note that this method is currently not
        ///implemented, and always returns zero.
        ///
        ///@return the ending frame number for the result
        /// <summary>
        public int getEndFrame() 
        {
            return 0; // [[[ TBD: write me ]]]
        }

        /// <summary>
        ///Gets the feature frames associated with this result
        ///
        ///@return the set of feature frames associated with this result, or null if the frames are not
        ///        available.
        /// <summary>
        public List<IData> getDataFrames() 
        {
            // find the best token, and then trace back for all the features
            Token token = getBestToken();

            if (token == null)
                return null;

            LinkedList<IData> featureList = new LinkedList<IData>();

            do {
                IData feature = token.getData();
                if (feature != null)
                    featureList.AddFirst(feature);

                token = token.getPredecessor();
            } while (token != null);

            return featureList.ToList();
        }

        /// <summary>
        ///Returns the string of the best result, removing any filler words. This method first attempts
        ///to return the best final result, that is, the result that has reached the final state of the
        ///search space. If there are no best final results, then the best non-final result, that is,
        ///the one that did not reach the final state, is returned.
        ///
        ///@return the string of the best result, removing any filler words
        /// <summary>
        public String getBestResultNoFiller() 
        {
            Token token = getBestToken();
            if (token == null) {
                return "";
            } else {
                return token.getWordPathNoFiller();
            }
        }

        /// <summary>
        ///Returns the string of the best final result, removing any filler words. A final result is a
        ///path that has reached the final state. A Result object can also contain paths that did not
        ///reach the final state, and those paths are not returned by this method.
        ///
        ///@return the string of the best result, removing any filler words, or null if there are no
        ///        best results
        /// <summary>
        public String getBestFinalResultNoFiller() 
        {
            Token token = getBestFinalToken();
            if (token == null) {
                return "";
            } else {
                return token.getWordPathNoFiller();
            }
        }


        /// <summary>
        ///The method is used when the application wants the phonemes on the best final path. Note that
        ///words may have more than one pronunciation, so this is not equivalent to the word path e.g.
        ///one[HH,W,AH,N] to[T,UW] three[TH,R,IY]
        ///
        ///@return the String of words and associated phonemes on the best path
        /// <summary>
        public String getBestPronunciationResult() 
        {
            Token token = getBestFinalToken();
            if (token == null) {
                return "";
            } 
            else {
                return token.getWordPath(false, true);
            }
        }


        /// <summary>
        ///Returns the string of words (with timestamp) for this token.
        ///
        ///@param withFillers true if we want filler words included, false otherwise
        ///@param wordTokenFirst true if the word tokens come before other types of tokens
        ///@return the string of words
        /// <summary>
        public List<WordResult> getTimedBestResult(Boolean withFillers) 
        {
            Token token = getBestToken();
            if (token == null) 
            {
                return (List<WordResult>)new List<WordResult> { };
            } 
            else {
                if (wordTokenFirst) {
                    return getTimedWordPath(token, withFillers);
                } else {
                    return getTimedWordTokenLastPath(token, withFillers);
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
        public List<WordResult> getTimedWordPath(Token token, Boolean withFillers) 
        {
            // Get to the first emitting token.
            while (token != null && !token.isEmitting()) 
            {
                token = token.getPredecessor();
            }

            List<WordResult> result = new List<WordResult>();
            if (token != null) 
            {
                IData prevWordFirstFeature = token.getData();
                IData prevFeature = prevWordFirstFeature;
                token = token.getPredecessor();

                while (token != null) 
                {
                    if (token.isWord()) {
                        IWord word = token.getWord();
                        if (withFillers || !word.isFiller()) 
                        {
                            TimeFrame timeFrame =
                                    new TimeFrame(
                                            ((FloatData) prevFeature)
                                                    .getCollectTime(),
                                            ((FloatData) prevWordFirstFeature)
                                                    .getCollectTime());
                            result.Add(new WordResult(word, timeFrame, token
                                    .getScore(), 1.0f));
                        }
                        prevWordFirstFeature = prevFeature;
                    }
                    IData feature = token.getData();
                    if (feature != null) {
                        prevFeature = feature;
                    }
                    token = token.getPredecessor();
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
        public  List<WordResult> getTimedWordTokenLastPath(Token token, Boolean withFillers) 
        {
            IWord word = null;
            IData lastFeature = null;
            IData lastWordFirstFeature = null;
            List<WordResult> result = new List<WordResult>();

            while (token != null) 
            {
                if (token.isWord()) 
                {
                    if (word != null && lastFeature != null) {
                        if (withFillers || !word.isFiller()) {
                            TimeFrame timeFrame = new TimeFrame(((FloatData) lastFeature).getCollectTime(),
                                    ((FloatData) lastWordFirstFeature).getCollectTime());
                            result.Add(new WordResult(word, timeFrame, token.getScore(), 1.0f));
                        }
                        word = token.getWord();
                        lastWordFirstFeature = lastFeature;
                    }
                    word = token.getWord();
                }
                IData feature = token.getData();
                if (feature != null) {
                    lastFeature = feature;
                    if (lastWordFirstFeature == null) {
                        lastWordFirstFeature = lastFeature;
                    }
                }
                token = token.getPredecessor();
            }
            result.Reverse();
            return result;
        }

        /// <summary> Returns a string representation of this object/// <summary>
        override public String ToString() 
        {
            Token token = getBestToken();
            if (token == null) {
                return "";
            } else {
                return token.getWordPath();
            }
        }


        /// <summary>
        ///Sets the results as a final result
        ///
        ///@param finalResult if true, the result should be made final
        /// <summary>
        public void setFinal(Boolean finalResult) 
        {
            this._isFinal = finalResult;
        }


        /// <summary>
        ///Determines if the Result is valid. This is used for testing and debugging
        ///
        /// @return true if the result is properly formed.
        /// <summary>
        public Boolean validate() 
        {
            Boolean valid = true;
            foreach (Token token in activeList.getTokens()) 
            {
                if (!token.validate()) 
                {
                    valid = false;
                    token.dumpTokenPath();
                }
            }
            return valid;
        }


        /// <summary>
        ///Sets the reference text
        ///
        ///@param ref the reference text
        /// <summary>
        public void setReferenceText(String _ref) 
        {
            reference = _ref;
        }

        /// <summary>
        ///Retrieves the reference text. The reference text is a transcript of the text that was spoken.
        ///
        ///@return the reference text or null if no reference text exists.
        /// <summary>
        public String getReferenceText() 
        {
            return reference;
        }


    }
}
