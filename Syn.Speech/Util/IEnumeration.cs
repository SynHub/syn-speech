//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    public interface IEnumeration<T>  {
        bool HasMoreElements();

        T NextElement();
}
}
