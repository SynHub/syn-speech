//PATROLLED

using System.IO;

namespace Syn.Speech.Alignment
{
    /// <summary>
    /// Implements a finite state machine that checks if a given string is a suffix.
    /// </summary>
    public class SuffixFSM : PronounceableFSM
    {
        public SuffixFSM(FileInfo path)
            : base(path, false)
        {
        }

        public SuffixFSM(string path)
            : base(path, false)
        {
        }
    }
}
