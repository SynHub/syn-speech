using System;
using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents a single state in an HMM
    /// </summary>
    public interface IHMMState
    {
        /// <summary>
        /// Gets the HMM associated with this state
        /// </summary>
        /// <value>the HMM</value>
        IHMM HMM { get; }

        /// <summary>
        /// Returns the mixture components associated with this Gaussian
        /// </summary>
        /// <returns>the array of mixture components</returns>
        MixtureComponent[] GetMixtureComponents();
        /// <summary>
        /// Gets the id of the mixture
        /// </summary>
        /// <returns>the id</returns>
        long GetMixtureId();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>the mixture weights vector</returns>
        float[] GetLogMixtureWeights();

        /// <summary>
        /// Gets the state
        /// </summary>
        /// <value>the state</value>
        int State { get; }

        /// <summary>
        /// Gets the score for this HMM state
        /// </summary>
        /// <param name="data">the data to be scored</param>
        /// <returns>the acoustic score for this state.</returns>
        float GetScore(IData data);

        float[] CalculateComponentScore(IData data);

        /// <summary>
        /// Determines if this HMMState is an emitting state
        /// </summary>
        /// <value>true if the state is an emitting state</value>
        bool IsEmitting { get; }

        /// <summary>
        /// Retrieves the state of successor states for this state
        /// </summary>
        /// <returns>the set of successor state arcs</returns>
        HmmStateArc[] GetSuccessors();

        /// <summary>
        /// Determines if this state is an exit state of the HMM
        /// </summary>
        /// <returns>true if the state is an exit state</returns>
        Boolean IsExitState();

    }
}
