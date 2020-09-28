using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist
{
    /// <summary>
    /// Represents a single HMM state in a language search space
    /// </summary>
    public interface IHMMSearchState : ISearchState
    {
        /// <summary>
        /// Gets the hmm state
        /// </summary>
        /// <value></value>
        IHMMState HmmState { get; }
    }
}
