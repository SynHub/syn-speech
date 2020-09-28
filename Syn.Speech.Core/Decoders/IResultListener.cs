//PATROLLED + REFACTORED
using Syn.Speech.Results;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Decoders
{
    public interface IResultListener: IConfigurable
    {
        /// <summary>
        ///  Method called when a new result is generated
        /// </summary>
        /// <param name="result">The new result.</param>
        void NewResult(Result result);
    }
}
