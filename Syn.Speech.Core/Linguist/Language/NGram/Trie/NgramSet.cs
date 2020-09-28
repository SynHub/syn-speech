using Syn.Speech.Helper;
//PATROLLED
namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    abstract class NgramSet
    {
        private readonly NgramTrie _parent;//Java Nested Class
        internal int memPtr;
        protected int wordBits;
        int wordMask;
        protected int totalBits;
        int insertIdx;
        internal int maxVocab;

        protected NgramSet(NgramTrie parent, int memPtr, int maxVocab, int remainingBits)
        {
            _parent = parent;
            this.maxVocab = maxVocab;
            this.memPtr = memPtr;
            wordBits = NgramTrie.requiredBits(maxVocab);
            if (wordBits > 25)
                throw new Error("Sorry, word indices more than" + (1 << 25) + " are not implemented");
            totalBits = wordBits + remainingBits;
            wordMask = (1 << wordBits) - 1;
            insertIdx = 0;
        }

        internal int readNgramWord(int ngramIdx)
        {
            int offset = ngramIdx * totalBits;
            return _parent.bitArr.readInt(memPtr, offset, wordMask);
        }

        internal int getNgramWeightsOffset(int ngramIdx)
        {
            return ngramIdx * totalBits + wordBits;
        }

        abstract public int getQuantBits();
    }
}
