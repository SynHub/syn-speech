//PATROLLED
namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    class LongestNgramSet : NgramSet
    {
        private readonly NgramTrie _parent; //Java Nested Class

        public LongestNgramSet(NgramTrie parent, int memPtr, int quantBits, int maxVocab): base(parent, memPtr, maxVocab, quantBits)
        {
            _parent = parent;
        }

        public override int getQuantBits()
        {
            return _parent.quantProbLen;
        }
    }
}
