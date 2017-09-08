using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// A factory for PartitionActiveLists
    /// </summary>
    public class PartitionActiveListFactory : ActiveListFactory
    {
        public PartitionActiveListFactory(int absoluteBeamWidth, double relativeBeamWidth) :base(absoluteBeamWidth, relativeBeamWidth)
        {
            
        }

        public PartitionActiveListFactory() 
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
        }


        public override ActiveList NewInstance() 
        {
            return new PartitionActiveList(AbsoluteBeamWidth, LogRelativeBeamWidth,this);
        }
    }
}    

