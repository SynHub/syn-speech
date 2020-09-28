using System;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents a unit of speech. Units may represent phones, words or any other suitable unit
    /// </summary>
    public class Unit
    {
        public static Unit[] EmptyArray = new Unit[0];

        private readonly string _name = String.Empty;
        private readonly int _baseID=-1;

        private volatile string _key = String.Empty;

        /// <summary>
        /// Constructs a context independent unit. Constructors are package private, use the UnitManager to create and access
        /// units.
        /// </summary>
        /// <param name="name">the name of the unit</param>
        /// <param name="filler"><code>true</code> if the unit is a filler unit</param>
        /// <param name="id">the base id for the unit</param>
        public Unit(String name, Boolean filler, int id)
        {
            _name = name;
            IsFiller = filler;
            IsSilence = name.Equals(UnitManager.SilenceName);
            _baseID = id;
            BaseUnit = this;
            Context = Context.EmptyContext;
        }

        /// <summary>
        /// Constructs a context independent unit. Constructors are package private, use the UnitManager to create and access
        /// units.
        /// </summary>
        /// <param name="baseUnit">the base id for the unit</param>
        /// <param name="filler"><code>true</code> if the unit is a filler unit</param>
        /// <param name="context"></param>
        public Unit(Unit baseUnit, Boolean filler, Context context)
        {
            _name = baseUnit.Name;
            IsFiller = filler;
            IsSilence = _name.Equals(UnitManager.SilenceName);
            _baseID = baseUnit.BaseID;
            BaseUnit = baseUnit;
            Context = context;
        }

        /// <summary>
        /// Gets the name for this unit
        /// </summary>
        /// <value>the name for this unit</value>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Determines if this unit is a filler unit
        /// </summary>
        /// <value></value>
        public bool IsFiller { get; private set; }

        /// <summary>
        /// Determines if this unit is the silence unit
        /// </summary>
        /// <value></value>
        public bool IsSilence { get; private set; }

        /// <summary>
        /// Gets the  base unit id associated with this HMM
        /// </summary>
        /// <value></value>
        public int BaseID
        {
            get { return _baseID; }
        }

        /// <summary>
        /// Gets the  base unit associated with this HMM
        /// </summary>
        /// <value></value>
        public Unit BaseUnit { get; private set; }

        /// <summary>
        /// Returns the context for this unit
        /// </summary>
        /// <value></value>
        public Context Context { get; private set; }

        /// <summary>
        /// Determines if this unit is context dependent
        /// </summary>
        /// <returns></returns>
        public Boolean IsContextDependent() {
            return Context != Context.EmptyContext;
        }
        /// <summary>
        /// gets the key for this unit
        /// </summary>
        /// <returns></returns>
        public string GetKey() 
        {
            return ToString();
        }

        /// <summary>
        /// Checks to see of an object is equal to this unit
        /// </summary>
        /// <param name="o">the object to check</param>
        /// <returns>true if the objects are equal</returns>
        public override bool Equals(Object o)
        {
            if (this == o) 
            {
                return true;
            }
            if (o is Unit) 
            {
                var otherUnit = (Unit) o;
                return GetKey().Equals(otherUnit.GetKey());
            }
            return false;
        }

        /// <summary>
        /// calculates a hashCode for this unit. Since we defined an equals for Unit, we must define a hashCode as well
        /// </summary>
        /// <returns>the hashcode for this object</returns>
        public override int GetHashCode() 
        {
            return GetKey().GetHashCode();
        }
        /// <summary>
        /// Converts to a string
        /// </summary>
        /// <returns>string version</returns>
        public override string ToString() 
        {
            if (_key != null) 
            {
                if (Context == Context.EmptyContext) 
                {
                    _key = (IsFiller ? "*" : "") + _name;
                } 
                else {
                    _key = (IsFiller ? "*" : "") + _name + '[' + Context + ']';
                }
            }
            return _key;
        }
        /// <summary>
        /// Checks to see if the given unit with associated contexts is a partial match for this unit.   Zero, One or both
        /// contexts can be null. A null context matches any context
        /// </summary>
        /// <param name="name">the name of the unit</param>
        /// <param name="context">the  context to match against</param>
        /// <returns>true if this unit matches the name and non-null context</returns>
        public Boolean IsPartialMatch(String name, Context context) 
        {    
            return Name.Equals(name) && context.IsPartialMatch(Context);
        }

        /// <summary>
        /// Creates and returns an empty context with the given size. The context is padded with SIL filler
        /// </summary>
        /// <param name="size">the size of the context</param>
        /// <returns>the context</returns>
        public static Unit[] GetEmptyContext(int size) 
        {
            var context = new Unit[size];
            Arrays.Fill(context,UnitManager.Silence);
            return context;
        }


        /// <summary>
        /// Checks to see that there is 100% overlap in the given contexts
        /// </summary>
        /// <param name="a">context to check for a match</param>
        /// <param name="b">context to check for a match</param>
        /// <returns><code>true</code> if the contexts match</returns>
        public static Boolean IsContextMatch(Unit[] a, Unit[] b)
        {
            if (a == null || b == null) 
            {
                return a == b;
            }
            if (a.Length != b.Length) 
            {
                return false;
            }
            for (var i = 0; i < a.Length; i++) 
            {
                if (!a[i].Name.Equals(b[i].Name)) 
                {
                    return false;
                }
            }
            return true;
        }
    }
}
