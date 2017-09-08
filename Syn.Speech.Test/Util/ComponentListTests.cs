using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Util
{
    [TestClass]
    public class ComponentListTests
    {
        [TestMethod]
        public void ComponentList_InvalidList()
        {
            var cm = new ConfigurationManager();

            var props = new HashMap<String, Object>();
            cm.AddConfigurable(typeof(DummyProcessor), "dummyA");
            props.Put(DummyFrontEnd.DataProcs, Arrays.AsList("dummyA, dummyB"));
            cm.AddConfigurable(typeof(DummyFrontEnd), "dfe", props);

            cm.Lookup("dfe");
        }
    }
}
