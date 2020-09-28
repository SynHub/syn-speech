using Syn.Speech.Decoders.Search;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Pruner
{
    /// <summary>
    /// Provides a mechanism for pruning a set of StateTokens
    /// </summary>
    public interface IPruner: IConfigurable
    {
        /// <summary>
        /// Starts the pruner 
        /// </summary>
        void StartRecognition();

        /// <summary>
        /// prunes the given set of states 
        /// </summary>
        /// <param name="stateTokenList">a list containing StateToken objects to be scored</param>
        /// <returns>the pruned list, (may be the sample list as stateTokenList)</returns>
        ActiveList Prune(ActiveList stateTokenList);


        /// <summary>
        /// Performs post-recognition cleanup. 
        /// </summary>
        void StopRecognition();

        /// <summary>
        /// Allocates resources necessary for this pruner
        /// </summary>
        void Allocate();

        /// <summary>
        /// Deallocates resources necessary for this pruner 
        /// </summary>
        void Deallocate();

    }
}
