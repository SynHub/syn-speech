using System;
using System.Runtime.Serialization;
using Syn.Speech.Helper.Mathematics.Linear;

namespace Syn.Speech.Helper.Mathematics.Stat.Descriptive.Moment
{
    
public class SecondMoment : FirstMoment , ISerializable {

    /** Serializable version identifier */
    private const long serialVersionUID = 3942403127395076445L;

    /** second moment of values that have been added */
    public double m2;

    /**
     * Create a SecondMoment instance
     */
    public SecondMoment()
    {
        m2 = Double.NaN;
    }

    /**
     * Copy constructor, creates a new {@code SecondMoment} identical
     * to the {@code original}
     *
     * @param original the {@code SecondMoment} instance to copy
     * @throws NullArgumentException if original is null
     */
    public SecondMoment(SecondMoment original) :base(original) 
    {
        m2 = original.m2;
    }

    /**
     * {@inheritDoc}
     */

    public override void increment(double d) {
        if (n < 1) {
            m1 = m2 = 0.0;
        }
        base.increment(d);
        m2 += ((double) n - 1) * dev * nDev;
    }

    /**
     * {@inheritDoc}
     */

    public override void clear() {
        base.clear();
        m2 = Double.NaN;
    }

    /**
     * {@inheritDoc}
     */
    public override double getResult() {
        return m2;
    }

    /**
     * {@inheritDoc}
     */

    public override UnivariateStatistic copy()//TODO: Supposed to be public override SecondMoment copy()
    {
        SecondMoment result = new SecondMoment();
        // no try-catch or advertised NAE because args are guaranteed non-null
        copy(this, result);
        return result;
    }

    /**
     * Copies source to dest.
     * <p>Neither source nor dest can be null.</p>
     *
     * @param source SecondMoment to copy
     * @param dest SecondMoment to copy to
     * @throws NullArgumentException if either source or dest is null
     */
    public static void copy(SecondMoment source, SecondMoment dest){
        MathUtils.checkNotNull(source);
        MathUtils.checkNotNull(dest);
        FirstMoment.copy(source, dest);
        dest.m2 = source.m2;
    }

}
}
