using System;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Represents a hidden-markov-model. An HMM consists of a unit (context dependent or independent), a transition matrix
    /// from state to state, and a sequence of senones associated with each state. This representation of an HMM is a
    /// specialized left-to-right markov model. No backward transitions are allowed.
    /// </summary>
    class SenoneHMM: IHMM
    {
        private static int _objectCount;
        private readonly IHMMState[] _hmmStates;


        /**
        /// Constructs an HMM
         *
        /// @param unit             the unit for this HMM
        /// @param senoneSequence   the sequence of senones for this HMM
        /// @param transitionMatrix the state transition matrix
        /// @param position         the position associated with this HMM
         */
        public SenoneHMM(Unit unit, SenoneSequence senoneSequence,
                         float[][] transitionMatrix, HMMPosition position) 
        {
            Unit = unit;
            SenoneSequence = senoneSequence;
            TransitionMatrix = transitionMatrix;
            Position = position;
            Utilities.ObjectTracker("HMM", _objectCount++);

            _hmmStates = new IHMMState[transitionMatrix.Length];
            for (var i = 0; i < _hmmStates.Length; i++)
            {
                _hmmStates[i] = new SenoneHMMState(this, i);
            }
            // baseUnit = Unit.getUnit(unit.getName());
            BaseUnit = unit.BaseUnit;
        }


        /// <summary>
        /// Gets the  unit associated with this HMM
        /// </summary>
        /// <value>
        /// the unit associated with this HMM
        /// </value>
        public Unit Unit { get; private set; }


        /// <summary>
        /// Gets the  base unit associated with this HMM
        /// </summary>
        /// <value>
        /// the unit associated with this HMM
        /// </value>
        public Unit BaseUnit { get; private set; }

        /// <summary>
        /// Retrieves the hmm state
        /// </summary>
        /// <param name="which"></param>
        /// <returns>
        /// which the state of interest
        /// </returns>
        public IHMMState GetState(int which) 
        {
            return _hmmStates[which];
        }



        /// <summary>
        /// Returns the order of the HMM
        /// </summary>
        /// <value>the order of the HMM</value>
        /// <remarks>
        // NOTE: this method is probably not explicitly needed since
        // getSenoneSequence.getSenones().length will provide the same
        // value, but this is certainly more convenient and easier to
        // understand
        /// </remarks>
        public int Order
        {
            get { return SenoneSequence.Senones.Length; }
        }


        /**
        /// Returns the SenoneSequence associated with this HMM
         *
        /// @return the sequence of senones associated with this HMM. The length of the sequence is N, where N is the order
        ///         of the HMM. Note that senone sequences may be shared among HMMs.
         */
        // [[ NOTE: the senone sequence may in fact be a sequence of
        // composite senones

        public SenoneSequence SenoneSequence { get; private set; }


        /**
        /// Determines if this HMM is a composite HMM
         *
        /// @return true if this is a composite hmm
         */
        public Boolean IsComposite() 
        {
            var senones = SenoneSequence.Senones;
            foreach (var senone in senones) 
            {
                if (senone is CompositeSenone) 
                {
                    return true;
                }
            }
            return false;
        }


        /**
        /// Returns the transition matrix that determines the state transition probabilities for the matrix. Each entry in
        /// the transition matrix defines the probability of transitioning from one state to the next. For example, the
        /// probability of transitioning from state 1 to state 2 can be determined by accessing transition matrix
        /// element[1][2].
         *
        /// @return the transition matrix (in log domain) of size NxN where N is the order of the HMM
         */

        public float[][] TransitionMatrix { get; private set; }


        /**
        /// Returns the transition probability between two states.
         *
        /// @param stateFrom the index of the state this transition goes from
        /// @param stateTo   the index of the state this transition goes to
        /// @return the transition probability (in log domain)
         */
        public float GetTransitionProbability(int stateFrom, int stateTo) 
        {
            return TransitionMatrix[stateFrom][stateTo];
        }


        /**
        /// Retrieves the position of this HMM. Possible
         *
        /// @return the position for this HMM
         */

        public HMMPosition Position { get; private set; }


        /**
        /// Determines if this HMM represents a filler unit. A filler unit is speech that is not meaningful such as a cough,
        /// 'um' , 'er', or silence.
         *
        /// @return true if the HMM  represents a filler unit
         */

        public bool IsFiller
        {
            get { return Unit.IsFiller; }
        }


        /**
        /// Determines if this HMM corresponds to a context dependent unit
         *
        /// @return true if the HMM is context dependent
         */
        public Boolean IsContextDependent() 
        {
            return Unit.IsContextDependent();
        }


        /**
        /// Gets the initial states (with probabilities) for this HMM
         *
        /// @return the set of arcs that transition to the initial states for this HMM
         */
        public IHMMState GetInitialState() 
        {
            return GetState(0);
        }


        /**
        /// Returns the string representation of this object
         *
        /// @return the string representation
         */

        public override string ToString() 
        {
            var name = IsComposite() ? "HMM@" : "HMM";
            return name + '(' + Unit + "):" + (char)Position;
        }


        public override int GetHashCode() 
        {
            return SenoneSequence.GetHashCode();
        }


        public override bool Equals(Object o) 
        {
            if (this == o) {
                return true;
            }
            if (o is SenoneHMM) 
            {
                var other = (SenoneHMM) o;
                return SenoneSequence.Equals(other.SenoneSequence);
            }
            return false;
        }
    }
}
