using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Allphone
{
    
public class AllphoneSearchGraph : ISearchGraph {

    private readonly AllphoneLinguist _linguist;
    
    public AllphoneSearchGraph(AllphoneLinguist linguist) {
        _linguist = linguist;
    }

    public ISearchState InitialState
    {
        get
        {
            var silHmmState =
                _linguist.AcousticModel.LookupNearestHMM(UnitManager.Silence, HMMPosition.Undefined, true).GetInitialState();
            return new PhoneHmmSearchState(silHmmState, _linguist, LogMath.LogOne, LogMath.LogOne);
        }
    }

    public int NumStateOrder
    {
        get { return 2; }
    }

    public bool WordTokenFirst
    {
        get { return false; }
    }
}
}
