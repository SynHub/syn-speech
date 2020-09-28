using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{

    /// <summary>
    /// A factory for simple active lists
    /// </summary>
    public class SimpleActiveListFactory : ActiveListFactory
    {

        public SimpleActiveListFactory(int absoluteBeamWidth,
                double relativeBeamWidth)
            : base(absoluteBeamWidth, relativeBeamWidth)
        {
        }

        public SimpleActiveListFactory()
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
        }


        public override ActiveList NewInstance()
        {
            return new SimpleActiveList(AbsoluteBeamWidth, LogRelativeBeamWidth);

        }
    }
}
