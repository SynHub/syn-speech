using System;

//PATROLLED
using Syn.Speech.Decoders.Search.Comparator;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// Partitions a list of tokens according to the token score, used
    /// in {@link PartitionActiveListFactory}. This method is supposed 
    /// to provide O(n) performance so it's more preferable than 
    /// </summary>
    public class Partitioner
    {
        /// <summary>
        /// Max recursion depth
        /// </summary>
        private const int MaxDepth = 50;


        /**
        /// Partitions sub-array of tokens around the end token. 
        /// Put all elements less or equal then pivot to the start of the array,
        /// shifting new pivot position
         *
        /// @param tokens the token array to partition
        /// @param start      the starting index of the subarray
        /// @param end      the pivot and the ending index of the subarray, inclusive
        /// @return the index (after partitioning) of the element around which the array is partitioned
         */
        private int EndPointPartition(Token[] tokens, int start, int end) 
        {
            Token pivot = tokens[end];
            float pivotScore = pivot.Score;
               
            int i = start;
            int j = end - 1;
        
            while (true) {

                while (i < end && tokens[i].Score >= pivotScore)
                    i++;                
                while (j > i && tokens[j].Score < pivotScore)
                    j--;
            
                if (j <= i)
                    break;
            
                Token current = tokens[j];
                SetToken(tokens, j, tokens[i]);
                SetToken(tokens, i, current);            
            }

            SetToken(tokens, end, tokens[i]);
            SetToken(tokens, i, pivot);
            return i;
        }


        /**
        /// Partitions sub-array of tokens around the x-th token by selecting the midpoint of the token array as the pivot.
        /// Partially solves issues with slow performance on already sorted arrays.
         *
        /// @param tokens the token array to partition
        /// @param start      the starting index of the subarray
        /// @param end      the ending index of the subarray, inclusive
        /// @return the index of the element around which the array is partitioned
         */
        private int MidPointPartition(Token[] tokens, int start, int end)
        {
            //int middle = (start + end) >>> 1; //TODO: CHECK SEMANTICS
            int middle = Java.TripleShift((start + end), 1);
            Token temp = tokens[end];
            SetToken(tokens, end, tokens[middle]);
            SetToken(tokens, middle, temp);
            return EndPointPartition(tokens, start, end);
        }


        /**
        /// Partitions the given array of tokens in place, so that the highest scoring n token will be at the beginning of
        /// the array, not in any order.
         *
        /// @param tokens the array of tokens to partition
        /// @param size   the number of tokens to partition
        /// @param n      the number of tokens in the final partition
        /// @return the index of the last element in the partition
         */
        public int Partition(Token[] tokens, int size, int n)
        {
            if (tokens.Length > n) {
                return MidPointSelect(tokens, 0, size - 1, n, 0);
            }
            return FindBest(tokens, size);
        }

        /**
        /// Simply find the best token and put it in the last slot
        /// 
        /// @param tokens array of tokens
        /// @param size the number of tokens to partition
        /// @return index of the best token
         */
        private int FindBest(Token[] tokens, int size) 
        {
            int r = -1;
            float lowestScore = Float.MAX_VALUE;
            for (int i = 0; i < tokens.Length; i++) 
            {
                float currentScore = tokens[i].Score;
                if (currentScore <= lowestScore) {
                    lowestScore = currentScore;
                    r = i; // "r" is the returned index
                }
            }

            // exchange tokens[r] <=> last token,
            // where tokens[r] has the lowest score
            int last = size - 1;
            if (last >= 0) {
                Token lastToken = tokens[last];
                SetToken(tokens, last, tokens[r]);
                SetToken(tokens, r, lastToken);
            }

            // return the last index
            return last;
        }

        private static void SetToken(Token[] list, int index, Token token) 
        {
            list[index] = token;
        }

        /**
        /// Selects the token with the ith largest token score.
         *
        /// @param tokens       the token array to partition
        /// @param start        the starting index of the subarray
        /// @param end          the ending index of the subarray, inclusive
        /// @param targetSize   target size of the partition
        /// @param depth        recursion depth to avoid stack overflow and fall back to simple partition.
        /// @return the index of the token with the ith largest score
         */
        private int MidPointSelect(Token[] tokens, int start, int end, int targetSize, int depth) 
        {
            if (depth > MaxDepth) {
                return SimplePointSelect (tokens, start, end, targetSize);
            }
            if (start == end) {
                return start;
            }
            int partitionToken = MidPointPartition(tokens, start, end);
            int newSize = partitionToken - start + 1;
            if (targetSize == newSize) {
                return partitionToken;
            }
            if (targetSize < newSize) {
                return MidPointSelect(tokens, start, partitionToken - 1, targetSize, depth + 1);
            }
            return MidPointSelect(tokens, partitionToken + 1, end, targetSize - newSize, depth + 1);
        }
    
        /**
        /// Fallback method to get the partition
         *
        /// @param tokens       the token array to partition
        /// @param start        the starting index of the subarray
        /// @param end          the ending index of the subarray, inclusive
        /// @param targetSize   target size of the partition
        /// @return the index of the token with the ith largest score
         */
        private static int SimplePointSelect(Token[] tokens, int start, int end, int targetSize) 
        {
            Array.Sort(tokens, start, (end + 1) - start, new ScoreableComparator());
            return start + targetSize - 1;
        }

    }
}
