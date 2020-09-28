//REFACTORED
namespace Syn.Speech.Linguist
{
    /// <summary>
    /// Represents a search graph
    /// </summary>
    public interface ISearchGraph
    {
        /// <summary>
        /// Retrieves initial search state
        /// </summary>
        /// <value></value>
        ISearchState InitialState { get; }

        /// <summary>
        /// Returns the number of different state types maintained in the search graph
        /// </summary>
        /// <value></value>
        int NumStateOrder { get; }

        /// <summary>
        /// Returns order of words and data tokens
        /// </summary>
        /// <value></value>
        bool WordTokenFirst { get; }
    }
}
