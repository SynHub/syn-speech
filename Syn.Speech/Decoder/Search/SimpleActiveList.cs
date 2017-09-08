using System.Collections.Generic;
using Syn.Speech.Common;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    public class SimpleActiveList : ActiveList
    {

        private int absoluteBeamWidth = 2000;
        private float logRelativeBeamWidth;
        private Token bestToken;
        private List<Token> tokenList = new List<Token>();


        /**
         * Creates an empty active list
         *
         * @param absoluteBeamWidth    the absolute beam width
         * @param logRelativeBeamWidth the relative beam width (in the log domain)
         */
        public SimpleActiveList(int absoluteBeamWidth,
                                float logRelativeBeamWidth)
        {
            this.absoluteBeamWidth = absoluteBeamWidth;
            this.logRelativeBeamWidth = logRelativeBeamWidth;
        }


        /**
         * Adds the given token to the list
         *
         * @param token the token to add
         */
        public override void add(Token token)
        {
            tokenList.Add(token);
            if (bestToken == null || token.getScore() > bestToken.getScore())
            {
                bestToken = token;
            }
        }


        /**
         * Replaces an old token with a new token
         *
         * @param oldToken the token to replace (or null in which case, replace works like add).
         * @param newToken the new token to be placed in the list.
         */
        public void replace(Token oldToken, Token newToken)
        {
            add(newToken);
            if (oldToken != null)
            {
                if (!tokenList.Remove(oldToken))
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
        public override ActiveList purge()
        {
            if (absoluteBeamWidth > 0 && tokenList.Count > absoluteBeamWidth)
            {
                tokenList.Sort(new ScoreableComparator());
                //List<Token>(tokenList, Scoreable.COMPARATOR);
                tokenList = tokenList.GetRange(0, absoluteBeamWidth);
            }
            return this;
        }


        /**
         * Retrieves the iterator for this tree.
         *
         * @return the iterator for this token list
         */
        public IEnumerator<Token> iterator()
        {
            return tokenList.GetEnumerator();
        }


        /**
         * Gets the set of all tokens
         *
         * @return the set of tokens
         */
        public override List<Token> getTokens()
        {
            return tokenList;
        }


        /**
         * Returns the number of tokens on this active list
         *
         * @return the size of the active list
         */
        public override int size()
        {
            return tokenList.Count;
        }


        /**
         * gets the beam threshold best upon the best scoring token
         *
         * @return the beam threshold
         */
        public override float getBeamThreshold()
        {
            return getBestScore() + logRelativeBeamWidth;
        }


        /**
         * gets the best score in the list
         *
         * @return the best score
         */
        public override float getBestScore()
        {
            float bestScore = -float.MaxValue;
            if (bestToken != null)
            {
                bestScore = bestToken.getScore();
            }
            return bestScore;
        }


        /**
         * Sets the best scoring token for this active list
         *
         * @param token the best scoring token
         */
        public override void setBestToken(Token token)
        {
            bestToken = token;
        }


        /**
         * Gets the best scoring token for this active list
         *
         * @return the best scoring token
         */
        public override Token getBestToken()
        {
            return bestToken;
        }


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.decoder.search.ActiveList#createNew()
        */
        public override ActiveList newInstance()
        {
            return new SimpleActiveList(absoluteBeamWidth, logRelativeBeamWidth);
        }
    }
}
