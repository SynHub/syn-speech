using System;
using System.Runtime.Serialization;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents  the context for a unit
    /// </summary>
    [Serializable]
    public class Context: ISerializable
    {
        /// <summary>
        /// Represents an empty context
        /// </summary>
        public static Context EmptyContext = new Context();

        /// <summary>
        /// No instantiations allowed
        /// </summary>
        protected Context()
        {
        }
        /// <summary>
        /// Checks to see if there is a partial match with the given context. For a simple context such as this we always
        /// match.
        /// </summary>
        /// <param name="context">the context to check</param>
        /// <returns>true if there is a partial match</returns>
        public virtual Boolean IsPartialMatch(Context context)
        {
            return true;
        }
        /// <summary>
        /// Provides a string representation of a context
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "";
        }

        /// <summary>
        /// Determines if an object is equal to this context
        /// </summary>
        /// <param name="o">the object to check</param>
        /// <returns>true if the objects are equal</returns>
        public override bool Equals(Object o) 
        {
            if (this == o) 
            {
                return true;
            } 
            else 
                if (o is Context) 
                {
                    var otherContext = (Context) o;
                    return ToString().Equals(otherContext.ToString());

                } 
                else 
                {
                    return false;
                }
        }
        /// <summary>
        /// calculates a hashCode for this context. Since we defined an equals for context, we must define a hashCode as
        /// well
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
