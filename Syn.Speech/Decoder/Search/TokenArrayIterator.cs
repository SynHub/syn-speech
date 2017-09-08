using System.Collections;
using System.Collections.Generic;
using Syn.Speech.Helper;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    class TokenArrayIterator : IEnumerator<Token> {

        private  Token[] tokenArray;
        private  int size;
        private int pos;


        TokenArrayIterator(Token[] tokenArray, int size) {
            this.tokenArray = tokenArray;
            this.pos = 0;
            this.size = size;
        }


        /** Returns true if the iteration has more tokens. */
        public bool hasNext() {
            return pos < size;
        }


        /** Returns the next token in the iteration. */
        public Token next() {
            if (pos >= tokenArray.Length) {
                throw new NoSuchElementException();
            }
            return tokenArray[pos++];
        }


        /** Unimplemented, throws an Error if called. */
        public void remove() {
            throw new Error("TokenArrayIterator.remove() unimplemented");
        }

        public void Dispose()
        {
            //throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
            return this.hasNext();
        }

        public void Reset()
        {
            //throw new System.NotImplementedException();
        }

        public Token Current { get { return this.next(); } }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}