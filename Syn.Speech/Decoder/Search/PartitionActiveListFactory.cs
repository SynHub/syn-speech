using System;
using System.Collections.Generic;
using System.Linq;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// A factory for PartitionActiveLists
    /// </summary>
    public class PartitionActiveListFactory : ActiveListFactory
    {
        /**
        /// 
        /// @param absoluteBeamWidth
        /// @param relativeBeamWidth
         */
        public PartitionActiveListFactory(int absoluteBeamWidth, double relativeBeamWidth) 
            :base(absoluteBeamWidth, relativeBeamWidth)
        {
            
        }

        public PartitionActiveListFactory() 
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
        public override ActiveList newInstance() 
        {
            return new PartitionActiveList(absoluteBeamWidth, logRelativeBeamWidth,this);
        }

    }
    /**
       /// An active list that does absolute beam with pruning by partitioning the
       /// token list based on absolute beam width, instead of sorting the token
       /// list, and then chopping the list up with the absolute beam width. The
       /// expected run time of this partitioning algorithm is O(n), instead of O(n log n) 
       /// for merge sort.
       /// <p/>
       /// This class is not thread safe and should only be used by a single thread.
       /// <p/>
       /// Note that all scores are maintained in the LogMath log base.
        */
    class PartitionActiveList:ActiveList 
    {

        private int _size;
        private int absoluteBeamWidth;
        private float logRelativeBeamWidth;
        private Token bestToken;
        // when the list is changed these things should be
        // changed/updated as well
        private Token[] tokenList;
        private Partitioner partitioner = new Partitioner();
        PartitionActiveListFactory parent = null;


        /** Creates an empty active list
           /// @param absoluteBeamWidth
           /// @param logRelativeBeamWidth*/
        public PartitionActiveList(int absoluteBeamWidth,
                                    float logRelativeBeamWidth, PartitionActiveListFactory _parent) 
        {
            this.absoluteBeamWidth = absoluteBeamWidth;
            this.logRelativeBeamWidth = logRelativeBeamWidth;
            int listSize = 2000;
            if (absoluteBeamWidth > 0) {
                listSize = absoluteBeamWidth / 3;
            }
            this.tokenList = new Token[listSize];
            parent = _parent;
        }


        /**
           /// Adds the given token to the list
            *
           /// @param token the token to add
            */
        public override void add(Token token) 
        {
            if (_size < tokenList.Length) 
            {
                tokenList[_size] = token;
                //token.setLocation(_size);
                _size++;
            } else {
                // token array too small, double the capacity
                doubleCapacity();
                add(token);
            }
            if (bestToken == null || token.getScore() > bestToken.getScore()) {
                bestToken = token;
            }
        }


        /** Doubles the capacity of the Token array. */
        private void doubleCapacity() 
        {
            Array.Copy(tokenList,tokenList, tokenList.Length* 2);
        }


        /**
           /// Replaces an old token with a new token
            *
           /// @param oldToken the token to replace (or null in which case, replace works like add).
           /// @param newToken the new token to be placed in the list.
            */

        ////TODO: EXTRA
        //public void replace(Token oldToken, Token newToken) 
        //{
        //    if (oldToken != null) {
        //        int location = oldToken.getLocation();
        //        // check to see if the old token is still in the list
        //        if (location != -1 && tokenList[location] == oldToken) {
        //            tokenList[location] = newToken;
        //            newToken.setLocation(location);
        //            oldToken.setLocation(-1);
        //        } else {
        //            add(newToken);
        //        }
        //    } else {
        //        add(newToken);
        //    }
        //    if (bestToken == null || newToken.getScore() > bestToken.getScore()) {
        //        bestToken = newToken;
        //    }
        //}


        /**
           /// Purges excess members. Remove all nodes that fall below the relativeBeamWidth
            *
           /// @return a (possible new) active list
            */
        public override ActiveList purge() 
        {
            // if the absolute beam is zero, this means there
            // should be no constraint on the abs beam size at all
            // so we will only be relative beam pruning, which means
            // that we don't have to sort the list
            if (absoluteBeamWidth > 0) {
                // if we have an absolute beam, then we will
                // need to sort the tokens to apply the beam
                if (_size > absoluteBeamWidth) {
                    _size = partitioner.partition(tokenList, _size,
                            absoluteBeamWidth) + 1;
                }
            }
            return this;
        }


        /**
           /// gets the beam threshold best upon the best scoring token
            *
           /// @return the beam threshold
            */
        public override float getBeamThreshold() 
        {
            return getBestScore() + logRelativeBeamWidth;
        }


        /**
           /// gets the best score in the list
            *
           /// @return the best score
            */
        public override float getBestScore() 
        {
            float bestScore = -float.MaxValue;
            if (bestToken != null) {
                bestScore = bestToken.getScore();
            }
            // A sanity check
            // for (Token t : this) {
            //    if (t.getScore() > bestScore) {
            //         System.out.println("GBS: found better score "
            //             + t + " vs. " + bestScore);
            //    }
            // }
            return bestScore;
        }


        /**
           /// Sets the best scoring token for this active list
            *
           /// @param token the best scoring token
            */
        public override void setBestToken(Token token) 
        {
            bestToken = token;
        }


        /**
           /// Gets the best scoring token for this active list
            *
           /// @return the best scoring token
            */
        public override Token getBestToken() {
            return bestToken;
        }


        /**
           /// Retrieves the iterator for this tree.
            *
           /// @return the iterator for this token list
            */
        public IEnumerator<Token> iterator() 
        {
            return tokenList.ToList().GetEnumerator();
        }


        /**
           /// Gets the list of all tokens
            *
           /// @return the list of tokens
            */
        public override List<Token> getTokens() 
        {
            return tokenList.ToList();
        }

        /**
           /// Returns the number of tokens on this active list
            *
           /// @return the size of the active list
            */
        public override int size() 
        {
            return _size;
        }


        /* (non-Javadoc)
       /// @see edu.cmu.sphinx.decoder.search.ActiveList#createNew()
        */
        public override ActiveList newInstance() 
        {
            return parent.newInstance();
        }
    }
}    

