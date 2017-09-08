using Syn.Speech.Helper;
//PATROLLED
namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    class MiddleNgramSet : NgramSet
    {
        private readonly NgramTrie _parent;//Java Nested Class
        int nextMask;
        int nextOrderMemPtr;

        public MiddleNgramSet(NgramTrie parent, int memPtr, int quantBits, int entries, int maxVocab, int maxNext)
            : base(parent, memPtr, maxVocab, quantBits + NgramTrie.requiredBits(maxNext))
        {
            _parent = parent;
            nextMask = (1 << NgramTrie.requiredBits(maxNext)) - 1;
            if (entries + 1 >= (1 << 25) || (maxNext >= (1 << 25)))
                throw new Error("Sorry, current implementation doesn't support more than " + (1 << 25) + " n-grams of particular order");
        }

        internal void readNextRange(int ngramIdx, TrieRange range)
        {
            int offset = ngramIdx * totalBits;
            offset += wordBits;
            offset += getQuantBits();
            range.begin = _parent.bitArr.readInt(memPtr, offset, nextMask);
            offset += totalBits;
            range.end = _parent.bitArr.readInt(memPtr, offset, nextMask);
        }

        public override int getQuantBits()
        {
            return _parent.quantProbBoLen;
        }
    }
}
