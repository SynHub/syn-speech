using System.Collections.Generic;

namespace Syn.Speech.Helper
{
    //public interface Iterator
    //{
    //    bool MoveNext();
    //    object Current();
    //    void Remove();
    //}
    public interface  Iterator<out T> : IEnumerator<T>//, Iterator
    {
        void Remove();
        //bool HasNext();
        //T Next();
    }

}
