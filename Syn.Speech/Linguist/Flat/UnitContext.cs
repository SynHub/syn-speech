using System;
using Syn.Logging;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    
    /// <summary>
    /// A class that represents a set of units used as a context
    /// </summary>
    public class UnitContext
    {
        private static readonly Cache<UnitContext> UnitContextCache = new Cache<UnitContext>();
        private readonly int _hashCode = 12;
        public static UnitContext Empty = new UnitContext(Unit.EmptyArray);
        public static UnitContext Silence = new UnitContext(new[] { UnitManager.Silence });


        static UnitContext()
        {
            UnitContextCache.cache(Empty);
            UnitContextCache.cache(Silence);
        }

        /// <summary>
        /// Creates a UnitContext for the given context. This constructor is not directly accessible, use the factory method
        /// instead.
        /// </summary>
        /// <param name="context">context the context to wrap with this UnitContext</param>
        private UnitContext(Unit[] context)
        {
            //unitContextCache.cache(EMPTY);
            //unitContextCache.cache(SILENCE);
            Units = context;
            _hashCode = 12;
            for (int i = 0; i < context.Length; i++)
            {
                if (context[i]!=null)
                    _hashCode += context[i].Name.GetHashCode() * ((i + 1) * 34);
            }
           
        }


        /// <summary>
        /// Gets the unit context for the given units. There is a single unit context for each unit combination.
        /// </summary>
        /// <param name="units">the units of interest</param>
        /// <returns>the unit context.</returns>
        public static UnitContext Get(Unit[] units)
        {
            var newUc = new UnitContext(units);
            var cachedUc = UnitContextCache.cache(newUc);
            return cachedUc ?? newUc;
        }

        /// <summary>
        /// Retrieves the units for this context
        /// </summary>
        /// <value>the units associated with this context</value>
        public Unit[] Units { get; private set; }

        /// <summary>
        /// Determines if the given object is equal to this UnitContext
        /// </summary>
        /// <param name="o">the object to compare to</param>
        /// <returns><code>true</code> if the objects are equal</returns>
        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o is UnitContext)
            {
                UnitContext other = (UnitContext)o;
                if (Units.Length != other.Units.Length)
                {
                    return false;
                }
                for (int i = 0; i < Units.Length; i++)
                {
                    if (Units[i] != other.Units[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hashcode for this object
        /// </summary>
        /// <returns>the hashCode</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }


        /// <summary>
        /// Dumps information about the total number of UnitContext objects
        /// </summary>
        public static void DumpInfo()
        {
            Logger.LogInfo<UnitContext>("Total number of UnitContexts : " + UnitContextCache.Misses + " folded: " + UnitContextCache.Hits);
        }


        /// <summary>
        /// Returns a string representation of this object
        /// </summary>
        /// <returns>string representation</returns>
        public override string ToString()
        {
            return LeftRightContext.GetContextName(Units);
        }
        

    }

}
