namespace Syn.Speech.Linguist.Language.NGram
{
    /// <summary>
    /// 
    /// Represents the generic interface to an N-Gram language model
    /// that uses backoff to estimate unseen probabilities. Backoff
    /// depth is important in search space optimization, for example
    /// it's used in LexTreeLinguist to collapse states which has
    /// only unigram backoff. This ways unlikely sequences are penalized.
    /// </summary>
    public abstract class IBackoffLanguageModel : ILanguageModel
    {
        public abstract ProbDepth getProbDepth(WordSequence wordSequence);
    }
}
