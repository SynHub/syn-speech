using System;
using System.Runtime.Serialization;

//REFACTORED
namespace Syn.Speech.Fsts.Semirings
{
    /// <summary>
    /// bstract semiring class.
    /// 
    /// @author "John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    [Serializable]
    public abstract class Semiring
    {
        //significant decimal digits in floating point numbers
        protected static int Accuracy = 5;

        /**
        /// Semiring's plus operation
         */
        public abstract float Plus(float w1, float w2);

        public abstract float Reverse(float w1);

        /**
        /// Semiring's times operation
         */
        public abstract float Times(float w1, float w2);

        /**
        /// Semiring's divide operation
         */
        public abstract float Divide(float w1, float w2);

        /**
        /// Semiring's zero element
         */
        public abstract float Zero { get; }

        /**
        /// Semiring's one element
         */
        public abstract float One { get; }

        /**
        /// Checks if a value is a valid one the semiring
         */
        public abstract Boolean IsMember(float w);

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
            return true;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#toString()
         */

        public override string ToString() 
        {
            return GetType().ToString();
        }

        /**
        /// NATURAL ORDER
        /// 
        /// By definition: a <= b iff a + b = a
        /// 
        /// The natural order is a negative partial order iff the semiring is
        /// idempotent. It is trivially monotonic for plus. It is left (resp. right)
        /// monotonic for times iff the semiring is left (resp. right) distributive.
        /// It is a total order iff the semiring has the path property.
        /// 
        /// See Mohri,
        /// "Semiring Framework and Algorithms for Shortest-Distance Problems",
        /// Journal of Automata, Languages and Combinatorics 7(3):321-350, 2002.
        /// 
        /// We define the strict version of this order below.
        /// 
         */
        public Boolean NaturalLess(float w1, float w2) 
        {
            return (Plus(w1, w2) == w1) && (w1 != w2);
        }

    }
}
