using System;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a transition in a sentence HMM. Each transition is described by the next state and the associated acoustic
    /// and language probability for the transition.
    /// 
    /// All probabilities are in the LogMath log domain
    /// </summary>
    public class SentenceHMMStateArc : ISearchStateArc
    {
        private readonly SentenceHMMState _nextState;
        private readonly int _hashCode;


        /**
        /// Creates a SentenceHMMStateArc
         *
        /// @param nextState               the next state
        /// @param logLanguageProbability  the log language probability
        /// @param logInsertionProbability the log insertion probability
         */
        public SentenceHMMStateArc(SentenceHMMState nextState,
                                   float logLanguageProbability,
                                   float logInsertionProbability) 
        {
            _nextState = nextState;
            LanguageProbability = logLanguageProbability;
            InsertionProbability = logInsertionProbability;

            _hashCode = 111 + nextState.GetHashCode() +
                    17* Float.FloatToIntBits(logLanguageProbability) +
                    23 * Float.FloatToIntBits(logInsertionProbability);

        }


        /**
        /// Determines if the given object is equal to this object
         *
        /// @param o the object to compare to
        /// @return <code>true</code> if the objects are equal
         */
        override  public bool Equals(Object o)
        {
            if (this == o) 
            {
                return true;
            }
            if (o is SentenceHMMStateArc) 
            {
                var other = (SentenceHMMStateArc) o;
                return _nextState == other._nextState &&
                       LanguageProbability == other.LanguageProbability &&
                       InsertionProbability == other.InsertionProbability;

            }
            return false;
        }


        /**
        /// Returns a hashCode for this object
         *
        /// @return the hashCode
         */
        public override int GetHashCode() 
        {
            return _hashCode;
        }


        /**
        /// Retrieves the next state
         *
        /// @return the next state
         */

        public ISearchState State
        {
            get { return _nextState; }
        }


        /**
        /// For backwards compatibility
        /// <p/>
        /// Returns the next state as a SentenceHMSMtate
         *
        /// @return the next state
         */
        public SentenceHMMState GetNextState() 
        {
            return (SentenceHMMState) State;
        }


        /**
        /// Retrieves the language transition probability for this transition
         *
        /// @return the language  transition probability in the logmath log domain
         */

        public float LanguageProbability { get; private set; }


        /**
        /// Retrieves the insertion probability for this transition
         *
        /// @return the insertion probability  in the logmath log domain
         */

        public float InsertionProbability { get; private set; }


        /**
        /// Gets the composite probability of entering this state
         *
        /// @return the log probability
         */
        public float GetProbability() 
        {
            return LanguageProbability + InsertionProbability;
        }

    }
}
