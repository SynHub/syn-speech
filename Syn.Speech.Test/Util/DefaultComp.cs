using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Util
{
    [TestClass]
    public class DummyComp : IConfigurable
    {

        /** doc of beamWidth. */
        [S4Integer(DefaultValue = 4)]
        public const String PropBeamWidth = "beamWidth";

        [S4String(DefaultValue = "salami&cheese")]
        public const String PropBestPizza = "bestPizza";

        [S4Boolean(DefaultValue = true)]
        public const String PropUseFoobar = "useFooBar";

        [S4Boolean(DefaultValue = true)]
        public const String PropUseFoobaz = "useFooBaz";

        /** doc of frontend. */
        [S4Component(Type = typeof(DummyFrontEnd), DefaultClass = typeof(AnotherDummyFrontEnd))]
        public const String PropFrontend = "frontend";

        [S4Double(DefaultValue = 1.3, Range = new double[] { -1, 15 })]
        public const String PropAlpha = "alpha";

        /** doc of the string. */
        [S4String(DefaultValue = "sphinx4", Range = new[] { "sphinx4", "htk" })]
        public const String PropBestAsr = "bestAsrSystem";


        private int _beamWidth;
        private DummyFrontEnd _frontEnd;
        private String _bestAsr;
        private double _alpha;
        private bool _useFooBaz;



        public int GetBeamWidth()
        {
            return _beamWidth;
        }


        public DummyFrontEnd GetFrontEnd()
        {
            return _frontEnd;
        }


        public String GetBestAsr()
        {
            return _bestAsr;
        }


        public double GetAlpha()
        {
            return _alpha;
        }



        public void NewProperties(PropertySheet ps)
        {
            _frontEnd = (DummyFrontEnd)ps.GetComponent(PropFrontend);
            _beamWidth = ps.GetInt(PropBeamWidth);
            _bestAsr = ps.GetString(PropBestAsr);
            _alpha = ps.GetDouble(PropAlpha);
            _useFooBaz = ps.GetBoolean(PropUseFoobaz);
        }


        public String GetName()
        {
            return "lalala";
        }


        [TestMethod]
        public void DefaultComp_GetDefaultInstance()
        {
            var dc = ConfigurationManager.GetInstance<DummyComp>();

            Assert.AreEqual(4, dc.GetBeamWidth());
            Assert.AreEqual(1.3, dc.GetAlpha(), 1E-10);
            Assert.AreEqual(false, _useFooBaz);

            var fe = dc.GetFrontEnd();
            Assert.IsTrue(fe != null);
            Assert.IsTrue(fe is AnotherDummyFrontEnd);
            Assert.IsTrue(fe.GetDataProcs().Count == 3);
            Assert.IsTrue(fe.GetDataProcs()[0] is DummyProcessor);
            Assert.IsTrue(fe.GetDataProcs()[1] is AnotherDummyProcessor);
            Assert.IsTrue(fe.GetDataProcs()[2] is DummyProcessor);

            Assert.IsTrue(dc.GetBestAsr().Equals("sphinx4"));
        }


        /** Use the all defaults defined by the annotations to instantiate a Configurable. */
        [TestMethod]
        public void DefaultComp_CustomizedDefaultInstance()
        {
            var defaultProps = new HashMap<String, Object>();
            defaultProps.Put(PropFrontend, new DummyFrontEnd());

            var dc = ConfigurationManager.GetInstance<DummyComp>(typeof(DummyComp), defaultProps);

            Assert.AreEqual(4, dc.GetBeamWidth());
            Assert.AreEqual(1.3, dc.GetAlpha(), 1E-10);
            Assert.IsTrue(dc.GetFrontEnd() != null);
            Assert.IsTrue(dc.GetBestAsr().Equals("sphinx4"));
        }


        [TestMethod]
        public void DefaultComp_UseXmlConfig()
        {
            // probably you need to adpat this path. testconfig is located in the same folder as test
            var configFile = new URL(Helper.FilesDirectory + "/util/props/ConfigurationManagerTest.testconfig.sxl");

            var cm = new ConfigurationManager(configFile);

            var dc = (DummyComp)cm.Lookup("duco");

            Assert.AreEqual(dc.GetBeamWidth(), 123);
            Assert.AreEqual(9.99, dc.GetAlpha(), 1E-10);

            Assert.IsTrue(dc.GetFrontEnd() != null);
            Assert.IsTrue(dc.GetFrontEnd().IsUseMfccs());
            Assert.IsTrue(dc.GetFrontEnd().GetDataProcs().Count == 2);

            Assert.IsTrue(dc.GetBestAsr().Equals("sphinx4"));

        }
    }

}
