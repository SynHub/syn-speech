//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents a transition to single state in an HMM. 
    /// All probabilities are maintained in linear base.
    /// </summary>
    public class HmmStateArc 
    {
        /**
        /// Constructs an HMMStateArc
         *
        /// @param hmmState    destination state for this arc
        /// @param probability the probability for this transition
         */

        public HmmStateArc(IHMMState hmmState, float probability) {
            HmmState = hmmState;
            LogProbability = probability;
        }


        /**
        /// Gets the HMM associated with this state
         *
        /// @return the HMM
         */

        public IHMMState HmmState { get; private set; }


        /**
        /// Gets log transition probability
         *
        /// @return the probability in the LogMath log domain
         */

        public float LogProbability { get; private set; }


        /// <summary>
        /// Returns a string representation of the arc
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "HSA " + HmmState + " prob " + LogProbability;
        }
    }
}
