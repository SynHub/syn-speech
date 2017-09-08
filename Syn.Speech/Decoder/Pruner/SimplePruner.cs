using System;
using Syn.Speech.Common;
using Syn.Speech.Decoder.Search;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Pruner
{
    /// <summary>
    /// Performs the default pruning behavior which is to invoke the purge on the active list
    /// </summary>
    public class SimplePruner: IPruner
    {
        private String name;


        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
         */
        public void newProperties(PropertySheet ps)
        {
        }


        public SimplePruner() {
        }

        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.util.props.Configurable#getName()
         */
        public string getName() {
            return name;
        }


        /** Starts the pruner */
        public void startRecognition() {
        }


        /**
        /// prunes the given set of states
         *
        /// @param activeList a activeList of tokens
         */
        public ActiveList prune(ActiveList activeList) 
        {
            return activeList.purge();
        }


        /** Performs post-recognition cleanup. */
        public void stopRecognition() {
        }


        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.decoder.pruner.Pruner#allocate()
         */
        public void allocate() 
        {
        }


        /* (non-Javadoc)
        /// @see edu.cmu.sphinx.decoder.pruner.Pruner#deallocate()
         */
        public void deallocate() 
        {

        }


    }
}
