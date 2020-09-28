using System.Collections.Generic;
//PATROLLED
using Syn.Speech.Decoders.Search.Comparator;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    public class SimpleActiveList : ActiveList
    {

        private readonly int _absoluteBeamWidth = 2000;
        private readonly float _logRelativeBeamWidth;
        private Token _bestToken;
        private List<Token> _tokenList = new List<Token>();


        /**
         * Creates an empty active list
         *
         * @param absoluteBeamWidth    the absolute beam width
         * @param logRelativeBeamWidth the relative beam width (in the log domain)
         */
        public SimpleActiveList(int absoluteBeamWidth,float logRelativeBeamWidth)
        {
            _absoluteBeamWidth = absoluteBeamWidth;
            _logRelativeBeamWidth = logRelativeBeamWidth;
        }


        /**
         * Adds the given token to the list
         *
         * @param token the token to add
         */
        public override void Add(Token token)
        {
            _tokenList.Add(token);
            if (_bestToken == null || token.Score > _bestToken.Score)
            {
                _bestToken = token;
            }
        }


        /**
         * Replaces an old token with a new token
         *
         * @param oldToken the token to replace (or null in which case, replace works like add).
         * @param newToken the new token to be placed in the list.
         */
        public void Replace(Token oldToken, Token newToken)
        {
            Add(newToken);
            if (oldToken != null)
            {
                if (!_tokenList.Remove(oldToken))
                {
                    // Some optional debugging code here to dump out the paths
                    // when this "should never happen" error happens
                    // System.out.println("SimpleActiveList: remove "
                    //         + oldToken + " missing, but replaced by "
                    //         + newToken);
                    // oldToken.dumpTokenPath(true);
                    // newToken.dumpTokenPath(true);
                }
            }
        }


        /**
         * Purges excess members. Remove all nodes that fall below the relativeBeamWidth
         *
         * @return a (possible new) active list
         */
        public override ActiveList Purge()
        {
            if (_absoluteBeamWidth > 0 && _tokenList.Count > _absoluteBeamWidth)
            {
                _tokenList.Sort(new ScoreableComparator());
                //List<Token>(tokenList, Scoreable.COMPARATOR);
                _tokenList = _tokenList.GetRange(0, _absoluteBeamWidth);
            }
            return this;
        }


        /**
         * Retrieves the iterator for this tree.
         *
         * @return the iterator for this token list
         */
        public IEnumerator<Token> Iterator()
        {
            return _tokenList.GetEnumerator();
        }


        /**
         * Gets the set of all tokens
         *
         * @return the set of tokens
         */
        public override List<Token> GetTokens()
        {
            return _tokenList;
        }


        /**
         * Returns the number of tokens on this active list
         *
         * @return the size of the active list
         */

        public override int Size
        {
            get { return _tokenList.Count; }
        }


        /**
         * gets the beam threshold best upon the best scoring token
         *
         * @return the beam threshold
         */
        public override float GetBeamThreshold()
        {
            return GetBestScore() + _logRelativeBeamWidth;
        }


        /**
         * gets the best score in the list
         *
         * @return the best score
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
         * Sets the best scoring token for this active list
         *
         * @param token the best scoring token
         */
        public override void SetBestToken(Token token)
        {
            _bestToken = token;
        }


        /**
         * Gets the best scoring token for this active list
         *
         * @return the best scoring token
         */
        public override Token GetBestToken()
        {
            return _bestToken;
        }


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.decoder.search.ActiveList#createNew()
        */
        public override ActiveList NewInstance()
        {
            return new SimpleActiveList(_absoluteBeamWidth, _logRelativeBeamWidth);
        }
    }
}
