using System;
using System.Diagnostics;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Fsts
{
    /// <summary>
    /// The fst's arc implementation.
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Arc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Arc"/> class.
        /// </summary>
        public Arc() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arc"/> class.
        /// </summary>
        /// <param name="iLabel">The input label's id.</param>
        /// <param name="oLabel">The output label's id.</param>
        /// <param name="weight">The arc's weight.</param>
        /// <param name="nextState">The arc's next state.</param>
        public Arc(int iLabel, int oLabel, float weight, State nextState) 
        {
            this.Weight = weight;
            this.Ilabel = iLabel;
            this.Olabel = oLabel;
            this.NextState = nextState;
        }

        /// <summary>
        /// Get the arc's weight
        /// </summary>
        /// <value></value>
        public float Weight { get;  set; }

        /// <summary>
        /// Gets or sets the input label's id.
        /// </summary>
        /// <value>
        /// The ilabel.
        /// </value>
        public int Ilabel { get; set; }

        /// <summary>
        /// Gets or sets the output label's id.
        /// </summary>
        /// <value>
        /// The olabel.
        /// </value>
        public int Olabel { get; set; }

        /// <summary>
        /// Gets or sets the next state
        /// </summary>
        /// <value>
        /// The state of the next.
        /// </value>
        public State NextState { get; set; }


        public override bool Equals(Object obj)
        {
            //Note: NOT performance critical
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            Arc other = (Arc) obj;
            if (Ilabel != other.Ilabel)
                return false;
            if (NextState == null) {
                if (other.NextState != null)
                    return false;
            } else if (NextState.GetId() != other.NextState.GetId())
                return false;
            if (Olabel != other.Olabel)
                return false;
            if (!(Weight == other.Weight)) 
            {
                if (Float.FloatToIntBits(Weight) != Float.FloatToIntBits(other.Weight))
                    return false;
            }
            return true;
        }

        public override string ToString() 
        {
            return "(" + Ilabel + ", " + Olabel + ", " + Weight + ", " + NextState
                    + ")";
        }
    }
}
