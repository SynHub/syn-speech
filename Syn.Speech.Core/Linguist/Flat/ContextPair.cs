using System;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// A context pair hold a left and starting context. It is used as a hash into the set of starting points for a
    /// particular gstate
    /// </summary>
    class ContextPair
    {

        static readonly Cache<ContextPair> ContextPairCache = new Cache<ContextPair>();
        private readonly int _hashCode;


        /**
           /// Creates a UnitContext for the given context. This constructor is not directly accessible, use the factory method
           /// instead.
            *
           /// @param left  the left context
           /// @param right the right context
            */
        private ContextPair(UnitContext left, UnitContext right)
        {
            LeftContext = left;
            RightContext = right;
            _hashCode = 99 + left.GetHashCode()* 113 + right.GetHashCode();
        }


        /**
           /// Gets the ContextPair for the given set of contexts. This is a factory method. If the ContextPair already exists,
           /// return that one, otherwise, create it and store it so it can be reused.
            *
           /// @param left  the left context
           /// @param right the right context
           /// @return the unit context.
            */
        public static ContextPair Get(UnitContext left, UnitContext right)
        {
            var newCp = new ContextPair(left, right);
            var cachedCp = ContextPairCache.cache(newCp);
            return cachedCp ?? newCp;
        }


        /**
           /// Determines if the given object is equal to this UnitContext
            *
           /// @param o the object to compare to
           /// @return <code>true</code> if the objects are equal return;
            */
        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            else if (o is ContextPair)
            {
                ContextPair other = (ContextPair)o;
                return LeftContext.Equals(other.LeftContext) && RightContext.Equals(other.RightContext);
            }
            return false;
        }

        public bool Equals(ContextPair o)
        {
            if (this == o)
            {
                return true;
            }
            return LeftContext.Equals(o.LeftContext) && RightContext.Equals(o.RightContext);
        }

        /**
           /// Returns a hashcode for this object
            *
           /// @return the hashCode
            */
        public override int GetHashCode()
        {
            return _hashCode;
        }


        /**
           /// Returns a string representation of the object
            */
        public override string ToString()
        {
            return "CP left: " + LeftContext + " right: " + RightContext;
        }


        /**
           /// Gets the left unit context
            *
           /// @return the left unit context
            */

        public UnitContext LeftContext { get; private set; }


        /**
           /// Gets the right unit context
            *
           /// @return the right unit context
            */

        public UnitContext RightContext { get; private set; }
    }
    
}
