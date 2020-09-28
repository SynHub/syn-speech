using System;
using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist
{
    /// <summary>
    /// Represents a single word state in a language search space
    /// </summary>
    public interface IWordSearchState
    {
        /// <summary>
        /// Gets the word (as a pronunciation)
        /// </summary>
        /// <value></value>
        Pronunciation Pronunciation { get; }

        /// <summary>
        /// Returns true if this WordSearchState indicates the start of a word. Returns false if this WordSearchState
        /// indicates the end of a word.
        /// </summary>
        /// <returns>true if this WordSearchState indicates the start of a word, false if this WordSearchState indicates the
        ///         end of a word</returns>
        Boolean IsWordStart();
    }
}
