using System.Collections.Generic;
//REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    ///  A source of reference texts.
    /// </summary>
    public interface IReferenceSource
    {

        /// <summary>
        /// Returns a list of reference text.
        /// </summary>
        IList<string> References { get; }
    }
}
