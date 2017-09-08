using Syn.Speech.Decoders.Search;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Pruner
{

    /// <summary>
    /// A Null pruner. Does no actual pruning
    /// </summary>
    public class NullPruner : IPruner
    {


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public void NewProperties(PropertySheet ps)
        {
        }


        /** Creates a simple pruner */
        public NullPruner()
        {
        }


        /** starts the pruner */
        public void StartRecognition()
        {
        }

        public ActiveList Prune(ActiveList activeList)
        {
            return activeList;
        }


        /** Performs post-recognition cleanup. */
        public void StopRecognition()
        {
        }


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.decoder.pruner.Pruner#allocate()
        */
        public void Allocate()
        {

        }


        /* (non-Javadoc)
        * @see edu.cmu.sphinx.decoder.pruner.Pruner#deallocate()
        */
        public void Deallocate()
        {

        }

    }
}
