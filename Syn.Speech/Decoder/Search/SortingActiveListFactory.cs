using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syn.Speech.Common;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// @author plamere
    /// </summary>
    public class SortingActiveListFactory: ActiveListFactory
    {
        /**
        /// @param absoluteBeamWidth
        /// @param relativeBeamWidth
        /// @param logMath
         */
        public SortingActiveListFactory(int absoluteBeamWidth,
                double relativeBeamWidth)
            :base(absoluteBeamWidth, relativeBeamWidth)
        {
            
        }

        public SortingActiveListFactory() 
        {

        }
    
        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        
        public new void newProperties(PropertySheet ps)
        {
            base.newProperties(ps);
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.decoder.search.ActiveListFactory#newInstance()
        */
        
        override public ActiveList newInstance() 
        {
            SortingActiveList newObject  = new SortingActiveList(absoluteBeamWidth, logRelativeBeamWidth);
            newObject.activeListFactory = this;
            return newObject;
        }

    }
    
    
    /**
       /// An active list that tries to be simple and correct. This type of active list will be slow, but should exhibit
       /// correct behavior. Faster versions of the ActiveList exist (HeapActiveList, TreeActiveList).
       /// <p/>
       /// This class is not thread safe and should only be used by a single thread.
       /// <p/>
       /// Note that all scores are maintained in the LogMath log base.
        */

    class SortingActiveList : ActiveList 
    {

        private static int DEFAULT_SIZE = 1000;
        private int absoluteBeamWidth;
        private float logRelativeBeamWidth;
        private Token bestToken;
        // when the list is changed these things should be
        // changed/updated as well
        private List<Token> tokenList;
        public SortingActiveListFactory activeListFactory=null;

        /** Creates an empty active list
           /// @param absoluteBeamWidth
           /// @param logRelativeBeamWidth*/
        public SortingActiveList(int absoluteBeamWidth, float logRelativeBeamWidth) 
        {
            this.absoluteBeamWidth = absoluteBeamWidth;
            this.logRelativeBeamWidth = logRelativeBeamWidth;

            int initListSize = absoluteBeamWidth > 0 ? absoluteBeamWidth : DEFAULT_SIZE;
            this.tokenList = new List<Token>(initListSize);
        }


        /**
           /// Adds the given token to the list
            *
           /// @param token the token to add
            */
        override public void add(Token token) 
        {
            //token.setLocation(tokenList.Count);
            tokenList.Add(token);
            if (bestToken == null || token.getScore() > bestToken.getScore()) {
                bestToken = token;
            }
        }


        /**
           /// Replaces an old token with a new token
            *
           /// @param oldToken the token to replace (or null in which case, replace works like add).
           /// @param newToken the new token to be placed in the list.
            */

        //TODO: EXTRA
        //override public void replace(Token oldToken, Token newToken) 
        //{
        //    if (oldToken != null) 
        //    {
        //        int location = oldToken.getLocation();
        //        // just a sanity check:
        //        if (tokenList[location] != oldToken) 
        //        {
        //            Trace.WriteLine("SortingActiveList: replace " + oldToken
        //                    + " not where it should have been.  New "
        //                    + newToken + " location is " + location + " found "
        //                    + tokenList[location]);
        //        }
        //        tokenList[location]= newToken;
        //        newToken.setLocation(location);
        //        if (bestToken == null
        //                || newToken.getScore() > bestToken.getScore()) {
        //            bestToken = newToken;
        //        }
        //    } 
        //    else 
        //    {
        //        add(newToken);
        //    }
        //}


        /**
           /// Purges excess members. Reduce the size of the token list to the absoluteBeamWidth
            *
           /// @return a (possible new) active list
            */
        override  public ActiveList purge() 
        {
            // if the absolute beam is zero, this means there
            // should be no constraint on the abs beam size at all
            // so we will only be relative beam pruning, which means
            // that we don't have to sort the list
            if (absoluteBeamWidth > 0 && tokenList.Count > absoluteBeamWidth)
            {
                tokenList.Sort(new ScoreableComparatorToken());
                tokenList = tokenList.Take(absoluteBeamWidth).ToList();
            }
            return this;
        }


        /**
           /// gets the beam threshold best upon the best scoring token
            *
           /// @return the beam threshold
            */
        override public float getBeamThreshold() 
        {
            return getBestScore() + logRelativeBeamWidth;
        }


        /**
           /// gets the best score in the list
            *
           /// @return the best score
            */
        override public float getBestScore() 
        {
            float bestScore = -float.MaxValue;
            if (bestToken != null) {
                bestScore = bestToken.getScore();
            }
            return bestScore;
        }


        /**
           /// Sets the best scoring token for this active list
            *
           /// @param token the best scoring token
            */
        override  public void setBestToken(Token token) 
        {
            bestToken = token;
        }


        /**
           /// Gets the best scoring token for this active list
            *
           /// @return the best scoring token
            */
        override  public Token getBestToken() 
        {
            return bestToken;
        }


        /**
           /// Retrieves the iterator for this tree.
            *
           /// @return the iterator for this token list
            */
        public IEnumerator<Token> iterator() 
        {
            return tokenList.GetEnumerator();
        }


        /**
           /// Gets the list of all tokens
            *
           /// @return the list of tokens
            */
        override public List<Token> getTokens() 
        {
            return tokenList;
        }

        /**
           /// Returns the number of tokens on this active list
            *
           /// @return the size of the active list
            */
        override public int size() 
        {
            return tokenList.Count;
        }


        /* (non-Javadoc)
       /// @see edu.cmu.sphinx.decoder.search.ActiveList#newInstance()
        */
        override public ActiveList newInstance() 
        {
            if(activeListFactory!=null)
                return activeListFactory.newInstance();
            return null;
        }

        
    }

    
}
