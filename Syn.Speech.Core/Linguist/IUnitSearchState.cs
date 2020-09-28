using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist
{
    /// <summary>
    /// Represents a unit state in a search space
    /// </summary>
    public interface IUnitSearchState: ISearchState
    {
        /// <summary>
        /// Gets the unit
        /// </summary>
        /// <value>the unit</value>
        Unit Unit { get; }
    }
}
