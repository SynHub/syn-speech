using System.Collections.Generic;
using System.Linq;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    public class PartitionActiveList: ActiveList 
    {
        private readonly int _absoluteBeamWidth;
        private readonly float _logRelativeBeamWidth;
        private Token _bestToken;
        // when the list is changed these things should be changed/updated as well
        private Token[] _tokenList;
        private readonly Partitioner _partitioner = new Partitioner();
        readonly PartitionActiveListFactory _parent;

        private int _size; //Introduced for property creation

        public PartitionActiveList(int absoluteBeamWidth,
            float logRelativeBeamWidth, PartitionActiveListFactory parent) 
        {
            _absoluteBeamWidth = absoluteBeamWidth;
            _logRelativeBeamWidth = logRelativeBeamWidth;
            int listSize = 2000;
            if (absoluteBeamWidth > 0) {
                listSize = absoluteBeamWidth / 3;
            }
            _tokenList = new Token[listSize];
            _parent = parent;
        }

        /// <summary>
        /// Adds the given token to the list.
        /// </summary>
        /// <param name="token">The token to add</param>
        public override void Add(Token token) 
        {
            if (_size < _tokenList.Length) 
            {
                _tokenList[_size] = token;
                _size++;
            } else {
                // token array too small, double the capacity
                DoubleCapacity();
                Add(token);
            }
            if (_bestToken == null || token.Score > _bestToken.Score) {
                _bestToken = token;
            }
        }


        /// <summary>
        /// Doubles the capacity of the Token array.
        /// </summary>
        private void DoubleCapacity() 
        {
            _tokenList = Arrays.copyOf(_tokenList, _tokenList.Length* 2);
        }

        /// <summary>
        /// Purges excess members. Remove all nodes that fall below the relativeBeamWidth
        /// </summary>
        /// <returns>
        /// a (possible new) active list
        /// </returns>
        public override ActiveList Purge() 
        {
            // if the absolute beam is zero, this means there
            // should be no constraint on the abs beam size at all
            // so we will only be relative beam pruning, which means
            // that we don't have to sort the list
            if (_absoluteBeamWidth > 0) {
                // if we have an absolute beam, then we will
                // need to sort the tokens to apply the beam
                if (_size > _absoluteBeamWidth)
                {
                    _size = _partitioner.Partition(_tokenList, _size,
                        _absoluteBeamWidth) + 1;
                }
            }

            return this;
        }

        /// <summary>
        /// gets the beam threshold best upon the best scoring token
        /// </summary>
        /// <returns>
        /// the beam threshold
        /// </returns>
        public override float GetBeamThreshold() 
        {
            return GetBestScore() + _logRelativeBeamWidth;
        }

        /// <summary>
        /// gets the best score in the list
        /// </summary>
        /// <returns>
        /// the best score
        /// </returns>
        public override float GetBestScore()
        {
            float bestScore = -Float.MAX_VALUE;
            if (_bestToken != null) {
                bestScore = _bestToken.Score;
            }

            return bestScore;
        }


        /// <summary>
        /// Sets the best scoring token for this active list.
        /// </summary>
        /// <param name="token">The best scoring token</param>
        public override void SetBestToken(Token token) 
        {
            _bestToken = token;
        }


        /// <summary>
        ///  Gets the best scoring token for this active list.
        /// </summary>
        /// <returns>
        /// the best scoring token
        /// </returns>
        public override Token GetBestToken() {
            return _bestToken;
        }

        /// <summary>
        /// Retrieves the iterator for this tree.
        /// </summary>
        /// <returns>The iterator for this token list.</returns>
        public IEnumerator<Token> Iterator()
        {
            return new TokenArrayIterator(_tokenList, _size);
        }


        /// <summary>
        /// Gets the list of all tokens
        /// </summary>
        /// <returns>
        /// List of tokens
        /// </returns>
        public override List<Token> GetTokens()
        {
            var toReturn = _tokenList.ToList().GetRange(0, _size);
            this.LogDebug("Token SubRange: {0}:{1}", _tokenList.Length, toReturn.Count);
            return toReturn;
        }

       
        /// <summary>
        /// Returns the number of tokens on this active list.
        /// </summary>
        /// <value>
        /// the size
        /// </value>
        public override int Size { get { return _size; } }

        public override ActiveList NewInstance() 
        {
            return _parent.NewInstance();
        }
    }
}