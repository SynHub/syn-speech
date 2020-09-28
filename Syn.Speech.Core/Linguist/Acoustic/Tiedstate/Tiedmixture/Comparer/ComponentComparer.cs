using System.Collections.Generic;
//PATROLLED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate.Tiedmixture.Comparer
{
    public class ComponentComparer : IComparer<PrunableMixtureComponent>
    {
        public int Compare(PrunableMixtureComponent a, PrunableMixtureComponent b)
        {
            return (int)(a.StoredScore - b.StoredScore);
        }
    }
}
