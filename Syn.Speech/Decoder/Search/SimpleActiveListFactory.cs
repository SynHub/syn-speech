using System.Collections.Generic;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{

    /** A factory for simple active lists */
    public class SimpleActiveListFactory : ActiveListFactory
    {

        /**
         * 
         * @param absoluteBeamWidth
         * @param relativeBeamWidth
         * @param logMath
         */
        public SimpleActiveListFactory(int absoluteBeamWidth,
                double relativeBeamWidth)
            : base(absoluteBeamWidth, relativeBeamWidth)
        {
            ;
        }

        public SimpleActiveListFactory()
        {

        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */

        public new void newProperties(PropertySheet ps)
        {
            base.newProperties(ps);
        }


        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.decoder.search.ActiveListFactory#newInstance()
        */
        public override ActiveList newInstance()
        {
            return new SimpleActiveList(absoluteBeamWidth, logRelativeBeamWidth);
        }


        /**
         * An active list that tries to be simple and correct. This type of active list will be slow, but should exhibit
         * correct behavior. Faster versions of the ActiveList exist (HeapActiveList, TreeActiveList).
         * <p/>
         * This class is not thread safe and should only be used by a single thread.
         * <p/>
         * Note that all scores are maintained in the LogMath log domain
         */

    }
}
