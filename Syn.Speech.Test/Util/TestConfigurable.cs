using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Util
{
    [TestClass]
    public class TestConfigurable : IConfigurable
    {
        // note: no default component here
    [S4Component(Type = typeof(AnotherDummyProcessor))]
    public const String PROP_DATA_PROC = "dataProc";
    private DummyProcessor dataProc;

    [S4String(Mandatory = false)]
    public const String PROP_ASTRING = "mystring";
    private String myString;

    [S4Double(DefaultValue = 1.3)]
    public const String PROP_GAMMA = "gamma";
    private double gamma;


    public void NewProperties(PropertySheet ps)  {
        dataProc = (DummyProcessor) ps.GetComponent(PROP_DATA_PROC);
        myString = ps.GetString(PROP_ASTRING);
        gamma = ps.GetDouble(PROP_GAMMA);
    }


    public String Name {
        get { return GetType().Name; }
    }


        public double Gamma
        {
            get { return gamma; }
        }


        public DummyProcessor DataProc
        {
            get { return dataProc; }
        }


    [TestMethod]
    // note: it is not a bug but a feature of this test to print a stacktrace
    public void Configurable_DynamicConfCreationWithoutDefaultProperty() {
        try {
            var cm = new ConfigurationManager();

            const string instanceName = "testconf";
            cm.AddConfigurable(typeof(TestConfigurable), instanceName);

            cm.Lookup(instanceName);
            Assert.Fail("add didn't fail without given default frontend");
        } catch (NullReferenceException e) {
        } catch (PropertyException e) {
        }
    }


    [TestMethod]
    public void Configurable_NullStringProperty()  {
        var props = new HashMap<String, Object>();
        props.Put("dataProc", new AnotherDummyProcessor());

        var teco = ConfigurationManager.GetInstance<TestConfigurable>(typeof(TestConfigurable),props);
        Assert.IsNull(teco.myString);
    }


    [TestMethod]
    public void Configurable_PropSheetFromConfigurableInstance() {
        const string testString = "test";

        var props = new HashMap<String, Object>();
        props.Put(PROP_ASTRING, testString);
        props.Put(PROP_DATA_PROC, new DummyProcessor());
        var tc = ConfigurationManager.GetInstance <TestConfigurable>(typeof(TestConfigurable), props);

        // now create a property sheet in order to modify the configurable
        var propSheet = new PropertySheet(tc, null, new RawPropertyData("tt", tc.GetType().Name), new ConfigurationManager());
        propSheet.SetComponent(PROP_DATA_PROC, "tt", new AnotherDummyProcessor());
        tc.NewProperties(propSheet);

        // test whether old props were preserved and new ones were applied

        // FIXME: Its by design not possible to preserve the old properties without have a CM
        // probably we should remove the possibility to let the user create PropertySheet instances.
        // Assert.assertTrue(tc.myString.equals(testString));
        // Assert.assertTrue(tc.gamma == testDouble);
        Assert.IsTrue(tc.dataProc != null && tc.dataProc is AnotherDummyProcessor);
    }
    }
}
