//PATROLLED
namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    public class NgramTrie
    {
        private MiddleNgramSet[] middles;
        private LongestNgramSet longest;
        internal NgramTrieBitarr bitArr;
        private int ordersNum;
        internal int quantProbBoLen;
        internal int quantProbLen;

        public NgramTrie(int[] counts, int quantProbBoLen, int quantProbLen)
        {
            int memLen = 0;
            int[] ngramMemSize = new int[counts.Length - 1];
            for (int i = 1; i <= counts.Length - 1; i++)
            {
                int entryLen = requiredBits(counts[0]);
                if (i == counts.Length - 1)
                {
                    //longest ngram
                    entryLen += quantProbLen;
                }
                else
                {
                    //middle ngram
                    entryLen += requiredBits(counts[i + 1]);
                    entryLen += quantProbBoLen;
                }
                // Extra entry for next pointer at the end.  
                // +7 then / 8 to round up bits and convert to bytes
                // +8 (or +sizeof(uint64))so that reading bit array doesn't exceed bounds 
                // Note that this waste is O(order), not O(number of ngrams).
                int tmpLen = ((1 + counts[i]) * entryLen + 7) / 8 + 8;
                ngramMemSize[i - 1] = tmpLen;
                memLen += tmpLen;
            }
            bitArr = new NgramTrieBitarr(memLen);
            this.quantProbLen = quantProbLen;
            this.quantProbBoLen = quantProbBoLen;
            middles = new MiddleNgramSet[counts.Length - 2];
            int[] startPtrs = new int[counts.Length - 2];
            int startPtr = 0;
            for (int i = 0; i < counts.Length - 2; i++)
            {
                startPtrs[i] = startPtr;
                startPtr += ngramMemSize[i];
            }
            // Crazy backwards thing so we initialize using pointers to ones that have already been initialized
            for (int i = counts.Length - 1; i >= 2; --i)
            {
                middles[i - 2] = new MiddleNgramSet(this, startPtrs[i - 2], quantProbBoLen, counts[i - 1], counts[0], counts[i]);
            }
            longest = new LongestNgramSet(this, startPtr, quantProbLen, counts[0]);
            ordersNum = middles.Length + 1;
        }

        public byte[] getMem()
        {
            return bitArr.getArr();
        }

        private int findNgram(NgramSet ngramSet, int wordId, TrieRange range)
        {
            int ptr;
            range.begin--;
            if ((ptr = uniformFind(ngramSet, range, wordId)) < 0)
            {
                range.setFound(false);
                return -1;
            }
            //read next order ngrams for future searches
            if (ngramSet is MiddleNgramSet)
                ((MiddleNgramSet)ngramSet).readNextRange(ptr, range);
            return ptr;
        }

        public float readNgramBackoff(int wordId, int orderMinusTwo, TrieRange range, NgramTrieQuant quant)
        {
            int ptr;
            NgramSet ngram = getNgram(orderMinusTwo);
            if ((ptr = findNgram(ngram, wordId, range)) < 0)
                return 0.0f;
            return quant.readBackoff(bitArr, ngram.memPtr, ngram.getNgramWeightsOffset(ptr), orderMinusTwo);
        }

        public float readNgramProb(int wordId, int orderMinusTwo, TrieRange range, NgramTrieQuant quant)
        {
            int ptr;
            NgramSet ngram = getNgram(orderMinusTwo);
            if ((ptr = findNgram(ngram, wordId, range)) < 0)
                return 0.0f;
            return quant.readProb(bitArr, ngram.memPtr, ngram.getNgramWeightsOffset(ptr), orderMinusTwo);
        }

        private int calculatePivot(int offset, int range, int width)
        {
            return (int)(((long)offset * width) / (range + 1));
        }

        private int uniformFind(NgramSet ngram, TrieRange range, int wordId)
        {
            var vocabRange = new TrieRange(0, ngram.maxVocab);
            while (range.getWidth() > 1)
            {
                int pivot = range.begin + 1 + calculatePivot(wordId - vocabRange.begin, vocabRange.getWidth(), range.getWidth() - 1);
                int mid = ngram.readNgramWord(pivot);
                if (mid < wordId)
                {
                    range.begin = pivot;
                    vocabRange.begin = mid;
                }
                else if (mid > wordId)
                {
                    range.end = pivot;
                    vocabRange.end = mid;
                }
                else
                {
                    return pivot;
                }
            }
            return -1;
        }

        private NgramSet getNgram(int orderMinusTwo)
        {
            if (orderMinusTwo == ordersNum - 1)
                return longest;
            return middles[orderMinusTwo];
        }

        protected internal static int requiredBits(int maxValue)
        {
            if (maxValue == 0) return 0;
            int res = 1;
            while ((maxValue >>= 1) != 0) res++;
            return res;
        }
    }
}
