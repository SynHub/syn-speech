using Syn.Speech.Common;
//PATROLLED
namespace Syn.Speech.Decoder
{
    public interface IResultListener: Util.Props.IConfigurable
    {
        /// <summary>
        ///  Method called when a new result is generated
        /// </summary>
        /// <param name="result">The new result.</param>
        void newResult(Results.Result result);
    }
}
