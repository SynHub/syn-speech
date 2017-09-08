using Syn.Speech.Common;
using Syn.Speech.Decoder.Search;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Pruner
{
    /// <summary>
    /// Provides a mechanism for pruning a set of StateTokens
    /// </summary>
    public interface IPruner: IConfigurable
    {
        /// <summary>
        /// Starts the pruner 
        /// </summary>
        void startRecognition();

        /// <summary>
        /// prunes the given set of states 
        /// </summary>
        /// <param name="stateTokenList">a list containing StateToken objects to be scored</param>
        /// <returns>the pruned list, (may be the sample list as stateTokenList)</returns>
        ActiveList prune(ActiveList stateTokenList);


        /// <summary>
        /// Performs post-recognition cleanup. 
        /// </summary>
        void stopRecognition();

        /// <summary>
        /// Allocates resources necessary for this pruner
        /// </summary>
        void allocate();

        /// <summary>
        /// Deallocates resources necessary for this pruner 
        /// </summary>
        void deallocate();

    }
}
