using Syn.Speech.Decoders.Search;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Pruner
{
    /// <summary>
    /// Performs the default pruning behavior which is to invoke the purge on the active list
    /// </summary>
    public class SimplePruner: IPruner
    {
        private string _name;

        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
         */
        public void NewProperties(PropertySheet ps)
        {

        }


        public SimplePruner()
        {
        }

        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.util.props.Configurable#getName()
         */

        public string Name { get { return _name; } }


        /** Starts the pruner */
        public void StartRecognition() {
        }


        /**
        /// prunes the given set of states
         *
        /// @param activeList a activeList of tokens
         */
        public ActiveList Prune(ActiveList activeList) 
        {
            return activeList.Purge();
        }


        /** Performs post-recognition cleanup. */
        public void StopRecognition() {
        }


        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.decoder.pruner.Pruner#allocate()
         */
        public void Allocate() 
        {
        }


        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.decoder.pruner.Pruner#deallocate()
         */
        public void Deallocate() 
        {

        }


    }
}
