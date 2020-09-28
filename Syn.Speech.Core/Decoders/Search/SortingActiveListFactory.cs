using System.Collections.Generic;
using Syn.Speech.Decoders.Search.Comparator;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// @author plamere
    /// </summary>
    public class SortingActiveListFactory : ActiveListFactory
    {

        public SortingActiveListFactory(int absoluteBeamWidth,
                double relativeBeamWidth)
            : base(absoluteBeamWidth, relativeBeamWidth)
        {

        }

        public SortingActiveListFactory()
        {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
        }

        public override ActiveList NewInstance()
        {
            var newObject = new SortingActiveList(AbsoluteBeamWidth, LogRelativeBeamWidth) { ActiveListFactory = this };
            return newObject;
        }

    }


    // <summary>
    /// An active list that tries to be simple and correct. This type of active list will be slow, but should exhibit
    /// correct behavior. Faster versions of the ActiveList exist (HeapActiveList, TreeActiveList).
    /// <p/>
    /// This class is not thread safe and should only be used by a single thread.
    /// <p/>
    /// Note that all scores are maintained in the LogMath log base.
    // </summary>

    class SortingActiveList : ActiveList
    {
        private const int DefaultSize = 1000;
        private readonly int _absoluteBeamWidth;
        private readonly float _logRelativeBeamWidth;
        private Token _bestToken;
        // when the list is changed these things should be
        // changed/updated as well
        private List<Token> _tokenList;
        public SortingActiveListFactory ActiveListFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortingActiveList"/> class.
        /// </summary>
        /// <param name="absoluteBeamWidth">Width of the absolute beam.</param>
        /// <param name="logRelativeBeamWidth">Width of the log relative beam.</param>
        public SortingActiveList(int absoluteBeamWidth, float logRelativeBeamWidth)
        {
            _absoluteBeamWidth = absoluteBeamWidth;
            _logRelativeBeamWidth = logRelativeBeamWidth;

            int initListSize = absoluteBeamWidth > 0 ? absoluteBeamWidth : DefaultSize;
            _tokenList = new List<Token>(initListSize);
        }

        public override void Add(Token token)
        {
            //token.setLocation(tokenList.Count);
            _tokenList.Add(token);
            if (_bestToken == null || token.Score > _bestToken.Score)
            {
                _bestToken = token;
            }
        }


        /**
           /// Replaces an old token with a new token
            *
           /// @param oldToken the token to replace (or null in which case, replace works like add).
           /// @param newToken the new token to be placed in the list.
            */


        /**
           /// Purges excess members. Reduce the size of the token list to the absoluteBeamWidth
            *
           /// @return a (possible new) active list
            */
        override public ActiveList Purge()
        {
            // if the absolute beam is zero, this means there
            // should be no constraint on the abs beam size at all
            // so we will only be relative beam pruning, which means
            // that we don't have to sort the list
            if (_absoluteBeamWidth > 0 && _tokenList.Count > _absoluteBeamWidth)
            {
                _tokenList.Sort(new ScoreableComparatorToken());
                _tokenList = _tokenList.GetRange(0, _absoluteBeamWidth);
            }
            return this;
        }


        /**
           /// gets the beam threshold best upon the best scoring token
            *
           /// @return the beam threshold
            */
        public override float GetBeamThreshold()
        {
            return GetBestScore() + _logRelativeBeamWidth;
        }


        /**
           /// gets the best score in the list
            *
           /// @return the best score
            */
        public override float GetBestScore()
        {
            float bestScore = -Float.MAX_VALUE;
            if (_bestToken != null)
            {
                bestScore = _bestToken.Score;
            }
            return bestScore;
        }


        /**
           /// Sets the best scoring token for this active list
            *
           /// @param token the best scoring token
            */
        override public void SetBestToken(Token token)
        {
            _bestToken = token;
        }


        /**
           /// Gets the best scoring token for this active list
            *
           /// @return the best scoring token
            */
        override public Token GetBestToken()
        {
            return _bestToken;
        }


        /**
           /// Retrieves the iterator for this tree.
            *
           /// @return the iterator for this token list
            */
        public IEnumerator<Token> Iterator()
        {
            return _tokenList.GetEnumerator();
        }


        /**
           /// Gets the list of all tokens
            *
           /// @return the list of tokens
            */
        public override List<Token> GetTokens()
        {
            return _tokenList;
        }

        /**
           /// Returns the number of tokens on this active list
            *
           /// @return the size of the active list
            */

        public override int Size
        {
            get { return _tokenList.Count; }
        }


        /* (non-Javadoc)
       /// @see edu.cmu.sphinx.decoder.search.ActiveList#newInstance()
        */
        public override ActiveList NewInstance()
        {
            if (ActiveListFactory != null)
                return ActiveListFactory.NewInstance();
            return null;
        }


    }


}
