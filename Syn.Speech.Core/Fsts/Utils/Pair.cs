using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Fsts.Utils
{
    /// <summary>
    /// Pairs two elements
    /// 
    /// Original code obtained by
    /// http://stackoverflow.com/questions/521171/a-java-collection-of-value
    /// -pairs-tuples
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Pair<TL, TR>
    {

        // The left element
        private TL _left;

        // The right element
        private TR _right;

        /**
        /// Constructor specifying the left and right elements of the Pair.
         */
        public Pair(TL left, TR right)
        {
            _left = left;
            _right = right;
        }

        /**
        /// Set the left element of the Pair
         */
        public void SetLeft(TL left)
        {
            _left = left;
        }

        /**
        /// Set the right element of the Pair
         */
        public void SetRight(TR right)
        {
            _right = right;
        }

        /**
        /// Get the left element of the Pair
         */
        public TL GetLeft()
        {
            return _left;
        }

        /**
        /// Get the right element of the Pair
         */
        public TR GetRight()
        {
            return _right;
        }

        public override int GetHashCode()
        {
            var prime = 31;
            var result = 1;
            result = prime* result + _left.GetHashCode();
            result = prime* result + _right.GetHashCode();
            return result;
        }


        public override bool Equals(Object obj) 
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            var other = (Pair<TL,TR>) obj;
            if (!_left.Equals(other._left))
                return false;
            if (!_right.Equals(other._right))
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
            return "(" + _left + ", " + _right + ")";
        }
    }
}
