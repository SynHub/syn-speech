using System;
//REFACTORED
namespace Syn.Speech.Linguist.Language.NGram.Large
{
    /// <summary>
    /// Implements a buffer that contains NGrams. It assumes that the first two bytes of each n-gram entry is the ID of the
    /// n-gram.
    /// </summary>
    public class NGramBuffer
    {
        /**
        /// Constructs a NGramBuffer object with the given byte[].
         *
        /// @param buffer       the byte[] with NGrams
        /// @param numberNGrams the number of N-gram
        /// @param bigEndian	   the buffer's endianness
        /// @param is32bits     whether the buffer is 16 or 32 bits
        /// @param n	           the buffer's order
        /// @param firstNGramEntry  the first NGram Entry
         */
        public NGramBuffer(sbyte[] buffer, int numberNGrams, Boolean bigEndian, Boolean is32Bits, int n, int firstNGramEntry) 
        {
            Buffer = buffer;
            NumberNGrams = numberNGrams;
            IsBigEndian = bigEndian;
            Is32Bits = is32Bits;
            Position = 0;
            N = n;
	    FirstNGramEntry = firstNGramEntry;
        }


        /**
        /// Returns the byte[] of n-grams.
         *
        /// @return the byte[] of n-grams
         */

        public sbyte[] Buffer { get; private set; }

        /**
        /// Returns the firstNGramEntry
        /// @return the firstNGramEntry of the buffer
         */

        public int FirstNGramEntry { get; private set; }


        /**
        /// Returns the size of the buffer in bytes.
         *
        /// @return the size of the buffer in bytes
         */

        public int Size
        {
            get { return Buffer.Length; }
        }


        /**
        /// Returns the number of n-grams in this buffer.
         *
        /// @return the number of n-grams in this buffer
         */

        public int NumberNGrams { get; private set; }


        /**
        /// Returns the position of the buffer.
         *
        /// @return the position of the buffer
         */
        protected int Position { get; set; }

        protected int N { get; private set; }


        /**
        /// Returns the word ID of the nth follower, assuming that the ID is the first two bytes of the NGram entry.
         *
        /// @param nthFollower starts from 0 to (numberFollowers - 1).
        /// @return the word ID
         */
        public int GetWordID(int nthFollower) 
        {
            var nthPosition = nthFollower* (Buffer.Length / NumberNGrams);
            Position = nthPosition;
            return ReadBytesAsInt();
        }


        /**
        /// Returns true if the NGramBuffer is big-endian.
         *
        /// @return true if the NGramBuffer is big-endian, false if little-endian
         */

        public bool IsBigEndian { get; private set; }


        /**
        /// Returns true if the NGramBuffer is 32 bits.
         *
        /// @return true if the NGramBuffer is 32 bits, false if 16 bits
         */

        public bool Is32Bits { get; private set; }

        /**
        /// Reads the next two bytes from the buffer's current position as an integer.
         *
        /// @return the next two bytes as an integer
         */
        public int ReadBytesAsInt()
        {
            if (Is32Bits)
            {
                if (IsBigEndian)
                {
                    var value = (0x000000ff & Buffer[Position++]);
                    value <<= 8;
                    value |= (0x000000ff & Buffer[Position++]);
                    value <<= 8;
                    value |= (0x000000ff & Buffer[Position++]);
                    value <<= 8;
                    value |= (0x000000ff & Buffer[Position++]);
                    return value;
                }
                else
                {
                    var value = (0x000000ff & Buffer[Position + 3]);
                    value <<= 8;
                    value |= (0x000000ff & Buffer[Position + 2]);
                    value <<= 8;
                    value |= (0x000000ff & Buffer[Position + 1]);
                    value <<= 8;
                    value |= (0x000000ff & Buffer[Position]);
                    Position += 4;
                    return value;
                }
            }

            if (IsBigEndian)
            {
                var value = (0x000000ff & Buffer[Position++]);
                value <<= 8;
                value |= (0x000000ff & Buffer[Position++]);
                return value;
            }
            else
            {
                var value = (0x000000ff & Buffer[Position + 1]);
                value <<= 8;
                value |= (0x000000ff & Buffer[Position]);
                Position += 2;
                return value;
            }
        }


        /**
        /// Returns true if this buffer was used in the last utterance.
         *
        /// @return true if this buffer was used in the last utterance
         */

        public bool Used { get;  set; }


        /**
        /// Finds the NGram probability ID for the given nth word in a NGram.
         *
        /// @param nthWordID the ID of the nth word
        /// @return the NGram Probability ID of the given nth word
         */
        public int FindProbabilityID(int nthWordID) 
        {
            int mid, start = 0, end = NumberNGrams;

            var nGram = -1;

            while ((end - start) > 0) {
                mid = (start + end) / 2;
                var midWordID = GetWordID(mid);
                if (midWordID < nthWordID) {
                    start = mid + 1;
                } else if (midWordID > nthWordID) {
                    end = mid;
                } else {
                    nGram = GetProbabilityID(mid);
                    break;
                }
            }
            return nGram;
        }


        /**
        /// Returns the NGramProbability of the nth follower.
         *
        /// @param nthFollower which follower
        /// @return the NGramProbability of the nth follower
         */
        public virtual int GetProbabilityID(int nthFollower) 
        {
            var nthPosition = nthFollower* LargeNGramModel.BytesPerNgram* ((Is32Bits) ? 4 : 2);
    	    Position = nthPosition + ((Is32Bits) ? 4 : 2); // to skip the word ID
    	
            return ReadBytesAsInt();
        }
    
    
        /**
        /// Finds the NGram probabilities for the given nth word in a NGram.
         *
        /// @param nthWordID the ID of the nth word
        /// @return the NGramProbability of the given nth word
         */
        public virtual NGramProbability FindNGram(int nthWordID) 
        {

            int mid, start = 0, end = NumberNGrams - 1;
            NGramProbability ngram = null;

            while ((end - start) > 0) {
                mid = (start + end) / 2;
                var midWordID = GetWordID(mid);
                if (midWordID < nthWordID) {
                    start = mid + 1;
                } else if (midWordID > nthWordID) {
                    end = mid;
                } else {
                    ngram = GetNGramProbability(mid);
                    break;
                }
            }

            return ngram;
        }
    
        /**
        /// Finds the NGram index for the given nth word in a NGram
        /// 
        /// @param nthWordID the ID of the nth word
        /// @return the NGramIndex of the given nth word
         */
        public int FindNGramIndex(int nthWordID) {

            int mid = -1, start = 0, end = NumberNGrams - 1;

            while ((end - start) > 0) {
                mid = (start + end) / 2;
                var midWordID = GetWordID(mid);
                if (midWordID < nthWordID) {
                    start = mid + 1;
                } else if (midWordID > nthWordID) {
                    end = mid;
                } else {
                    break;
                }
            }

            return mid;
        }
    

        /**
        /// Returns the NGramProbability of the nth follower.
         *
        /// @param nthFollower which follower
        /// @return the NGramProbability of the nth follower
         */
        public virtual NGramProbability GetNGramProbability(int nthFollower) {
            var nthPosition = nthFollower* LargeNGramModel.BytesPerNgram* ((Is32Bits) ? 4 : 2);
        
            Position = nthPosition;
        
            var wordID = ReadBytesAsInt();
            var probID = ReadBytesAsInt();
            var backoffID = ReadBytesAsInt();
            var firstNGram = ReadBytesAsInt();
            
            return (new NGramProbability(nthFollower, wordID, probID, backoffID, firstNGram));
        }
    }
}
