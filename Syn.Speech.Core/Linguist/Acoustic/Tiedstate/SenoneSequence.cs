using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Syn.Speech.Logging;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Contains an ordered list of senones.
    /// </summary>
    public class SenoneSequence: ISerializable
    {
        /**
        /// a factory method that creates a SeononeSequence from a list of senones.
         *
        /// @param senoneList the list of senones
        /// @return a composite senone
         */
        public static SenoneSequence Create(List<CompositeSenone> senoneList) 
        {
            return new SenoneSequence(senoneList.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SenoneSequence"/> class.
        /// </summary>
        /// <param name="sequence">The ordered set of senones for this sequence.</param>
        public SenoneSequence(ISenone[] sequence) 
        {
            Senones = sequence;
        }


        /// <summary>
        /// Gets the ordered set of senones for this sequence.
        /// </summary>
        /// <value>
        /// The ordered set of senones for this sequence.
        /// </value>
        public ISenone[] Senones { get; private set; }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() 
        {
            var hashCode = 31;
            foreach (var senone in Senones) 
            {
                hashCode = hashCode* 91 + senone.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>true  if the objects are equal.</returns>
        public override bool Equals(Object o) 
        {
            if (this == o) {
                return true;
            } 
            else 
            {
                if (o is SenoneSequence) 
                {
                    var other = (SenoneSequence) o;
                    if (Senones.Length != other.Senones.Length) 
                    {
                        return false;
                    }
                    for (var i = 0; i < Senones.Length; i++) 
                    {
                        if (!Senones[i].Equals(other.Senones[i])) 
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// Dumps this senone sequence
        /// </summary>
        /// <param name="msg">The string annotation.</param>
        public void Dump(String msg) 
        {
            this.LogInfo(" SenoneSequence " + msg + ':');
            foreach (var senone in Senones) 
            {
                senone.Dump("  seq:");
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
