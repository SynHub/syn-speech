using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Fsts
{
    /// <summary>
    /// The fst's immutable state implementation.
    /// 
    /// holds its outgoing {@link edu.cmu.sphinx.fst.Arc} objects in a fixed size
    /// array not allowing additions/deletions.
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class ImmutableState: State
    {
        // Outgoing arcs
        private Arc[] _arcs;

        /**
        /// Default protected constructor.
        /// 
        /// An ImmutableState cannot be created directly. It needs to be deserialized
        /// as part of an ImmutableFst.
        /// 
        /// @see edu.cmu.sphinx.fst.ImmutableFst#loadModel(String)
        /// 
         */
        protected ImmutableState() {
        }

        /**
        /// Constructor specifying the capacity of the arcs array.
        /// 
        /// @param numArcs
         */
        public ImmutableState(int numArcs)  :base(0)
        {
            InitialNumArcs = numArcs;
            _arcs = new Arc[numArcs];
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.State#arcSort(java.util.Comparator)
         */
        public override void ArcSort(Comparer<Arc> cmp) 
        {
            Array.Sort(_arcs,cmp);
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.State#addArc(edu.cmu.sphinx.fst.Arc)
         */
        public override void AddArc(Arc arc) {
            throw new InvalidOperationException("You cannot modify an ImmutableState.");
        }

        /**
        /// Set an arc at the specified position in the arcs' array.
        /// 
        /// @param index the position to the arcs' array
        /// @param arc the arc value to set
         */
        public override void SetArc(int index, Arc arc) 
        {
            _arcs[index] = arc;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.State#deleteArc(int)
         */
        public override Arc DeleteArc(int index) 
        {
            throw new InvalidOperationException("You cannot modify an ImmutableState.");
        }

        /**
        /// Set the state's arcs array
        /// 
        /// @param arcs the arcs array to set
         */
        public void SetArcs(Arc[] arcs) 
        {
            this._arcs = arcs;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.State#getNumArcs()
         */
        public override int GetNumArcs() 
        {
            return InitialNumArcs;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.State#getArc(int)
         */
        public override Arc GetArc(int index) 
        {
            return _arcs[index];
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#hashCode()
         */
        public override int GetHashCode() 
        {
            const int prime = 31;
            var result = 1;
            result = prime* result + Id;
            return result;
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
            if (GetType() != obj.GetType())
                return false;
            var other = (ImmutableState) obj;
            if (!Arrays.AreEqual(_arcs, other._arcs))
                return false;
            if (!base.Equals(obj))
                return false;
            return true;
        }

    }
}
