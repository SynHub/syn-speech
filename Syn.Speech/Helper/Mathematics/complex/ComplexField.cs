using System;
using System.Runtime.Serialization;

namespace Syn.Speech.Helper.Mathematics.complex
{
    
public class ComplexField : Field<Complex>, ISerializable  {



    /** Private constructor for the singleton.
     */
    private ComplexField() {
    }

    /** Get the unique instance.
     * @return the unique instance
     */
    public static ComplexField getInstance() {
        return LazyHolder.INSTANCE;
    }

    /** {@inheritDoc} */
    public Complex getOne() {
        return Complex.ONE;
    }

    /** {@inheritDoc} */
    public Complex getZero() {
        return Complex.ZERO;
    }

    /** {@inheritDoc} */
    public Type getRuntimeClass()
    {
        return typeof (Complex); //TODO: Check behaviour
    }

    // CHECKSTYLE: stop HideUtilityClassConstructor
    /** Holder for the instance.
     * <p>We use here the Initialization On Demand Holder Idiom.</p>
     */

    static class LazyHolder {
        /** Cached field instance. */
        public static readonly ComplexField INSTANCE = new ComplexField();
    }
    // CHECKSTYLE: resume HideUtilityClassConstructor

    /** Handle deserialization of the singleton.
     * @return the singleton instance
     */
    private Object readResolve() {
        // return the singleton instance
        return LazyHolder.INSTANCE;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }
}
}
