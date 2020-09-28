using System.Collections;
using System.Collections.Generic;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    class TokenArrayIterator : IEnumerator<Token> {

        private readonly Token[] _tokenArray;
        private readonly int _size;
        private int _pos;


        public TokenArrayIterator(Token[] tokenArray, int size) {
            _tokenArray = tokenArray;
            _pos = 0;
            _size = size;
        }


        /// <summary>
        /// Returns true if the iteration has more tokens.
        /// </summary>
        /// <returns></returns>
        public bool HasNext() {
            return _pos < _size;
        }


        /// <summary>
        /// Returns the next token in the iteration.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NoSuchElementException"></exception>
        public Token Next() {
            if (_pos >= _tokenArray.Length) {
                throw new NoSuchElementException();
            }
            return _tokenArray[_pos++];
        }


        /** Unimplemented, throws an Error if called. */
        public void Remove() {
            throw new Error("TokenArrayIterator.remove() unimplemented");
        }

        public void Dispose()
        {
            //throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
            return HasNext();
        }

        public void Reset()
        {
            //throw new System.NotImplementedException();
        }

        public Token Current { get { return Next(); } }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}