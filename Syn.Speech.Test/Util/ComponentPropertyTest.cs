using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Util
{
    [TestClass]
    public class ComponentPropertyTest : IConfigurable
    {

        [S4Component(Type = typeof(DummyProcessor), DefaultClass = typeof(AnotherDummyProcessor))]
        public const String PropDataProc = "dataProc";
        private DummyProcessor _dataProc;


        public void NewProperties(PropertySheet ps)
        {
            _dataProc = (DummyProcessor)ps.GetComponent(PropDataProc);
        }


        public string Name
        {
            get { return GetType().Name; }
        }


        [TestMethod]
        public void ComponentProperty_DefInstance()
        {
            var cpt = ConfigurationManager.GetInstance<ComponentPropertyTest>();

            Assert.IsNotNull(cpt);
            Assert.IsTrue(cpt._dataProc is AnotherDummyProcessor);
        }
    }
}
