using System.Runtime.Serialization;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// Implements the interface for all Data objects that passes between
    /// DataProcessors.
    /// Subclass of Data can contain the actual data, or be a signal
    /// (e.g., data start, data end, speech start, speech end).
    /// </summary>
    public interface IData: ISerializable
    {

    }
}
