using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Results
{
    public interface IResultListener: IConfigurable
    {
        void NewResult(Result result);
    }
}
