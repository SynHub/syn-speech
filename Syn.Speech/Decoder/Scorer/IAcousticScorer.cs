using System.Collections.Generic;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Scorer
{
    /// <summary>
    /// Provides a mechanism for scoring a set of HMM states
    /// </summary>
    public interface IAcousticScorer: IConfigurable
    {
        /// <summary>
        /// Allocates resources for this scorer 
        /// </summary>
        void allocate();

        /// <summary>
        /// Deallocates resources for this scorer 
        /// </summary>
        void deallocate();

        /// <summary>
        /// starts the scorer 
        /// </summary>
        void startRecognition();

        /// <summary>
        /// stops the scorer 
        /// </summary>
        void stopRecognition();

        /// <summary>
        /// Scores the given set of states
        /// </summary>
        /// <param name="scorableList">a list containing Scoreable objects to be scored</param>
        /// <returns>the best scoring scoreable, or null if there are no more frames to score</returns>
        IData calculateScores(List<IScoreable> scorableList);
      

    }
}
