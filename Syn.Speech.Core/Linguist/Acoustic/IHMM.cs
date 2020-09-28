//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents a hidden-markov-model. An HMM consists of a unit (context dependent or independent), a transition matrix
    /// from state to state, and a sequence of senones associated with each state. This representation of an HMM is a
    /// specialized left-to-right markov model. No backward transitions are allowed.
    /// </summary>
    public interface IHMM
    {
        /// <summary>
        /// Gets the  unit associated with this HMM
        /// </summary>
        /// <value>the unit associated with this HMM</value>
        Unit Unit { get; }

        /// <summary>
        /// Gets the  base unit associated with this HMM
        /// </summary>
        /// <value>the unit associated with this HMM</value>
        Unit BaseUnit { get; }

        /// <summary>
        /// Retrieves the hmm state
        /// </summary>
        /// <param name="which"></param>
        /// <returns>which the state of interest</returns>
        IHMMState GetState(int which);

        /// <summary>
        /// Returns the order of the HMM
        /// </summary>
        /// <value>the order of the HMM</value>
        int Order { get; }

        /// <summary>
        /// Retrieves the position of this HMM.
        /// </summary>
        /// <value>the position for this HMM</value>
        HMMPosition Position { get; }

        /// <summary>
        /// Gets the initial states (with probabilities) for this HMM
        /// </summary>
        /// <returns>the set of arcs that transition to the initial states for this HMM</returns>
        IHMMState GetInitialState();

    }
}
