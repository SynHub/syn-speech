using System.Collections.Generic;
using Syn.Speech.FrontEnds;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Scorer
{
    /// <summary>
    /// Provides a mechanism for scoring a set of HMM states
    /// </summary>
    public interface IAcousticScorer: IConfigurable
    {
        /// <summary>
        /// Allocates resources for this scorer 
        /// </summary>
        void Allocate();

        /// <summary>
        /// Deallocates resources for this scorer 
        /// </summary>
        void Deallocate();

        /// <summary>
        /// starts the scorer 
        /// </summary>
        void StartRecognition();

        /// <summary>
        /// stops the scorer 
        /// </summary>
        void StopRecognition();

        /// <summary>
        /// Scores the given set of states
        /// </summary>
        /// <param name="scorableList">a list containing Scoreable objects to be scored</param>
        /// <returns>the best scoring scoreable, or null if there are no more frames to score</returns>
        IData CalculateScores<T>(List<T> scorableList) where T : IScoreable;

        IData CalculateScoresAndStoreData<T>(List<T> scorableList) where T : IScoreable;


    }
}
