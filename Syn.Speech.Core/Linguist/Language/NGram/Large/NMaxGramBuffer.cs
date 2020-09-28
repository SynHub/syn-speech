using System;
//REFACTORED
namespace Syn.Speech.Linguist.Language.NGram.Large
{
    /// <summary>
    /// Implements a buffer that contains NGrams of model's MAX order. 
    /// It assumes that the first two bytes of each n-gram entry is the 
    /// ID of the n-gram.
    /// </summary>
    public class NMaxGramBuffer : NGramBuffer
    {
        /**
        /// Constructs a NMaxGramBuffer object with the given byte[].
         *
        /// @param buffer       the byte[] with NGrams
        /// @param numberNGrams the number of N-gram
        /// @param bigEndian	   the buffer's endianness
        /// @param is32bits     whether the buffer is 16 or 32 bits
        /// @param n	           the buffer's order
        /// @param firstCurrentNGramEntry the first Current NGram Entry
        */
        public NMaxGramBuffer(sbyte[] buffer, int numberNGrams, Boolean bigEndian, Boolean is32Bits, int n, int firstCurrentNGramEntry) 
            :base(buffer, numberNGrams, bigEndian, is32Bits, n, firstCurrentNGramEntry)
        {
        }


        /**
        /// Returns the NGramProbability of the nth follower.
         *
        /// @param nthFollower which follower
        /// @return the NGramProbability of the nth follower
         */
        public override int GetProbabilityID(int nthFollower) 
        {
    	    int nthPosition;
    	
    	    nthPosition = nthFollower* LargeNGramModel.BytesPerNmaxgram* ((Is32Bits) ? 4 : 2);
    	    Position = nthPosition + ((Is32Bits) ? 4 : 2); // to skip the word ID
    	
            return ReadBytesAsInt();
        }
    
    
        /**
        /// Finds the NGram probabilities for the given nth word in a NGram.
         *
        /// @param nthWordID the ID of the nth word
        /// @return the NGramProbability of the given nth word
         */
        public override NGramProbability FindNGram(int nthWordID) 
        {

            int mid, start = 0, end = NumberNGrams;
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
        /// Returns the NGramProbability of the nth follower.
         *
        /// @param nthFollower which follower
        /// @return the NGramProbability of the nth follower
         */

        public override NGramProbability GetNGramProbability(int nthFollower)
        {
    	    const int backoffID = 0;
            const int firstNGram = 0;

            var nthPosition = nthFollower* LargeNGramModel.BytesPerNmaxgram* ((Is32Bits) ? 4 : 2);

            Position = nthPosition;
        
            var wordID = ReadBytesAsInt();
            var probID = ReadBytesAsInt();
            
            return (new NGramProbability(nthFollower, wordID, probID, backoffID, firstNGram));
        }

    }
}
