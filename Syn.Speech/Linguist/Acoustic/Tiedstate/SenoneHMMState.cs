using System;
using System.Collections.Generic;
using Syn.Speech.FrontEnds;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Represents a single state in an HMM
    /// </summary>
    class SenoneHMMState : IHMMState
    {
        private readonly SenoneHMM _hmm;
        HmmStateArc[] _arcs;
        private readonly int _hashCode;

        private static int _objectCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SenoneHMMState"/> class.
        /// </summary>
        /// <param name="hmm">The HMM for this state.</param>
        /// <param name="which">The index for this particular state.</param>
        public SenoneHMMState(SenoneHMM hmm, int which) 
        {
            _hmm = hmm;
            State = which;
            IsEmitting = ((hmm.TransitionMatrix.Length - 1) != State);
            if (IsEmitting) 
            {
                var ss = hmm.SenoneSequence;
                Senone = ss.Senones[State];
            }
            Utilities.ObjectTracker("HMMState", _objectCount++);
            _hashCode = hmm.GetHashCode() + 37* State;
        }

        /// <summary>
        /// Gets the HMM associated with this state
        /// </summary>
        /// <value>
        /// the HMM
        /// </value>
        public IHMM HMM
        {
            get { return _hmm; }
        }

        /// <summary>
        /// Gets the state
        /// </summary>
        /// <value>
        /// the state
        /// </value>
        public int State { get; private set; }


        /// <summary>
        /// Gets the score for this HMM state
        /// </summary>
        /// <param name="feature">The feature to be scored.</param>
        /// <returns>The acoustic score for this state.</returns>
        public float GetScore(IData feature) 
        {
            return Senone.GetScore(feature);
        }


        /// <summary>
        /// Gets the scores for each mixture component in this HMM state
        /// </summary>
        /// <param name="feature">The feature to be scored.</param>
        /// <returns>The acoustic scores for the components of this state.</returns>
        public float[] CalculateComponentScore(IData feature) 
        {
            var unknownCall = _hmm.SenoneSequence;
            return Senone.CalculateComponentScore(feature);
        }

        /// <summary>
        /// Gets the senone for this HMM state
        /// </summary>
        /// <value>
        /// The senone for this state.
        /// </value>
        public ISenone Senone { get; private set; }


        /// <summary>
        /// Determines if two HMMStates are equal
        /// </summary>
        /// <param name="other">The state to compare this one to.</param>
        /// <returns>true if the states are equal.</returns>
        public override bool Equals(Object other) 
        {
            if (this == other) {
                return true;
            } 
            else if (!(other is SenoneHMMState)) 
            {
                return false;
            } else {
                var otherState = (SenoneHMMState) other;
                return _hmm == otherState._hmm &&
                        State == otherState.State;
            }
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() 
        {
            return _hashCode;
        }


        /// <summary>
        /// Determines if this HMMState is an emitting state
        /// </summary>
        /// <value>
        /// true if the state is an emitting state
        /// </value>
        /// TODO: We may have non-emitting entry states as well.
        public bool IsEmitting { get; private set; }


        /// <summary>
        /// Retrieves the state of successor states for this state.
        /// </summary>
        /// <returns>
        /// The set of successor state arcs.
        /// </returns>
        public HmmStateArc[] GetSuccessors() 
        {
            if (_arcs == null) {
                var list = new List<HmmStateArc>();
                var transitionMatrix = _hmm.TransitionMatrix;

                for (var i = 0; i < transitionMatrix.Length; i++) 
                {
                    if (transitionMatrix[State][i] > LogMath.LogZero) 
                    {
                        var arc = new HmmStateArc(_hmm.GetState(i),
                                transitionMatrix[State][i]);
                        list.Add(arc);
                    }
                }
                _arcs = list.ToArray();
            }
            return _arcs;
        }


 
        /// <summary>
        /// Determines if this state is an exit state of the HMM.
        /// </summary>
        /// <returns>
        /// true if the state is an exit state.
        /// </returns>
        public Boolean IsExitState() 
        {
            return !IsEmitting;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() 
        {
            return "HMMS " + _hmm + " state " + State;
        }

	    public MixtureComponent[] GetMixtureComponents() {
		    return Senone.MixtureComponents;
	    }

	    public long GetMixtureId() {
		    return Senone.ID;
	    }

	    public float[] GetLogMixtureWeights() {
		    return Senone.GetLogMixtureWeights();
	    }
    }
}
