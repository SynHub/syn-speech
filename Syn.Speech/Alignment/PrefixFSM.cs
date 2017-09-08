//PATROLLED

using System.IO;

namespace Syn.Speech.Alignment
{
    public class PrefixFSM : PronounceableFSM
    {
        public PrefixFSM(FileInfo path) : base(path, true)
        {
        }

        public PrefixFSM(string path)
            : base(path, true)
        {
        }
    }
}
