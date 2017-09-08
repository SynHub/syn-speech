using System.IO;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    /// <summary>
    /// Implements a finite state machine that checks if a given string is a suffix.
    /// </summary>
    public class SuffixFsm : PronounceableFsm
    {
        public SuffixFsm(FileInfo path): base(path, false)
        {
        }

        public SuffixFsm(string path) : base(path, false)
        {
        }
    }
}
