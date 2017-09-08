namespace Syn.Speech.Result
{
    public interface IResultListener: Util.Props.IConfigurable
    {
        void newResult(Result result);
    }
}
