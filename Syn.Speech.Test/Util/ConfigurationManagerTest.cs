using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Util
{
    [TestClass]
    public class ConfigurationManagerTest
    {
        [TestMethod]
        public void ConfigurationManager_DynamicConfCreation()
        {
            var cm = new ConfigurationManager();

            const string instanceName = "docu";
            var props = new HashMap<string, object>();
            props.Put(DummyComp.PropFrontend, new DummyFrontEnd());
            cm.AddConfigurable(typeof(DummyComp), instanceName, props);

            Assert.IsNotNull(cm.GetPropertySheet(instanceName));
            Assert.IsNotNull(cm.Lookup(instanceName));
            Assert.IsTrue(cm.Lookup(instanceName) is DummyComp);
        }

        [TestMethod]
        public void ConfigurationManager_DynamicConfiguruationChange()
        {

            var url = new URL(Helper.FilesDirectory + "/util/props/ConfigurationManagerTest.testconfig.sxl");
            var cm = new ConfigurationManager(url);


            Assert.IsTrue(cm.GetInstanceNames(typeof(DummyFrontEndProcessor)).Count == 0);

            var propSheet = cm.GetPropertySheet("duco");
            propSheet.SetDouble("alpha", 11);
            var duco = (DummyComp)cm.Lookup("duco");

            Assert.IsTrue(cm.GetInstanceNames(typeof(DummyFrontEndProcessor)).Count == 1);

            // IMPORTANT because we assume the configurable to be instantiated
            // first at lookup there is no need to call newProperties here
            // duco.newProperties(propSheet);
            Assert.IsTrue(Helper.CloseTo(duco.GetAlpha(), 11, JDouble.MIN_VALUE));
        }

        [TestMethod]
        public void ConfigurationManager_Load()
        {
            var configManager = new ConfigurationManager(new URL("TestConfig.xml"));

            var globalProps = configManager.GetGlobalProperties();

            Assert.AreEqual("INFO", globalProps["logLevel"]);
            Assert.AreEqual("300", globalProps["absoluteWordBeamWidth"]);
            Assert.AreEqual("TestConfig.xml", configManager.ConfigUrl.Path);

            Assert.IsTrue(configManager.GetComponentNames().Contains("recognizer"));
            Assert.IsFalse(configManager.GetComponentNames().Contains("something_recognizer"));
        }

        [TestMethod]
        public void ConfigurationManager_GetComponentClass()
        {

            var url = new URL(Helper.FilesDirectory + "/util/props/ConfigurationManagerTest.sxl");
            var cm = new ConfigurationManager(url);

            const string instanceName = "duco";
            var ps = cm.GetPropertySheet(instanceName);
            Assert.AreEqual(ps.GetComponentClass("frontend"), typeof(DummyFrontEnd));
            Assert.AreEqual(ps.GetComponentClass("anotherFrontend"), typeof(DummyFrontEnd));
        }
    }
}
