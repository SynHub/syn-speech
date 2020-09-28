using System;
using System.Collections.Generic;
using System.Text;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Fsts
{
    /// <summary>
    /// The fst's mutable state implementation.
    /// 
    /// Holds its outgoing {@link edu.cmu.sphinx.fst.Arc} objects in an ArrayList
    /// allowing additions/deletions
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class State
    {
        // State's Id
        protected internal int Id = -1; //Do not make this field readonly.

        // Final weight

        // Outgoing arcs
        private List<Arc> _arcs;

        // initial number of arcs
        public int InitialNumArcs = -1;

        /**
        /// Default Constructor
         */
        protected State() 
        {
            _arcs = new List<Arc>();
        }

        /**
        /// Constructor specifying the state's final weight
        /// 
        /// @param fnlWeight
         */
        public State(float fnlWeight) :this()
        {
            FinalWeight = fnlWeight;
        }

        /**
        /// Constructor specifying the initial capacity of the arc's ArrayList (this
        /// is an optimization used in various operations)
        /// 
        /// @param initialNumArcs
         */
        public State(int initialNumArcs) 
        {
            InitialNumArcs = initialNumArcs;
            if (initialNumArcs > 0) 
            {
                _arcs = new List<Arc>(initialNumArcs);
            }
        }

        /**
        /// Shorts the arc's ArrayList based on the provided Comparator
         */
        public virtual void ArcSort(Comparer<Arc> cmp) 
        {
            _arcs.Sort(cmp);
        }

        /**
        /// Get the state's final Weight
         */

        public float FinalWeight { get; set; }

        /**
        /// Set the state's arcs ArrayList
        /// 
        /// @param arcs the arcs ArrayList to set
         */
        public void SetArcs(List<Arc> arcs) 
        {
            _arcs = arcs;
        }

        /**
        /// Set the state's final weight
        /// 
        /// @param fnlfloat the final weight to set
         */

        /**
        /// Get the state's id
         */
        public int GetId() {
            return Id;
        }

        /**
        /// Get the number of outgoing arcs
         */
        public virtual int GetNumArcs() 
        {
            return _arcs.Count;
        }

        /**
        /// Add an outgoing arc to the state
        /// 
        /// @param arc the arc to add
         */
        public virtual void AddArc(Arc arc) 
        {
            _arcs.Add(arc);
        }

        /**
        /// Get an arc based on it's index the arcs ArrayList
        /// 
        /// @param index the arc's index
        /// @return the arc
         */
        public virtual Arc GetArc(int index) 
        {
            return _arcs[index];
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#equals(java.lang.Object)
         */

        public override bool Equals(Object obj) 
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            var other = (State) obj;
            if (Id != other.Id)
                return false;
            if (!(FinalWeight == other.FinalWeight)) {
                if (Float.FloatToIntBits(FinalWeight) != Float.FloatToIntBits(other.FinalWeight))
                    return false;
            }
            if (_arcs == null) {
                if (other._arcs != null)
                    return false;
            } else if (!Arrays.AreEqual(_arcs,other._arcs))
                return false;
            return true;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#toString()
         */

        public override string ToString() 
        {
            var sb = new StringBuilder();
            sb.Append("(" + Id + ", " + FinalWeight + ")");
            return sb.ToString();
        }

        /**
        /// Delete an arc based on its index
        /// 
        /// @param index the arc's index
        /// @return the deleted arc
         */
        public virtual Arc DeleteArc(int index)
        {
            return _arcs.Remove(index);
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#hashCode()
         */
        public override int GetHashCode() 
        {
            return Id * 991;
        }

        /**
        /// Set an arc at the specified position in the arcs' ArrayList.
        /// 
        /// @param index the position to the arcs' array
        /// @param arc the arc value to set
         */
        public virtual void SetArc(int index, Arc arc) 
        {
            _arcs[index] = arc;
        }

    }
}
