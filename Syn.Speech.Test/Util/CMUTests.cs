using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.FrontEnds;
using Syn.Speech.Helper;
using Syn.Speech.Instrumentation;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Util
{
    [TestClass]
    public class CMUTests
    {
        [TestMethod]
        public void CMU_ClassTesting()
        {
            Assert.IsTrue(ConfigurationManagerUtils.IsImplementingInterface(typeof(FrontEnd), typeof(IDataProcessor)));
            Assert.IsTrue(ConfigurationManagerUtils.IsImplementingInterface(typeof(IDataProcessor), typeof(IConfigurable)));
            Assert.IsFalse(ConfigurationManagerUtils.IsImplementingInterface(typeof(IConfigurable), typeof(IConfigurable)));

            Assert.IsFalse(ConfigurationManagerUtils.IsSubClass(typeof(IConfigurable), typeof(IConfigurable)));
            Assert.IsTrue(ConfigurationManagerUtils.IsSubClass(typeof(Integer), typeof(Object)));
            Assert.IsFalse(ConfigurationManagerUtils.IsSubClass(typeof(Object), typeof(Object)));

            Assert.IsTrue(ConfigurationManagerUtils.IsSubClass(typeof(BestPathAccuracyTracker), typeof(AccuracyTracker)));

            Assert.IsTrue(ConfigurationManagerUtils.IsDerivedClass(typeof(BestPathAccuracyTracker), typeof(AccuracyTracker)));
            Assert.IsTrue(ConfigurationManagerUtils.IsDerivedClass(typeof(BestPathAccuracyTracker), typeof(BestPathAccuracyTracker)));
            Assert.IsTrue(!ConfigurationManagerUtils.IsDerivedClass(typeof(BestPathAccuracyTracker), typeof(DoubleData)));
        }

        [TestMethod]
        public void CMU_ComponentProperty()
        {
            var configFile = new URL(Helper.FilesDirectory + "/util/props/ConfigurationManagerTest.testconfig.sxl");
            var cm = new ConfigurationManager(configFile);

            const int newBeamWidth = 4711;
            ConfigurationManagerUtils.SetProperty(cm, "beamWidth", newBeamWidth.ToString(CultureInfo.InvariantCulture));

            var dummyComp = (DummyComp)cm.Lookup("duco");
            Assert.AreEqual(newBeamWidth, dummyComp.GetBeamWidth());
        }
    }
}
