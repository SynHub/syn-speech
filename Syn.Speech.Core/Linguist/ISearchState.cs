//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist
{
    /// <summary>
    /// Represents a single state in a language search space
    /// </summary>
    public interface ISearchState
    {
        /// <summary>
        /// Gets a successor to this search state
        /// </summary>
        /// <returns></returns>
        ISearchStateArc[] GetSuccessors();

        /// <summary>
        /// Gets a value indicating whether this is an emitting state
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is emitting; otherwise, <c>false</c>.
        /// </value>
        bool IsEmitting { get; }

        /// <summary>
        /// Determines if this is a final state
        /// </summary>
        /// <value>
        /// <c>true</c> if the state is a final state; otherwise, <c>false</c>.
        /// </value>
        bool IsFinal { get; }

        /// <summary>
        /// Returns a pretty version of the string representation for this object.
        /// </summary>
        /// <returns>A pretty string</returns>
        string ToPrettyString();

        /// <summary>
        /// Returns a unique signature for this state
        /// </summary>
        /// <value>
        /// The signature for the state.
        /// </value>
        string Signature { get; }

        /// <summary>
        /// Gets the word history for this state.
        /// </summary>
        /// <value>
        /// The word history.
        /// </value>
        WordSequence WordHistory { get; }

        /// <summary>
        /// Gets the lex tree state.
        /// </summary>
        /// <value>
        /// The lex tree state.
        /// </value>
        object LexState { get; }

        /// <summary>
        ///  Returns the order of this particular state.
        /// </summary>
        /// <value>
        /// The state order for this state.
        /// </value>
        int Order { get; }
    }
}
