using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.machlearn
{
    /// <summary>
    /// An real-valued observation. 
    /// </summary>
    public class OVector: ICloneable, ISerializable
    {
        /// <summary>
         /// Initializes a new instance of the <see cref="OVector"/> class.
         /// Constructs a new observation for a given feature-vector.
         /// </summary>
         /// <param name="values">The values.</param>
        public OVector(double[] values) 
        {
            Values = values;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OVector"/> class. Creates a one-dimensional instance of this class
        /// </summary>
        /// <param name="value">The value.</param>
        public OVector(double value) 
            :this(new[]{value})
        {
            
        }

        /// <summary>
        /// Returns the values of this observation.
        /// </summary>
        /// <value>the values</value>
        public double[] Values { get; private set; }


        /// <summary>
        /// Returns the dimension of this observation.
        /// </summary>
        public int Dimension
        {
            get { return Values.Length; }
        }

        public override bool Equals(object obj) 
        {
            return obj is OVector && Arrays.AreEqual(Values, ((OVector)obj).Values);
        }

        public override int GetHashCode() 
        {
            return Values.GetHashCode();
        }

        public override string ToString() 
        {
            return Values.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
