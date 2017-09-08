using System;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Util
{

public class DummyProcessor : DummyFrontEndProcessor {


    public void NewProperties(PropertySheet ps) {
    }


    public String GetName()
    {
        return GetType().Name;
    }
}
}
