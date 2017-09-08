using Syn.Speech.Common;
using Syn.Speech.Decoder.Search;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Pruner
{

    /// <summary>
    /// A Null pruner. Does no actual pruning
    /// </summary>
    public class NullPruner : IPruner
    {


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public void newProperties(PropertySheet ps)
        {
        }


        /** Creates a simple pruner */
        public NullPruner()
        {
        }


        /** starts the pruner */
        public void startRecognition()
        {
        }

        public ActiveList prune(ActiveList activeList)
        {
            return activeList;
        }


        /** Performs post-recognition cleanup. */
        public void stopRecognition()
        {
        }


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.decoder.pruner.Pruner#allocate()
        */
        public void allocate()
        {

        }


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.decoder.pruner.Pruner#deallocate()
        */
        public void deallocate()
        {

        }

    }
}
