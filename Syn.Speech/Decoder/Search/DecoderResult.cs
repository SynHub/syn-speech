using System;
using System.Collections.Generic;
using Syn.Speech.Common;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Result;
using Syn.Speech.Util;
using Util;

namespace Syn.Speech.Decoder.Search
{
    public class DecoderResult: IResult
    {
        private IActiveList activeList;
        private List<IToken> resultList;
        private IAlternateHypothesisManager alternateHypothesisManager;
        private Boolean _isFinal = false;
        private String reference = String.Empty;
        private Boolean wordTokenFirst;
        private int currentFrameNumber = -1;
        private LogMath logMath = null;
        /// <summary>
        ///Creates a result
        ///@param activeList the active list associated with this result
        ///@param resultList the result list associated with this result
        ///@param frameNumber the frame number for this result.
        ///@param isFinal if true, the result is a final result. This means that the last frame in the
        ///       speech segment has been decoded.
        /// <summary>
        public DecoderResult(IActiveList activeList, List<IToken> resultList, int frameNumber, Boolean isFinal) 
        {
            this.activeList = activeList;
            this.resultList = resultList;
            this.currentFrameNumber = frameNumber;
            this._isFinal = isFinal;
            logMath = LogMath.getLogMath();
        }
      

        /// <summary>
        ///Creates a result
        ///@param activeList the active list associated with this result
        ///@param resultList the result list associated with this result
        ///@param frameNumber the frame number for this result.
        ///@param isFinal if true, the result is a final result
        /// <summary>
        public DecoderResult(IAlternateHypothesisManager alternateHypothesisManager,
                IActiveList activeList, List<IToken> resultList, int frameNumber,
                Boolean isFinal, Boolean wordTokenFirst) 
            : this(activeList, resultList, frameNumber, isFinal)
        {
            this.alternateHypothesisManager = alternateHypothesisManager;
            this.wordTokenFirst = wordTokenFirst;
        }

        IToken IResult.getBestToken()
        {
            throw new NotImplementedException();
        }

        void IResult.setReferenceText(string _ref)
        {
            throw new NotImplementedException();
        }

        bool IResult.isFinal()
        {
            throw new NotImplementedException();
        }


        IAlternateHypothesisManager IResult.getAlternateHypothesisManager()
        {
            throw new NotImplementedException();
        }


        List<WordResult> IResult.getTimedBestResult(bool withFillers)
        {
            throw new NotImplementedException();
        }


        LogMath IResult.getLogMath()
        {
            throw new NotImplementedException();
        }

        IActiveList IResult.getActiveTokens()
        {
            throw new NotImplementedException();
        }

        List<IToken> IResult.getResultTokens()
        {
            throw new NotImplementedException();
        }

        int IResult.getFrameNumber()
        {
            throw new NotImplementedException();
        }

        IToken IResult.getBestFinalToken()
        {
            throw new NotImplementedException();
        }

        IToken IResult.getBestActiveToken()
        {
            throw new NotImplementedException();
        }

        IToken IResult.findToken(string text)
        {
            throw new NotImplementedException();
        }

        List<IToken> IResult.findPartialMatchingTokens(string text)
        {
            throw new NotImplementedException();
        }

        IToken IResult.getBestActiveParitalMatchingToken(string text)
        {
            throw new NotImplementedException();
        }

        IFrameStatistics[] IResult.getFrameStatistics()
        {
            throw new NotImplementedException();
        }

        int IResult.getStartFrame()
        {
            throw new NotImplementedException();
        }

        int IResult.getEndFrame()
        {
            throw new NotImplementedException();
        }

        List<IData> IResult.getDataFrames()
        {
            throw new NotImplementedException();
        }

        string IResult.getBestResultNoFiller()
        {
            throw new NotImplementedException();
        }

        string IResult.getBestFinalResultNoFiller()
        {
            throw new NotImplementedException();
        }

        string IResult.getBestPronunciationResult()
        {
            throw new NotImplementedException();
        }

        List<WordResult> IResult.getTimedWordPath(IToken token, bool withFillers)
        {
            throw new NotImplementedException();
        }

        List<WordResult> IResult.getTimedWordTokenLastPath(IToken token, bool withFillers)
        {
            throw new NotImplementedException();
        }

        string IResult.ToString()
        {
            throw new NotImplementedException();
        }

        void IResult.setFinal(bool finalResult)
        {
            throw new NotImplementedException();
        }

        bool IResult.validate()
        {
            throw new NotImplementedException();
        }

        string IResult.getReferenceText()
        {
            throw new NotImplementedException();
        }
    }
}
