using System.IO;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    public class PrefixFsm : PronounceableFsm
    {
        public PrefixFsm(FileInfo path) : base(path, true)
        {
        }

        public PrefixFsm(string path)
            : base(path, true)
        {
        }
    }
}
