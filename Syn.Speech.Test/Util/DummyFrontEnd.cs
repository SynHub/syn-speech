using System;
using System.Collections.Generic;
using Syn.Speech.Util.Props;
namespace Syn.Speech.Test.Util
{
    public class S4ComponentListCustom : S4ComponentList
    {
        public S4ComponentListCustom()
        {
            BeTolerant = true;
            DefaultList = new Type[]{typeof(DummyProcessor), typeof(AnotherDummyProcessor), typeof(DummyProcessor)};
        }
    }

    /**
     * DOCUMENT ME!
     *
     * @author Holger Brandl
     */
    public class DummyFrontEnd : IConfigurable
    {

        [S4Boolean(DefaultValue = true)]
        public const String PropUseMffcs = "useMfccs";
        bool _useMfccs;

        [S4ComponentListCustom(Type = typeof(IConfigurable), BeTolerant = true )]
        public const String DataProcs = "dataProcs";
        List<IConfigurable> _dataProcs;


        public void NewProperties(PropertySheet ps)
        {
            _useMfccs = ps.GetBoolean(PropUseMffcs);
            _dataProcs = ps.GetComponentList<IConfigurable>(DataProcs);
        }


        public bool IsUseMfccs()
        {
            return _useMfccs;
        }


        public List<IConfigurable> GetDataProcs()
        {
            return _dataProcs;
        }


        public String GetName()
        {
            return GetType().Name;
        }
    }
}
