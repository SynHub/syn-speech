//PATROLLED
namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    public class TrieRange
    {
        internal int begin;
        internal int end;
        bool found;

        public TrieRange(int begin, int end)
        {
            this.begin = begin;
            this.end = end;
            found = true;
        }

        internal int getWidth()
        {
            return end - begin;
        }

        internal void setFound(bool found)
        {
            this.found = found;
        }

        internal bool getFound()
        {
            return found;
        }

        internal bool isSearchable()
        {
            return getWidth() > 0;
        }
    }
}